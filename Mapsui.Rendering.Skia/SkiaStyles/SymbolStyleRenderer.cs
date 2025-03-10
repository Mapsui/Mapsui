using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using SkiaSharp;
using System;

namespace Mapsui.Rendering.Skia;

public class SymbolStyleRenderer : ISkiaStyleRenderer, IFeatureSize
{
    public static void DrawStatic(SKCanvas canvas, Viewport viewport, ILayer layer, double x, double y, IPointStyle pointStyle, RenderService renderService)
    {
        var opacity = (float)(layer.Opacity * pointStyle.Opacity);
        PointStyleRenderer.DrawPointStyle(canvas, viewport, x, y, pointStyle, renderService, opacity, DrawSymbolStyle);
    }

    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, RenderService renderService, long iteration)
    {
        var symbolStyle = (SymbolStyle)style;
        feature.CoordinateVisitor((x, y, setter) =>
        {
            var opacity = (float)(layer.Opacity * symbolStyle.Opacity);
            PointStyleRenderer.DrawPointStyle(canvas, viewport, x, y, symbolStyle, renderService, opacity, DrawSymbolStyle);
        });
        return true;
    }

    private static void DrawSymbolStyle(SKCanvas canvas, IPointStyle pointStyle, RenderService renderService, float opacity)
    {
        if (pointStyle is SymbolStyle symbolStyle)
        {
            canvas.Save();

            var offset = symbolStyle.RelativeOffset.GetAbsoluteOffset(SymbolStyle.DefaultWidth, SymbolStyle.DefaultWidth);
            canvas.Translate((float)offset.X, (float)-offset.Y);

            using var path = renderService.VectorCache.GetOrCreate(symbolStyle.SymbolType, CreatePath);
            if (symbolStyle.Fill.IsVisible())
            {
                using var fillPaint = renderService.VectorCache.GetOrCreate((symbolStyle.Fill!, opacity), CreateFillPaint);
                canvas.DrawPath(path, fillPaint);
            }

            if (symbolStyle.Outline.IsVisible())
            {
                using var linePaint = renderService.VectorCache.GetOrCreate((symbolStyle.Outline!, opacity), CreateLinePaint);
                canvas.DrawPath(path, linePaint);
            }

            canvas.Restore();
        }
        else
            throw new ArgumentException($"Expected {nameof(SymbolStyle)} but got {pointStyle?.GetType().Name}");
    }

    private static SKPath CreatePath(SymbolType symbolType)
    {
        var width = (float)SymbolStyle.DefaultWidth;
        var halfWidth = width / 2;
        var halfHeight = (float)SymbolStyle.DefaultHeight / 2;
        var skPath = new SKPath();

        switch (symbolType)
        {
            case SymbolType.Ellipse:
                skPath.AddCircle(0, 0, halfWidth);
                break;
            case SymbolType.Rectangle:
                skPath.AddRect(new SKRect(-halfWidth, -halfHeight, halfWidth, halfHeight));
                break;
            case SymbolType.Triangle:
                TrianglePath(skPath, 0, 0, width);
                break;
            default: // Invalid value
                throw new ArgumentException($"Unknown {nameof(SymbolType)} '{nameof(symbolType)}'");
        }

        return skPath;
    }

    private static SKPaint CreateLinePaint((Pen outline, float opacity) valueTuple)
    {
        var outline = valueTuple.outline;
        var opacity = valueTuple.opacity;

        return new SKPaint
        {
            Color = outline.Color.ToSkia(opacity),
            StrokeWidth = (float)outline.Width,
            StrokeCap = outline.PenStrokeCap.ToSkia(),
            PathEffect = outline.PenStyle.ToSkia((float)outline.Width),
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };
    }

    private static SKPaint CreateFillPaint((Brush fill, float opacity) valueTuple)
    {
        var fill = valueTuple.fill;
        var opacity = valueTuple.opacity;

        return new SKPaint
        {
            Color = fill.Color.ToSkia(opacity),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
    }

    /// Triangle of side 'sideLength', centered on the same point as if a circle of diameter 'sideLength' was there
    private static void TrianglePath(SKPath path, float x, float y, float sideLength)
    {
        var altitude = Math.Sqrt(3) / 2.0 * sideLength;
        var inradius = altitude / 3.0;
        var circumradius = 2.0 * inradius;

        var topX = x;
        var topY = y - circumradius;
        var leftX = x + sideLength * -0.5;
        var leftY = y + inradius;
        var rightX = x + sideLength * 0.5;
        var rightY = y + inradius;

        path.MoveTo(topX, (float)topY);
        path.LineTo((float)leftX, (float)leftY);
        path.LineTo((float)rightX, (float)rightY);
        path.Close();
    }

    bool IFeatureSize.NeedsFeature => false;

    double IFeatureSize.FeatureSize(IStyle style, IRenderService renderService, IFeature? feature)
    {
        if (style is SymbolStyle symbolStyle)
        {
            return FeatureSize(symbolStyle, renderService);
        }

        return 0;
    }

    public static double FeatureSize(SymbolStyle symbolStyle, IRenderService renderService)
    {
        var vectorSize = VectorStyleRenderer.FeatureSize(symbolStyle);
        Size symbolSize = new Size(vectorSize, vectorSize);

        var size = Math.Max(symbolSize.Height, symbolSize.Width);
        size *= symbolStyle.SymbolScale; // Symbol Scale
        size = Math.Max(size, SymbolStyle.DefaultWidth); // if defaultWith is larger take this.

        // Calc offset (relative or absolute)
        var offset = symbolStyle.Offset.Combine(symbolStyle.RelativeOffset.GetAbsoluteOffset(symbolSize.Width, symbolSize.Height));

        // Pythagoras for maximal distance
        var length = Math.Sqrt(offset.X * offset.X + offset.Y * offset.Y);

        // add length to size multiplied by two because the total size increased by the offset
        size += length * 2;

        return size;
    }
}
