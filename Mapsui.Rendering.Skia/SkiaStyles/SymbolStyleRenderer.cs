﻿using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.Images;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using SkiaSharp;
using Svg.Skia;
using System;

namespace Mapsui.Rendering.Skia;

public class SymbolStyleRenderer : ISkiaStyleRenderer, IFeatureSize
{
    private static SKSamplingOptions _skSamplingOptions = new(SKFilterMode.Linear, SKMipmapMode.None);

    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, RenderService renderService, long iteration)
    {
        var symbolStyle = (SymbolStyle)style;
        feature.CoordinateVisitor((x, y, setter) =>
        {
            DrawXY(canvas, viewport, layer, x, y, symbolStyle, renderService);
        });
        return true;
    }

    public static void DrawXY(SKCanvas canvas, Viewport viewport, ILayer layer, double x, double y, SymbolStyle symbolStyle, RenderService renderService)
    {
        var opacity = (float)(layer.Opacity * symbolStyle.Opacity);
        var (destinationX, destinationY) = viewport.WorldToScreenXY(x, y);

        canvas.Save();
        canvas.Translate((float)destinationX, (float)destinationY);
        canvas.Scale((float)symbolStyle.SymbolScale, (float)symbolStyle.SymbolScale);

        var rotation = symbolStyle.SymbolRotation;
        if (symbolStyle.RotateWithMap)
            rotation += viewport.Rotation;
        if (rotation != 0)
            canvas.RotateDegrees((float)rotation);

        canvas.Translate((float)symbolStyle.Offset.X, (float)-symbolStyle.Offset.Y);

        if (symbolStyle.Image is Image sourceImage)
            DrawSourceImage(canvas, sourceImage, symbolStyle.RelativeOffset, renderService, opacity);
        else
            DrawBuiltInImage(canvas, symbolStyle, renderService.VectorCache, opacity);

        canvas.Restore();
    }

    private static void DrawSourceImage(SKCanvas canvas, Image image, RelativeOffset symbolOffset, RenderService renderService, float opacity)
    {
        canvas.Save();

        if (image is null)
            throw new Exception("SymbolStyle.Image should not be null in the DrawImage render method");

        var drawableImage = renderService.DrawableImageCache.GetOrCreate(image.SourceId,
            () => TryCreateDrawableImage(image, renderService.ImageSourceCache));
        if (drawableImage == null)
            return;

        var offset = symbolOffset.GetAbsoluteOffset(drawableImage.Width, drawableImage.Height); // Offset can be relative to the size so that is why Width and Height is needed.

        canvas.Translate((float)offset.X, -(float)offset.Y);

        if (drawableImage is BitmapDrawableImage bitmapImage)
        {
            if (image.BitmapRegion is not null) // Get image for region if specified
            {
                var key = image.GetSourceIdForBitmapRegion();
                if (renderService.DrawableImageCache.GetOrCreate(key, () => CreateBitmapImageForRegion(bitmapImage, image.BitmapRegion)) is BitmapDrawableImage bitmapRegionImage)
                    bitmapImage = bitmapRegionImage;
            }

            DrawSKImage(canvas, bitmapImage.Image, opacity);

        }
        else if (drawableImage is SvgDrawableImage svgImage)
        {
            if (image.SvgFillColor.HasValue || image.SvgStrokeColor.HasValue) // Get custom colored SVG if custom colors are set
            {
                var key = image.GetSourceIdForSvgWithCustomColors();
                if (renderService.DrawableImageCache.GetOrCreate(key, () => CreateCustomColoredSvg(image, svgImage)) is SvgDrawableImage customColoredSvgImage)
                    svgImage = customColoredSvgImage;
            }

            DrawSKPicture(canvas, svgImage.Picture, opacity, image.BlendModeColor);
        }
        canvas.Restore();
    }

    public static void DrawSKImage(SKCanvas canvas, SKImage bitmap, float opacity)
    {
        using var paint = new SKPaint { Color = new SKColor(255, 255, 255, (byte)(255 * opacity)) };

        var halfWidth = bitmap.Width >> 1;
        var halfHeight = bitmap.Height >> 1;

        var rect = new SKRect(-halfWidth, -halfHeight, halfWidth, halfHeight);

        canvas.DrawImage(bitmap, rect, _skSamplingOptions, paint);
    }

    public static void DrawSKPicture(SKCanvas canvas, SKPicture picture, float opacity, Color? blendModeColor)
    {
        using var skPaint = CreatePaintForSKPicture(opacity, blendModeColor);

        var halfWidth = picture.CullRect.Width / 2;
        var halfHeight = picture.CullRect.Height / 2;

        var matrix = SKMatrix.CreateTranslation(-halfWidth, -halfHeight);

        canvas.DrawPicture(picture, in matrix, skPaint);
    }

    private static SKPaint CreatePaintForSKPicture(float opacity, Color? blendModeColor)
    {
        var paint = new SKPaint();

        if (blendModeColor is not null)
            paint.ColorFilter = SKColorFilter.CreateBlendMode(blendModeColor.ToSkia(opacity), SKBlendMode.SrcIn);

        if (Math.Abs(opacity - 1) > Utilities.Constants.Epsilon)
            paint.Color = new SKColor(255, 255, 255, (byte)(255 * opacity));

        return paint;
    }

    private static SvgDrawableImage CreateCustomColoredSvg(Image image, SvgDrawableImage originalSvgImage)
    {
        var originalStream = originalSvgImage.OriginalStream ?? throw new NullReferenceException("Original Stream is null");
        using var modifiedSvgStream = SvgColorModifier.GetModifiedSvg(originalStream, image.SvgFillColor, image.SvgStrokeColor);
#pragma warning disable IDISP001
#pragma warning disable IDISP004
        var skSvg = new SKSvg();
        modifiedSvgStream.Position = 0;
        skSvg.Load(modifiedSvgStream);
#pragma warning restore IDISP001
#pragma warning restore IDISP004
        if (skSvg.Picture is null)
            throw new Exception("Failed to load modified SVG picture.");
        return new SvgDrawableImage(skSvg.Picture);
    }

    private static BitmapDrawableImage CreateBitmapImageForRegion(BitmapDrawableImage bitmapImage, BitmapRegion sprite)
    {
        return new BitmapDrawableImage(bitmapImage.Image.Subset(new SKRectI(sprite.X, sprite.Y, sprite.X + sprite.Width, sprite.Y + sprite.Height)));
    }

    private static void DrawBuiltInImage(SKCanvas canvas, SymbolStyle symbolStyle, VectorCache vectorCache,
        float opacity)
    {
        canvas.Save();

        var offset = symbolStyle.RelativeOffset.GetAbsoluteOffset(SymbolStyle.DefaultWidth, SymbolStyle.DefaultWidth);
        canvas.Translate((float)offset.X, (float)-offset.Y);

        using var path = vectorCache.GetOrCreate(symbolStyle.SymbolType, CreatePath);
        if (symbolStyle.Fill.IsVisible())
        {
            using var fillPaint = vectorCache.GetOrCreate((symbolStyle.Fill!, opacity), CreateFillPaint);
            canvas.DrawPath(path, fillPaint);
        }

        if (symbolStyle.Outline.IsVisible())
        {
            using var linePaint = vectorCache.GetOrCreate((symbolStyle.Outline!, opacity), CreateLinePaint);
            canvas.DrawPath(path, linePaint);
        }

        canvas.Restore();
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
                if (symbolStyle.Image is not null)
                {
                    var image = ((RenderService)renderService).DrawableImageCache.GetOrCreate(symbolStyle.Image.SourceId,
                        () => TryCreateDrawableImage(symbolStyle.Image, ((RenderService)renderService).ImageSourceCache));
                    if (image != null)
                        symbolSize = new Size(image.Width, image.Height);
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
        var offset = symbolStyle.Offset.Combine(symbolStyle.RelativeOffset.GetAbsoluteOffset(symbolSize.Width, symbolSize.Height));

        // Pythagoras for maximal distance
        var length = Math.Sqrt(offset.X * offset.X + offset.Y * offset.Y);

        // add length to size multiplied by two because the total size increased by the offset
        size += length * 2;

        return size;
    }

    // Todo: Figure out a better place for this method
    public static IDrawableImage? TryCreateDrawableImage(Image image, ImageSourceCache imageSourceCache)
    {
        var imageBytes = imageSourceCache.Get(image);
        if (imageBytes == null)
            return null;
        var drawableImage = ImageHelper.ToDrawableImage(imageBytes);
        return drawableImage;
    }

}
