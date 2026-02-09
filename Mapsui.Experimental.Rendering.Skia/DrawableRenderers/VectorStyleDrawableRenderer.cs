using Mapsui.Experimental.Rendering.Skia.Drawables;
using Mapsui.Experimental.Rendering.Skia.Extensions;
using Mapsui.Experimental.Rendering.Skia.SkiaStyles;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Nts;
using Mapsui.Rendering;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Mapsui.Experimental.Rendering.Skia.DrawableRenderers;

/// <summary>
/// Creates and draws VectorStyle drawables using the two-step architecture.
/// Step 1 (CreateDrawables): Creates SKPath objects in world coordinates and SKPaint objects
/// on a background thread. Points delegate to SymbolStyleDrawableRenderer.
/// Step 2 (DrawDrawable): Builds a world-to-screen matrix, clones and transforms the path,
/// then draws with pre-created paints. The path clone + native matrix transform is fast.
/// </summary>
public class VectorStyleDrawableRenderer : IDrawableStyleRenderer
{
    /// <summary>
    /// Fill paint scale for non-solid fill patterns (Cross, Dotted, etc.).
    /// Must match PolygonRenderer._scale.
    /// </summary>
    private const float Scale = 10.0f;

    public IReadOnlyList<IDrawable> CreateDrawables(Viewport viewport, ILayer layer, IFeature feature,
        IStyle style, RenderService renderService)
    {
        if (style is not VectorStyle vectorStyle)
            throw new ArgumentException($"Expected {nameof(VectorStyle)} but got {style?.GetType().Name}");

        var opacity = (float)(layer.Opacity * style.Opacity);
        var drawables = new List<IDrawable>();

        try
        {
            switch (feature)
            {
                case PointFeature pointFeature:
                    drawables.Add(CreatePointDrawable(pointFeature.Point.X, pointFeature.Point.Y, vectorStyle, opacity));
                    break;
                case GeometryFeature geometryFeature:
                    CreateGeometryDrawables(drawables, geometryFeature.Geometry, vectorStyle, opacity, viewport, renderService);
                    break;
                default:
                    Logger.Log(LogLevel.Warning, $"{nameof(VectorStyleDrawableRenderer)} can not render feature of type '{feature.GetType()}'");
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }

        return drawables;
    }

    public void DrawDrawable(object canvas, Viewport viewport, IDrawable drawable, ILayer layer)
    {
        if (canvas is not SKCanvas skCanvas)
            throw new ArgumentException($"Expected {nameof(SKCanvas)} but got {canvas?.GetType().Name}");

        switch (drawable)
        {
            case SymbolStyleDrawable symbolDrawable:
                SymbolStyleDrawableRenderer.DrawSymbolDrawable(skCanvas, viewport, symbolDrawable);
                break;
            case VectorStyleDrawable vectorDrawable:
                DrawVectorDrawable(skCanvas, viewport, vectorDrawable);
                break;
        }
    }

    private static void CreateGeometryDrawables(List<IDrawable> drawables, Geometry? geometry,
        VectorStyle vectorStyle, float opacity, Viewport viewport, RenderService renderService, int position = 0)
    {
        switch (geometry)
        {
            case GeometryCollection collection:
                for (var i = 0; i < collection.Count; i++)
                    CreateGeometryDrawables(drawables, collection[i], vectorStyle, opacity, viewport, renderService, i);
                break;
            case Point point:
                drawables.Add(CreatePointDrawable(point.X, point.Y, vectorStyle, opacity));
                break;
            case Polygon polygon:
                drawables.Add(CreatePolygonDrawable(polygon, vectorStyle, opacity, viewport, renderService));
                break;
            case LineString lineString:
                drawables.Add(CreateLineStringDrawable(lineString, vectorStyle, opacity));
                break;
            case null:
                break;
            default:
                Logger.Log(LogLevel.Warning, $"Unknown geometry type: {geometry.GetType()}");
                break;
        }
    }

    private static SymbolStyleDrawable CreatePointDrawable(double worldX, double worldY,
        VectorStyle vectorStyle, float opacity)
    {
        // Convert VectorStyle to SymbolStyle for point rendering (same as legacy VectorStyleRenderer)
        var symbolStyle = new SymbolStyle { Outline = vectorStyle.Outline, Fill = vectorStyle.Fill, Line = vectorStyle.Line };
        return SymbolStyleDrawableRenderer.CreateSymbolDrawable(worldX, worldY, symbolStyle, opacity);
    }

    private static VectorStyleDrawable CreatePolygonDrawable(Polygon polygon, VectorStyle vectorStyle,
        float opacity, Viewport viewport, RenderService renderService)
    {
        // Create path in world coordinates (viewport-independent)
        var worldPath = polygon.ToWorldPath();

        // Centroid for IDrawable.WorldX/WorldY
        var envelope = polygon.EnvelopeInternal;
        var centroidX = (envelope.MinX + envelope.MaxX) / 2.0;
        var centroidY = (envelope.MinY + envelope.MaxY) / 2.0;

        // Create fill paint if visible
        SKPaint? fillPaint = null;
        var fillStyle = vectorStyle.Fill?.FillStyle ?? FillStyle.Solid;
        if (vectorStyle.Fill.IsVisible())
        {
            fillPaint = PolygonRenderer.CreateSkPaint((vectorStyle.Fill, opacity, viewport.Rotation), renderService);
        }

        // Create outline paint if visible
        SKPaint? outlinePaint = null;
        if (vectorStyle.Outline.IsVisible())
        {
            outlinePaint = PolygonRenderer.CreateSkPaint((vectorStyle.Outline, opacity));
        }

        return new VectorStyleDrawable(centroidX, centroidY, worldPath, fillPaint, outlinePaint, linePaint: null, fillStyle);
    }

    private static VectorStyleDrawable CreateLineStringDrawable(LineString lineString,
        VectorStyle vectorStyle, float opacity)
    {
        // Create path in world coordinates (viewport-independent)
        var worldPath = lineString.ToWorldPath();

        // Centroid for IDrawable.WorldX/WorldY
        var envelope = lineString.EnvelopeInternal;
        var centroidX = (envelope.MinX + envelope.MaxX) / 2.0;
        var centroidY = (envelope.MinY + envelope.MaxY) / 2.0;

        // Create outline paint if visible (drawn first, as border around line)
        SKPaint? outlinePaint = null;
        if (vectorStyle.Line.IsVisible() && vectorStyle.Outline?.Width > 0)
        {
            var width = vectorStyle.Outline.Width + vectorStyle.Outline.Width + vectorStyle.Line?.Width ?? 1;
            outlinePaint = LineStringRenderer.CreateSkPaint((vectorStyle.Outline, (float?)width, opacity));
        }

        // Create line paint if visible
        SKPaint? linePaint = null;
        if (vectorStyle.Line.IsVisible())
        {
            linePaint = LineStringRenderer.CreateSkPaint((vectorStyle.Line, (float?)null, opacity));
        }

        return new VectorStyleDrawable(centroidX, centroidY, worldPath, fillPaint: null, outlinePaint, linePaint, FillStyle.Solid);
    }

    private static void DrawVectorDrawable(SKCanvas canvas, Viewport viewport, VectorStyleDrawable drawable)
    {
        // Build the world-to-screen transformation matrix
        var matrix = CreateWorldToScreenMatrix(viewport);

        // Clone the world path and transform it to screen coordinates.
        // This is a native SkiaSharp operation — much faster than per-vertex managed math.
        // All paints remain in pixel/screen units, so no stroke/pattern adjustments needed.
        using var screenPath = new SKPath(drawable.WorldPath);
        screenPath.Transform(matrix);

        // Draw fill (polygon only) — must come first, outline goes on top
        if (drawable.FillPaint is not null)
        {
            DrawFillPath(canvas, screenPath, drawable.FillPaint, drawable.FillStyle);
        }

        // Draw outline (polygon outline or linestring outline — for linestring, drawn under line)
        if (drawable.OutlinePaint is not null)
        {
            canvas.DrawPath(screenPath, drawable.OutlinePaint);
        }

        // Draw line (linestring only — drawn on top of outline)
        if (drawable.LinePaint is not null)
        {
            canvas.DrawPath(screenPath, drawable.LinePaint);
        }
    }

    /// <summary>
    /// Draws a filled path, handling both solid fills and patterned fills (Cross, Dotted, etc.).
    /// For non-solid fills, clips the canvas to the path and draws a larger rect with the pattern paint.
    /// </summary>
    private static void DrawFillPath(SKCanvas canvas, SKPath path, SKPaint fillPaint, FillStyle fillStyle)
    {
        if (fillStyle == FillStyle.Solid)
        {
            canvas.DrawPath(path, fillPaint);
        }
        else
        {
            // For non-solid fills, clip to the path and draw a larger rect so the pattern fills completely
            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.ClipPath(path);
                var bounds = path.Bounds;
                var inflate = ((int)bounds.Width * 0.3f / Scale) * Scale;
                bounds.Inflate(inflate, inflate);
                canvas.DrawRect(bounds, fillPaint);
            }
        }
    }

    /// <summary>
    /// Creates an SKMatrix that transforms world coordinates to screen coordinates.
    /// The matrix handles: translation to center, scaling by 1/resolution, Y-axis flip,
    /// and optional viewport rotation around the screen center.
    /// </summary>
    internal static SKMatrix CreateWorldToScreenMatrix(Viewport viewport)
    {
        var res = (float)viewport.Resolution;
        var centerX = (float)viewport.CenterX;
        var centerY = (float)viewport.CenterY;
        var screenCenterX = (float)(viewport.Width / 2.0);
        var screenCenterY = (float)(viewport.Height / 2.0);

        // The unrotated world-to-screen transform:
        //   screenX = (worldX - centerX) / resolution + screenCenterX
        //   screenY = (centerY - worldY) / resolution + screenCenterY
        var matrix = new SKMatrix(
            scaleX: 1f / res,
            skewX: 0,
            transX: -centerX / res + screenCenterX,
            skewY: 0,
            scaleY: -1f / res,
            transY: centerY / res + screenCenterY,
            persp0: 0,
            persp1: 0,
            persp2: 1
        );

        if (viewport.Rotation != 0)
        {
            // Rotate around the screen center, matching ViewportExtensions.WorldToScreenXY behavior.
            // Skia's CreateRotationDegrees with positive angle in screen coords (Y-down)
            // matches the clockwise rotation by -viewport.Rotation used in WorldToScreenXY.
            var rotateAroundCenter = SKMatrix.CreateRotationDegrees((float)viewport.Rotation, screenCenterX, screenCenterY);
            matrix = SKMatrix.Concat(rotateAroundCenter, matrix);
        }

        return matrix;
    }
}
