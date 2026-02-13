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
/// Two-step renderer for VectorStyle. No cache interaction inside the renderer —
/// caching is managed externally by the orchestrator.
/// <list type="bullet">
///   <item><description><see cref="CreateDrawables"/>: Creates SKPath objects in world coordinates
///         (expensive, runs on background thread).</description></item>
///   <item><description><see cref="DrawDrawable"/>: Transforms and draws a single pre-created drawable
///         (fast, render thread).</description></item>
/// </list>
/// </summary>
public class TwoStepVectorStyleRenderer : ITwoStepStyleRenderer
{

    /// <inheritdoc />
    public IDrawableCache CreateCache() => new DrawableCache();

    /// <inheritdoc />
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
                    Logger.Log(LogLevel.Warning, $"{nameof(TwoStepVectorStyleRenderer)} can not render feature of type '{feature.GetType()}'");
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }

        return drawables;
    }

    /// <inheritdoc />
    public void DrawDrawable(object canvas, Viewport viewport, IDrawable drawable, ILayer layer)
    {
        if (canvas is not SKCanvas skCanvas)
            return;

        switch (drawable)
        {
            case SymbolStyleDrawable symbolDrawable:
                TwoStepSymbolStyleRenderer.DrawSymbolDrawable(skCanvas, viewport, symbolDrawable);
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
        return TwoStepSymbolStyleRenderer.CreateSymbolDrawable(worldX, worldY, symbolStyle, opacity);
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

        var fillStyle = vectorStyle.Fill?.FillStyle ?? FillStyle.Solid;

        // Pre-extract SKImage for bitmap fills (cache-owned, safe to store)
        SKImage? bitmapFillImage = null;
        if (fillStyle is FillStyle.Bitmap or FillStyle.BitmapRotated && vectorStyle.Fill?.Image is not null)
        {
            bitmapFillImage = PolygonRenderer.GetSKImage(renderService, vectorStyle.Fill.Image);
        }

        return new VectorStyleDrawable(
            centroidX, centroidY, worldPath,
            brush: vectorStyle.Fill.IsVisible() ? vectorStyle.Fill : null,
            fillOpacity: opacity,
            viewportRotation: viewport.Rotation,
            bitmapFillImage: bitmapFillImage,
            outlinePen: vectorStyle.Outline.IsVisible() ? vectorStyle.Outline : null,
            outlineWidthOverride: null,
            outlineOpacity: opacity,
            linePen: null,
            lineOpacity: 0,
            fillStyle: fillStyle);
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

        // Compute outline width override if both line and outline are visible
        float? outlineWidthOverride = null;
        Pen? outlinePen = null;
        if (vectorStyle.Line.IsVisible() && vectorStyle.Outline?.Width > 0)
        {
            outlineWidthOverride = (float)(vectorStyle.Outline.Width + vectorStyle.Outline.Width + (vectorStyle.Line?.Width ?? 1));
            outlinePen = vectorStyle.Outline;
        }

        return new VectorStyleDrawable(
            centroidX, centroidY, worldPath,
            brush: null,
            fillOpacity: 0,
            viewportRotation: 0,
            bitmapFillImage: null,
            outlinePen: outlinePen,
            outlineWidthOverride: outlineWidthOverride,
            outlineOpacity: opacity,
            linePen: vectorStyle.Line.IsVisible() ? vectorStyle.Line : null,
            lineOpacity: opacity,
            fillStyle: FillStyle.Solid);
    }

    private static void DrawVectorDrawable(SKCanvas canvas, Viewport viewport, VectorStyleDrawable drawable)
    {
        // Build the world-to-screen transformation matrix
        var matrix = CreateWorldToScreenMatrix(viewport);

        // Concat the matrix onto the canvas so the GPU applies the world-to-screen
        // transform directly. Using Concat (not SetMatrix) preserves any existing
        // canvas transformation (pixel density, layout offset, etc.).
        //
        // The canvas matrix scales geometry by 1/resolution. Stroke widths would be
        // scaled down by the matrix, so we compensate by multiplying with resolution.
        // All SKPaint objects are created locally (no cached native objects) to avoid
        // lifecycle issues between background and UI threads.
        var res = (float)viewport.Resolution;

        using (new SKAutoCanvasRestore(canvas))
        {
            canvas.Concat(matrix);

            // Draw fill (polygon only) — must come first, outline goes on top
            if (drawable.Brush is not null)
            {
                using var fillPaint = CreateFillPaint(drawable.Brush, drawable.FillOpacity,
                    drawable.ViewportRotation, drawable.BitmapFillImage);
                DrawFillPath(canvas, drawable.WorldPath, fillPaint, drawable.FillStyle);
            }

            // Draw outline (polygon outline or linestring outline — for linestring, drawn under line)
            if (drawable.OutlinePen is not null)
            {
                using var outlinePaint = CreateStrokePaint(drawable.OutlinePen, drawable.OutlineWidthOverride, drawable.OutlineOpacity, res);
                canvas.DrawPath(drawable.WorldPath, outlinePaint);
            }

            // Draw line (linestring only — drawn on top of outline)
            if (drawable.LinePen is not null)
            {
                using var linePaint = CreateStrokePaint(drawable.LinePen, null, drawable.LineOpacity, res);
                canvas.DrawPath(drawable.WorldPath, linePaint);
            }
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
            // For non-solid fills, clip to the path and draw a larger rect so the pattern fills completely.
            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.ClipPath(path);
                var bounds = path.Bounds;
                // Inflate in world units (the canvas matrix will scale to screen)
                var inflate = bounds.Width * 0.3f;
                bounds.Inflate(inflate, inflate);
                canvas.DrawRect(bounds, fillPaint);
            }
        }
    }

    /// <summary>
    /// Creates an SKPaint for stroke rendering from a Mapsui Pen.
    /// This is created locally at draw time to avoid native-object lifecycle issues.
    /// </summary>
    private static SKPaint CreateStrokePaint(Pen pen, float? widthOverride, float opacity, float resolution)
    {
        var lineWidth = (widthOverride ?? (float)pen.Width) * resolution;
        var paint = new SKPaint { IsAntialias = true };
        paint.IsStroke = true;
        paint.StrokeWidth = lineWidth;
        paint.Color = pen.Color.ToSkia(opacity);
        paint.StrokeCap = pen.PenStrokeCap.ToSkia();
        paint.StrokeJoin = pen.StrokeJoin.ToSkia();
        paint.StrokeMiter = pen.StrokeMiterLimit;
        paint.PathEffect = pen.PenStyle != PenStyle.Solid
            ? pen.PenStyle.ToSkia(lineWidth, pen.DashArray, pen.DashOffset)
            : null;
        return paint;
    }

    /// <summary>
    /// Creates an SKPaint for fill rendering from a Mapsui Brush.
    /// Handles solid fills, pattern fills (Cross, Dotted, etc.), and bitmap fills.
    /// Created locally at draw time to avoid native-object lifecycle issues.
    /// </summary>
    private static SKPaint CreateFillPaint(Brush brush, float opacity, double viewportRotation, SKImage? bitmapImage)
    {
        var fillColor = brush.Color ?? new Color(128, 128, 128); // default gray
        var paint = new SKPaint { IsAntialias = true };

        if (brush.FillStyle == FillStyle.Solid)
        {
            paint.StrokeWidth = 0;
            paint.Style = SKPaintStyle.Fill;
            paint.Color = fillColor.ToSkia(opacity);
        }
        else if (brush.FillStyle is FillStyle.Bitmap or FillStyle.BitmapRotated)
        {
            paint.Style = SKPaintStyle.Fill;
            if (bitmapImage != null)
            {
                if (brush.FillStyle == FillStyle.BitmapRotated)
                {
                    paint.Shader = bitmapImage.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat,
                        SKMatrix.CreateRotation((float)(viewportRotation * Math.PI / 180.0f),
                            bitmapImage.Width >> 1, bitmapImage.Height >> 1));
                }
                else
                {
                    paint.Shader = bitmapImage.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
                }
            }
        }
        else
        {
            // Pattern fills (Cross, Dotted, etc.)
            const float scale = 10.0f;
            paint.StrokeWidth = 1;
            paint.Style = SKPaintStyle.Stroke;
            paint.Color = fillColor.ToSkia(opacity);
            using var fillPath = new SKPath();
            var matrix = SKMatrix.CreateScale(scale, scale);

            switch (brush.FillStyle)
            {
                case FillStyle.Cross:
                    fillPath.MoveTo(scale * 0.8f, scale * 0.8f);
                    fillPath.LineTo(0, 0);
                    fillPath.MoveTo(0, scale * 0.8f);
                    fillPath.LineTo(scale * 0.8f, 0);
                    paint.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.DiagonalCross:
                    fillPath.MoveTo(scale, scale);
                    fillPath.LineTo(0, 0);
                    fillPath.MoveTo(0, scale);
                    fillPath.LineTo(scale, 0);
                    paint.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.BackwardDiagonal:
                    fillPath.MoveTo(0, scale);
                    fillPath.LineTo(scale, 0);
                    paint.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.ForwardDiagonal:
                    fillPath.MoveTo(scale, scale);
                    fillPath.LineTo(0, 0);
                    paint.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.Dotted:
                    paint.Style = SKPaintStyle.StrokeAndFill;
                    fillPath.AddCircle(scale * 0.5f, scale * 0.5f, scale * 0.35f);
                    paint.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.Horizontal:
                    fillPath.MoveTo(0, scale * 0.5f);
                    fillPath.LineTo(scale, scale * 0.5f);
                    paint.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.Vertical:
                    fillPath.MoveTo(scale * 0.5f, 0);
                    fillPath.LineTo(scale * 0.5f, scale);
                    paint.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
            }
        }

        return paint;
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
