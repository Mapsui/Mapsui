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

public class ImageStyleRenderer : ISkiaStyleRenderer, IFeatureSize
{
    private static readonly SKSamplingOptions _skSamplingOptions = new(SKFilterMode.Linear, SKMipmapMode.None);

    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, RenderService renderService, long iteration)
    {
        var symbolStyle = (ImageStyle)style;
        feature.CoordinateVisitor((x, y, setter) =>
        {
            var opacity = (float)(layer.Opacity * symbolStyle.Opacity);
            PointStyleRenderer.DrawPointStyle(canvas, viewport, x, y, symbolStyle, renderService, opacity, DrawImageStyle);
        });
        return true;
    }

    private static void DrawImageStyle(SKCanvas canvas, IPointStyle pointStyle, RenderService renderService, float opacity)
    {
        if (pointStyle is ImageStyle imageStyle)
        {
            var image = imageStyle.Image ?? throw new ArgumentNullException(nameof(imageStyle.Image));
            var drawableImage = renderService.DrawableImageCache.GetOrCreate(image.SourceId,
                () => TryCreateDrawableImage(image, renderService.ImageSourceCache));
            if (drawableImage == null)
                return;

            var offset = pointStyle.RelativeOffset.GetAbsoluteOffset(drawableImage.Width, drawableImage.Height); // Offset can be relative to the size so that is why Width and Height is needed.

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
                    var coloredDrawableImage = renderService.DrawableImageCache.GetOrCreate(key, () => CreateCustomColoredSvg(image, svgImage));
                    if (coloredDrawableImage is SvgDrawableImage customColoredSvgImage)
                        DrawSKPicture(canvas, customColoredSvgImage.Picture, opacity, image.BlendModeColor);
                    else if (coloredDrawableImage is BitmapDrawableImage coloredBitmap)
                        DrawSKImage(canvas, coloredBitmap.Image, opacity);
                }
                else
                    DrawSKPicture(canvas, svgImage.Picture, opacity, image.BlendModeColor);
            }
        }
        else
            throw new Exception("Parameter pointStyle is not an ImageStyle");
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

    private static IDrawableImage CreateCustomColoredSvg(Image image, SvgDrawableImage originalSvgImage)
    {
        var originalStream = originalSvgImage.OriginalStream ?? throw new NullReferenceException("Original Stream is null");
        using var modifiedSvgStream = SvgColorModifier.GetModifiedSvg(originalStream, image.SvgFillColor, image.SvgStrokeColor);
#pragma warning disable IDISP004 // SvgDrawableImage will be responsible for disposing.
        var skSvg = new SKSvg();
        modifiedSvgStream.Position = 0;
        skSvg.Load(modifiedSvgStream);
#pragma warning restore IDISP004
        if (skSvg.Picture is null)
            throw new Exception("Failed to load modified SVG picture.");
        if (image.RasterizeSvg)
        {
            if (image.BlendModeColor is not null)
                throw new NotSupportedException("BlendModeColor is not supported for rasterized SVGs.");
            var result = new BitmapDrawableImage(ConvertPictureToImage(skSvg.Picture, (int)skSvg.Picture.CullRect.Width, (int)skSvg.Picture.CullRect.Height));
            skSvg.Dispose(); // If we rasterize the SVG we don't need the SKSvg and its SKPicture anymore
            return result;
        }
        return new SvgDrawableImage(skSvg);
    }

    public static SKImage ConvertPictureToImage(SKPicture picture, int width, int height)
    {
        // Create a surface with the given size
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        // Clear the canvas (optional)
        canvas.Clear(SKColors.Transparent);

        // Draw the SKPicture onto the canvas
        canvas.DrawPicture(picture);

        // Convert surface to image
        return surface.Snapshot();
    }

    private static BitmapDrawableImage CreateBitmapImageForRegion(BitmapDrawableImage bitmapImage, BitmapRegion sprite)
    {
        return new BitmapDrawableImage(bitmapImage.Image.Subset(new SKRectI(sprite.X, sprite.Y, sprite.X + sprite.Width, sprite.Y + sprite.Height)));
    }

    bool IFeatureSize.NeedsFeature => false;

    double IFeatureSize.FeatureSize(IStyle style, IRenderService renderService, IFeature? feature)
    {
        if (style is ImageStyle symbolStyle)
        {
            return FeatureSize(symbolStyle, renderService);
        }

        return 0;
    }

    public static double FeatureSize(ImageStyle imageStyle, IRenderService renderService)
    {
        Size symbolSize = new Size(SymbolStyle.DefaultWidth, SymbolStyle.DefaultHeight);

        if (imageStyle.Image is not null)
        {
            var image = ((RenderService)renderService).DrawableImageCache.GetOrCreate(imageStyle.Image.SourceId,
                () => TryCreateDrawableImage(imageStyle.Image, ((RenderService)renderService).ImageSourceCache));
            if (image != null)
                symbolSize = new Size(image.Width, image.Height);
        }


        var size = Math.Max(symbolSize.Height, symbolSize.Width);
        size *= imageStyle.SymbolScale; // Symbol Scale
        size = Math.Max(size, SymbolStyle.DefaultWidth); // if defaultWith is larger take this.

        // Calc offset (relative or absolute)
        var offset = imageStyle.Offset.Combine(imageStyle.RelativeOffset.GetAbsoluteOffset(symbolSize.Width, symbolSize.Height));

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
