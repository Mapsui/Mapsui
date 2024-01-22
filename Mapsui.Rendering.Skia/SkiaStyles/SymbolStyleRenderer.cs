using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mapsui.Rendering.Skia;

public class SymbolStyleRenderer : ISkiaStyleRenderer, IFeatureSize
{
    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, IRenderCache renderCache, long iteration)
    {
        var cache = (IRenderCache<SKPath, SKPaint>)renderCache;
        var symbolStyle = (SymbolStyle)style;
        switch (feature)
        {
            case PointFeature pointFeature:
                DrawXY(canvas, viewport, layer, pointFeature.Point.X, pointFeature.Point.Y, symbolStyle, cache);
                break;
            case GeometryFeature geometryFeature:
                switch (geometryFeature.Geometry)
                {
                    case GeometryCollection collection:
                        foreach (var point in GetPoints(collection))
                            DrawXY(canvas, viewport, layer, point.X, point.Y, symbolStyle, cache);
                        break;
                    case Point point:
                        DrawXY(canvas, viewport, layer, point.X, point.Y, symbolStyle, cache);
                        break;
                }
                break;
        }

        return true;
    }

    private static IEnumerable<Point> GetPoints(GeometryCollection geometryCollection)
    {
        foreach (var geometry in geometryCollection)
        {
            if (geometry is Point point)
                yield return point;
            if (geometry is GeometryCollection collection)
            {
                var points = GetPoints(collection);
                foreach (var p in points)
                    yield return p;
            }
        }
    }

    public static bool DrawXY(SKCanvas canvas, Viewport viewport, ILayer layer, double x, double y, SymbolStyle symbolStyle, IRenderCache<SKPath,SKPaint> renderCache)
    {
        if (symbolStyle.SymbolType == SymbolType.Image)
        {
            return DrawImage(canvas, viewport, layer, x, y, symbolStyle, renderCache);
        }
        else
        {
            return DrawSymbol(canvas, viewport, layer, x, y, symbolStyle, renderCache);
        }
    }

    private static bool DrawImage(SKCanvas canvas, Viewport viewport, ILayer layer, double x, double y, SymbolStyle symbolStyle, ISymbolCache symbolCache)
    {
        var opacity = (float)(layer.Opacity * symbolStyle.Opacity);

        var (destX, destY) = viewport.WorldToScreenXY(x, y);

        if (symbolStyle.BitmapId < 0)
            return false;

        var bitmap = (BitmapInfo)symbolCache.GetOrCreate(symbolStyle.BitmapId);
        if (bitmap == null)
            return false;

        // Calc offset (relative or absolute)
        var offset = symbolStyle.SymbolOffset.CalcOffset(bitmap.Width, bitmap.Height);

        var rotation = (float)symbolStyle.SymbolRotation;
        if (symbolStyle.RotateWithMap) rotation += (float)viewport.Rotation;

        switch (bitmap.Type)
        {
            case BitmapType.Bitmap:
                if (bitmap.Bitmap == null)
                    return false;

                BitmapRenderer.Draw(canvas, bitmap.Bitmap,
                    (float)destX, (float)destY,
                    rotation,
                    (float)offset.X, (float)offset.Y,
                    opacity: opacity, scale: (float)symbolStyle.SymbolScale);
                break;
            case BitmapType.Picture:
                if (bitmap.Picture == null)
                    return false;

                PictureRenderer.Draw(canvas, bitmap.Picture,
                    (float)destX, (float)destY,
                    rotation,
                    (float)offset.X, (float)offset.Y,
                    opacity: opacity, scale: (float)symbolStyle.SymbolScale, blendModeColor: symbolStyle.BlendModeColor);
                break;
            case BitmapType.Svg:
                // Todo: Perhaps remove BitmapType.Svg and SvgRenderer?
                // It looks like BitmapType.Svg is not use at all the the moment.
                if (bitmap.Svg == null)
                    return false;

                SvgRenderer.Draw(canvas, bitmap.Svg,
                    (float)destX, (float)destY,
                    rotation,
                    (float)offset.X, (float)offset.Y,
                    opacity: opacity, scale: (float)symbolStyle.SymbolScale);
                break;
            case BitmapType.Sprite:
                if (bitmap.Sprite == null)
                    return false;

                var sprite = bitmap.Sprite;
                if (sprite.Data == null)
                {
                    var bitmapAtlas = (BitmapInfo)symbolCache.GetOrCreate(sprite.Atlas);
                    sprite.Data = bitmapAtlas?.Bitmap?.Subset(new SKRectI(sprite.X, sprite.Y, sprite.X + sprite.Width,
                        sprite.Y + sprite.Height));
                }
                if (sprite.Data is SKImage skImage)
                    BitmapRenderer.Draw(canvas, skImage,
                        (float)destX, (float)destY,
                        rotation,
                        (float)offset.X, (float)offset.Y,
                        opacity: opacity, scale: (float)symbolStyle.SymbolScale);
                break;
        }

        return true;
    }

    private static bool DrawSymbol(SKCanvas canvas, Viewport viewport, ILayer layer, double x, double y, SymbolStyle symbolStyle, IVectorCache<SKPath, SKPaint> vectorCache)
    {
        var opacity = (float)(layer.Opacity * symbolStyle.Opacity);

        var (destX, destY) = viewport.WorldToScreenXY(x, y);

        canvas.Save();

        canvas.Translate((float)destX, (float)destY);
        canvas.Scale((float)symbolStyle.SymbolScale, (float)symbolStyle.SymbolScale);

        var offset = symbolStyle.SymbolOffset.CalcOffset(SymbolStyle.DefaultWidth, SymbolStyle.DefaultWidth);

        canvas.Translate((float)offset.X, (float)-offset.Y);

        if (symbolStyle.SymbolRotation != 0)
        {
            var rotation = symbolStyle.SymbolRotation;
            if (symbolStyle.RotateWithMap) rotation += viewport.Rotation;
            canvas.RotateDegrees((float)rotation);
        }

        using var path = vectorCache.GetOrCreatePath(symbolStyle.SymbolType, CreatePath);
        if (symbolStyle.Fill.IsVisible())
        {
            using var fillPaint = vectorCache.GetOrCreatePaint((symbolStyle.Fill!, opacity), CreateFillPaint);
            canvas.DrawPath(path, fillPaint);
        }

        if (symbolStyle.Outline.IsVisible())
        {
            using var linePaint = vectorCache.GetOrCreatePaint((symbolStyle.Outline!, opacity), CreateLinePaint);
            canvas.DrawPath(path, linePaint);
        }

        canvas.Restore();

        return true;
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

    double IFeatureSize.FeatureSize(IStyle style, IRenderCache renderCache, IFeature? feature)
    {
        if (style is SymbolStyle symbolStyle)
        {
            return FeatureSize(symbolStyle, renderCache);
        }

        return 0;
    }

    public static double FeatureSize(SymbolStyle symbolStyle, ISymbolCache symbolCache)
    {
        Size symbolSize = new Size(SymbolStyle.DefaultWidth, SymbolStyle.DefaultHeight);

        switch (symbolStyle.SymbolType)
        {
            case SymbolType.Image:
                if (symbolStyle.BitmapId >= 0)
                {
                    var bitmapSize = symbolCache.GetSize(symbolStyle.BitmapId);
                    if (bitmapSize != null)
                    {
                        symbolSize = bitmapSize;
                    }
                }

                break;
            case SymbolType.Ellipse:
            case SymbolType.Rectangle:
            case SymbolType.Triangle:
                var vectorSize = VectorStyleRenderer.FeatureSize(symbolStyle);
                symbolSize = new Size(vectorSize, vectorSize);
                break;
        }

        var size = Math.Max(symbolSize.Height, symbolSize.Width);
        size *= symbolStyle.SymbolScale; // Symbol Scale
        size = Math.Max(size, SymbolStyle.DefaultWidth); // if defaultWith is larger take this.

        // Calc offset (relative or absolute)
        var offset = symbolStyle.SymbolOffset.CalcOffset(symbolSize.Width, symbolSize.Height);

        // Pythagoras for maximal distance
        var length = Math.Sqrt(offset.X * offset.X + offset.Y * offset.Y);

        // add length to size multiplied by two because the total size increased by the offset
        size += (length * 2);

        return size;
    }
}
