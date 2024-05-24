using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using SkiaSharp;
using Svg.Skia;
using System;

namespace Mapsui.Rendering.Skia;

public class SymbolStyleRenderer : ISkiaStyleRenderer, IFeatureSize
{
    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, RenderService renderService, long iteration)
    {
        var symbolStyle = (SymbolStyle)style;
        feature.CoordinateVisitor((x, y, setter) =>
        {
            DrawXY(canvas, viewport, layer, x, y, symbolStyle, renderService);
        });
        return true;
    }


    public static bool DrawXY(SKCanvas canvas, Viewport viewport, ILayer layer, double x, double y, SymbolStyle symbolStyle, RenderService renderService)
    {
        if (symbolStyle.SymbolType == SymbolType.Image)
        {
            return DrawImage(canvas, viewport, layer, x, y, symbolStyle, renderService);
        }
        else
        {
            return DrawSymbol(canvas, viewport, layer, x, y, symbolStyle, renderService.VectorCache);
        }
    }

    private static bool DrawImage(SKCanvas canvas, Viewport viewport, ILayer layer, double x, double y, SymbolStyle symbolStyle, RenderService renderService)
    {
        var opacity = (float)(layer.Opacity * symbolStyle.Opacity);

        var (destinationX, destinationY) = viewport.WorldToScreenXY(x, y);

        if (symbolStyle.ImageSource is null)
            return false;

        var symbolCache = renderService.SymbolCache;
        var bitmapInfo = symbolCache.GetOrCreate(symbolStyle.ImageSource.ToString());
        if (bitmapInfo == null)
            return false;

        // Calc offset (relative or absolute)
        var offset = symbolStyle.SymbolOffset.CalcOffset(bitmapInfo.Width, bitmapInfo.Height);

        var rotation = (float)symbolStyle.SymbolRotation;
        if (symbolStyle.RotateWithMap) rotation += (float)viewport.Rotation;

        switch (bitmapInfo.Type)
        {
            case BitmapType.Bitmap:
                if (bitmapInfo.Bitmap == null)
                    return false; // Should we throw instead?

                if (symbolStyle.BitmapRegion is null) // It is an ordinary bitmap.
                {
                    BitmapRenderer.Draw(canvas, bitmapInfo.Bitmap,
                        (float)destinationX, (float)destinationY,
                        rotation,
                        (float)offset.X, (float)offset.Y,
                        opacity: opacity, scale: (float)symbolStyle.SymbolScale);
                }
                else
                {
                    var sprite = symbolStyle.BitmapRegion;

                    if (symbolStyle.ImageSource is null)
                        throw new Exception("If Sprite parameters are specified a ImageSource is required.");

                    var skiaSpriteCache = (SpriteCache)renderService.SpriteCache;
                    var skImage = skiaSpriteCache.GetOrCreateSKObject(ToSpriteKey(symbolStyle.ImageSource.ToString(), symbolStyle.BitmapRegion),
                        () => bitmapInfo.Bitmap.Subset(new SKRectI(sprite.X, sprite.Y, sprite.X + sprite.Width, sprite.Y + sprite.Height)));

                    BitmapRenderer.Draw(canvas, skImage,
                        (float)destinationX, (float)destinationY,
                        rotation,
                        (float)offset.X, (float)offset.Y,
                        opacity: opacity, scale: (float)symbolStyle.SymbolScale);
                }

                break;
            case BitmapType.Picture:
                if (bitmapInfo.Picture == null)
                    return false;

                PictureRenderer.Draw(canvas, bitmapInfo.Picture,
                    (float)destinationX, (float)destinationY,
                    rotation,
                    (float)offset.X, (float)offset.Y,
                    opacity: opacity, scale: (float)symbolStyle.SymbolScale, blendModeColor: symbolStyle.BlendModeColor);
                break;
            case BitmapType.Svg:
                // Todo: Perhaps remove BitmapType.Svg and SvgRenderer?
                // It looks like BitmapType.Svg is not use at all the the moment.
                if (bitmapInfo.Svg == null)
                    throw new Exception("The BitmapInfo.Svg can not be null for type Svg.");

                if (symbolStyle.SvgFillColor.HasValue || symbolStyle.SvgStrokeColor.HasValue)
                {
                    var skPicture = ((SpriteCache)renderService.SpriteCache).GetOrCreateSKObject(ToModifiedSvgKey(symbolStyle.ImageSource, symbolStyle.SvgFillColor, symbolStyle.SvgStrokeColor),
                        () =>
                        {
                            var modifiedSvgStream = SvgColorModifier.GetModifiedSvg(bitmapInfo.Svg.OriginalStream, symbolStyle.SvgFillColor, symbolStyle.SvgStrokeColor);
                            var skSvg = new SKSvg();
                            modifiedSvgStream.Position = 0;
                            skSvg.Load(modifiedSvgStream);
                            return skSvg.Picture;
                        });

                    PictureRenderer.Draw(canvas, skPicture,
                        (float)destinationX, (float)destinationY,
                        rotation,
                        (float)offset.X, (float)offset.Y,
                        opacity: opacity, scale: (float)symbolStyle.SymbolScale, blendModeColor: symbolStyle.BlendModeColor);
                    break;
                }
                else
                {
                    PictureRenderer.Draw(canvas, bitmapInfo.Svg.SKSvg.Picture,
                        (float)destinationX, (float)destinationY,
                        rotation,
                        (float)offset.X, (float)offset.Y,
                        opacity: opacity, scale: (float)symbolStyle.SymbolScale, blendModeColor: symbolStyle.BlendModeColor);
                    break;
                }
        }

        return true;
    }

    private static bool DrawSymbol(SKCanvas canvas, Viewport viewport, ILayer layer, double x, double y, SymbolStyle symbolStyle, VectorCache vectorCache)
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
        Size symbolSize = new Size(SymbolStyle.DefaultWidth, SymbolStyle.DefaultHeight);

        switch (symbolStyle.SymbolType)
        {
            case SymbolType.Image:
                if (symbolStyle.ImageSource is not null)
                {
                    var bitmapSize = ((RenderService)renderService).SymbolCache.GetSize(symbolStyle.ImageSource.ToString());
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

    public static string ToSpriteKey(string imageSource, BitmapRegion bitmapRegion)
        => $"{imageSource}?sprite=true,x={bitmapRegion.X},y={bitmapRegion.Y},width={bitmapRegion.Width},height={bitmapRegion.Height}";
    public static string ToModifiedSvgKey(string imageSource, Color? fill, Color? stroke)
        => $"{imageSource}?modifiedsvg=true,fill={fill?.ToString() ?? ""},stroke={stroke?.ToString() ?? ""}";


}
