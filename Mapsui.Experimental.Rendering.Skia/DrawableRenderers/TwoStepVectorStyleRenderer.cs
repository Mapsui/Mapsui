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
///   <item><description><see cref="CreateDrawable"/>: Creates SKPath objects in world coordinates
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
#pragma warning disable IDISP015 // Member should not return created and cached instance - ownership transfers to cache
    public IDrawable? CreateDrawable(Viewport viewport, ILayer layer, IFeature feature,
        IStyle style, RenderService renderService)
#pragma warning restore IDISP015
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

        return drawables.Count switch
        {
            0 => null,
            1 => drawables[0],
            _ => new CompositeDrawable(drawables)
        };
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
            case CompositeDrawable composite:
                foreach (var child in composite.Children)
                    DrawDrawable(canvas, viewport, child, layer);
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
        // Path uses relative coordinates (centered at centroid) to avoid float precision loss.
        // Large world coordinates (e.g., EPSG:3857 values in millions) lose precision when cast to float.
        // By subtracting the centroid, coordinates stay small (~meters) and preserve precision.
        var worldPath = polygon.ToWorldPath(polygon.Centroid);

        var fillStyle = vectorStyle.Fill?.FillStyle ?? FillStyle.Solid;

        // Pre-create fill paint
        SKPaint? fillPaint = null;
        if (vectorStyle.Fill.IsVisible())
        {
            SKImage? bitmapFillImage = null;
            if (fillStyle is FillStyle.Bitmap or FillStyle.BitmapRotated && vectorStyle.Fill?.Image is not null)
            {
                bitmapFillImage = PolygonRenderer.GetSKImage(renderService, vectorStyle.Fill.Image);
            }
            fillPaint = CreateFillPaint(vectorStyle.Fill!, opacity, viewport.Rotation, bitmapFillImage);
        }

        // Pre-create outline paint
        SKPaint? outlinePaint = null;
        float baseOutlineWidth = 0;
        if (vectorStyle.Outline.IsVisible())
        {
            baseOutlineWidth = (float)vectorStyle.Outline!.Width;
            outlinePaint = CreateStrokePaint(vectorStyle.Outline, null, opacity);
        }

        return new VectorStyleDrawable(
            polygon.Centroid.X, polygon.Centroid.Y, worldPath,
            fillPaint: fillPaint,
            fillStyle: fillStyle,
            outlinePaint: outlinePaint,
            baseOutlineWidth: baseOutlineWidth,
            linePaint: null,
            baseLineWidth: 0);
    }

    private static VectorStyleDrawable CreateLineStringDrawable(LineString lineString,
        VectorStyle vectorStyle, float opacity)
    {
        // Path uses relative coordinates (centered at centroid) to avoid float precision loss.
        // Large world coordinates (e.g., EPSG:3857 values in millions) lose precision when cast to float.
        // By subtracting the centroid, coordinates stay small (~meters) and preserve precision.
        var worldPath = lineString.ToWorldPath(lineString.Centroid);

        // Pre-create outline paint (if both line and outline are visible)
        SKPaint? outlinePaint = null;
        float baseOutlineWidth = 0;
        if (vectorStyle.Line.IsVisible() && vectorStyle.Outline?.Width > 0)
        {
            baseOutlineWidth = (float)(vectorStyle.Outline.Width + vectorStyle.Outline.Width + (vectorStyle.Line?.Width ?? 1));
            outlinePaint = CreateStrokePaint(vectorStyle.Outline, baseOutlineWidth, opacity);
        }

        // Pre-create line paint
        SKPaint? linePaint = null;
        float baseLineWidth = 0;
        if (vectorStyle.Line.IsVisible())
        {
            baseLineWidth = (float)(vectorStyle.Line?.Width ?? 1);
            linePaint = CreateStrokePaint(vectorStyle.Line!, null, opacity);
        }

        return new VectorStyleDrawable(
            lineString.Centroid.X, lineString.Centroid.Y, worldPath,
            fillPaint: null,
            fillStyle: FillStyle.Solid,
            outlinePaint: outlinePaint,
            baseOutlineWidth: baseOutlineWidth,
            linePaint: linePaint,
            baseLineWidth: baseLineWidth);
    }

    private static void DrawVectorDrawable(SKCanvas canvas, Viewport viewport, VectorStyleDrawable drawable)
    {
        // Build the world-to-screen transformation matrix
        var matrix = CreateWorldToScreenMatrix(viewport);

        // Create translation matrix to move from origin to the drawable's reference point (centroid).
        // The path is stored in relative coordinates (centered at origin), so we translate to the
        // actual world position before applying the world-to-screen transform.
        var translateToWorld = SKMatrix.CreateTranslation((float)drawable.WorldX, (float)drawable.WorldY);

        // Combine: first translate to world position, then apply world-to-screen transform
        var combinedMatrix = SKMatrix.Concat(matrix, translateToWorld);

        // Concat the matrix onto the canvas so the GPU applies the combined transform directly.
        // Using Concat (not SetMatrix) preserves any existing canvas transformation (pixel density, layout offset, etc.).
        //
        // The canvas matrix scales geometry by 1/resolution. Stroke widths would be
        // scaled down by the matrix, so we compensate by multiplying with resolution.
        var res = (float)viewport.Resolution;

        using (new SKAutoCanvasRestore(canvas))
        {
            canvas.Concat(combinedMatrix);

            // Draw fill (polygon only) — must come first, outline goes on top
            if (drawable.FillPaint is not null)
            {
                DrawFillPath(canvas, drawable.WorldPath, drawable.FillPaint, drawable.FillStyle);
            }

            // Draw outline (polygon outline or linestring outline — for linestring, drawn under line)
            if (drawable.OutlinePaint is not null)
            {
                drawable.OutlinePaint.StrokeWidth = drawable.BaseOutlineWidth * res;
                canvas.DrawPath(drawable.WorldPath, drawable.OutlinePaint);
            }

            // Draw line (linestring only — drawn on top of outline)
            if (drawable.LinePaint is not null)
            {
                drawable.LinePaint.StrokeWidth = drawable.BaseLineWidth * res;
                canvas.DrawPath(drawable.WorldPath, drawable.LinePaint);
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
    /// Pre-created at drawable creation time; stroke width is scaled by resolution at draw time.
    /// </summary>
    private static SKPaint CreateStrokePaint(Pen pen, float? widthOverride, float opacity)
    {
        var baseWidth = widthOverride ?? (float)pen.Width;
        return new SKPaint
        {
            IsAntialias = true,
            IsStroke = true,
            StrokeWidth = baseWidth, // Will be scaled by resolution at draw time
            Color = pen.Color.ToSkia(opacity),
            StrokeCap = pen.PenStrokeCap.ToSkia(),
            StrokeJoin = pen.StrokeJoin.ToSkia(),
            StrokeMiter = pen.StrokeMiterLimit,
            // Note: Dash pattern uses base width. The pattern will scale with the stroke width at draw time.
            PathEffect = pen.PenStyle != PenStyle.Solid
                ? pen.PenStyle.ToSkia(baseWidth, pen.DashArray, pen.DashOffset)
                : null
        };
    }

    /// <summary>
    /// Creates an SKPaint for fill rendering from a Mapsui Brush.
    /// Handles solid fills, pattern fills (Cross, Dotted, etc.), and bitmap fills.
    /// Pre-created at drawable creation time to avoid allocations during rendering.
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
