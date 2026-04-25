using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Experimental.Rendering.Skia.Extensions;
using Mapsui.Experimental.Rendering.Skia.Images;
using Mapsui.Experimental.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using SkiaSharp;
using System;
using Topten.RichTextKit;

namespace Mapsui.Experimental.Rendering.Skia;

public class CalloutStyleRenderer : ISkiaStyleRenderer
{
    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, Styles.IStyle style,
        Mapsui.Rendering.RenderService renderService, long iteration)
    {
        if (!style.Enabled)
            return false;

        var centroid = feature.Extent?.Centroid;

        if (centroid is null)
            return false;

        var calloutStyle = (CalloutStyle)style;

        var (x, y) = viewport.WorldToScreenXY(centroid.X, centroid.Y);

        // The CalloutStyleRenderer creates an SKPicture for rendering. We store inside an SvgImage and put it in the image cache, but it is actually
        // just a drawable like any other. We probably should use the general cache instead.

#pragma warning disable IDISP001 // The cache is responsible for disposing the items created in the cache.
#pragma warning disable IDISP004 
        var contentDrawableImage = (SvgDrawableImage)renderService.DrawableImageCache.GetOrCreate(calloutStyle.ImageIdOfCalloutContent,
            () => new SvgDrawableImage(CreateCalloutContent(calloutStyle, renderService)))!;
        var drawableImage = (SvgDrawableImage)renderService.DrawableImageCache.GetOrCreate(calloutStyle.ImageIdOfCallout,
            () => new SvgDrawableImage(calloutStyle.BalloonDefinition.CreateCallout(contentDrawableImage.Picture)))!;
#pragma warning restore IDISP004
#pragma warning restore IDISP001

        var offset = calloutStyle.Offset.Combine(calloutStyle.RelativeOffset.GetAbsoluteOffset(drawableImage.Picture.CullRect.Width, drawableImage.Picture.CullRect.Height));

        var rotation = (float)calloutStyle.SymbolRotation;

        if (viewport.Rotation != 0)
        {
            if (calloutStyle.RotateWithMap)
                rotation += (float)viewport.Rotation;
            if (calloutStyle.SymbolOffsetRotatesWithMap)
                offset = new Offset(offset.ToPoint().Rotate(-viewport.Rotation));
        }

        // Save state of the canvas, so we could move and rotate the canvas
        canvas.Save();

        // Move 0/0 to the Anchor point of Callout
        canvas.Translate((float)(x - offset.X), (float)(y - offset.Y));
        canvas.Scale((float)calloutStyle.SymbolScale, (float)calloutStyle.SymbolScale);

        // 0/0 are assumed at center of image, but Picture has 0/0 at left top position
        canvas.RotateDegrees(rotation);

        var balloonBounds = calloutStyle.BalloonDefinition.GetBalloonBounds(contentDrawableImage.Picture.GetSize());
        canvas.Translate((float)-balloonBounds.TailTip.X, (float)-balloonBounds.TailTip.Y);

        using var skPaint = new SKPaint() { IsAntialias = true };
        canvas.DrawPicture(drawableImage.Picture, skPaint);

        canvas.Restore();

        return true;
    }

    /// <summary>
    /// Update content for single and detail
    /// </summary>
    public static SKPicture CreateCalloutContent(CalloutStyle callout, Mapsui.Rendering.RenderService renderService)
    {
        if (callout.Type == CalloutType.Image && callout.Image is not null)
        {
            using var recorder = new SKPictureRecorder();

#pragma warning disable IDISP001 // The cache is responsible for disposing the items created in the cache.
            var image = renderService.DrawableImageCache.GetOrCreate(callout.Image.SourceId,
                () => ImageStyleRenderer.TryCreateDrawableImage(callout.Image, renderService.ImageSourceCache));
#pragma warning restore IDISP001

            if (image is null)
            {
                Logger.Log(LogLevel.Error, $"Image not found: {callout.Image.Source}");
                return recorder.EndRecording();
            }
            using var canvas = recorder.BeginRecording(new SKRect(0, 0, image.Width, image.Height));
            using var paint = new SKPaint();
            if (image is BitmapDrawableImage bitmapImage)
                canvas.DrawImage(bitmapImage.Image, 0, 0, paint);
            else if (image is SvgDrawableImage svgImage)
                canvas.DrawPicture(svgImage.Picture, paint);
            return recorder.EndRecording();
        }
        else
        {
            using var titleFont = CreateSkFont(callout.TitleFont, renderService);
            var titleTextBlock = SkiaTextLayoutHelper.CreateTextBlock(
                callout.Title, titleFont, callout.TitleTextAlignment, callout.TitleFontColor.ToSkia(), (float)callout.MaxWidth);

            TextBlock? subtitleTextBlock = null;
            SKFont? subtitleFont = null;

            if (callout.Type == CalloutType.Detail)
            {
                subtitleFont = CreateSkFont(callout.SubtitleFont, renderService);
                subtitleTextBlock = SkiaTextLayoutHelper.CreateTextBlock(
                    callout.Subtitle, subtitleFont, callout.SubtitleTextAlignment, callout.SubtitleFontColor.ToSkia(), (float)callout.MaxWidth);
            }

            var width = Math.Max(titleTextBlock.MeasuredWidth, subtitleTextBlock?.MeasuredWidth ?? 0);
            var height = titleTextBlock.MeasuredHeight + (subtitleTextBlock != null ? subtitleTextBlock.MeasuredHeight + (float)callout.Spacing : 0f);

            // Re-layout with final width so text alignment works correctly
            titleTextBlock.MaxWidth = width;
            titleTextBlock.Layout();
            if (subtitleTextBlock != null)
            {
                subtitleTextBlock.MaxWidth = width;
                subtitleTextBlock.Layout();
            }

            using var recorder = new SKPictureRecorder();
            using var canvas = recorder.BeginRecording(new SKRect(0, 0, width, height));

            SkiaTextLayoutHelper.PaintTextBlock(canvas, titleTextBlock, 0, 0);

            if (subtitleTextBlock != null)
                SkiaTextLayoutHelper.PaintTextBlock(canvas, subtitleTextBlock, 0, titleTextBlock.MeasuredHeight + (float)callout.Spacing);

            subtitleFont?.Dispose();

            return recorder.EndRecording();
        }
    }

    private static SKFont CreateSkFont(Font font, Mapsui.Rendering.RenderService renderService)
    {
        SKTypeface? typeface = null;

        if (font.FontSource != null)
        {
            var bytes = renderService.FontSourceCache.Get(font.FontSource);
            if (bytes != null)
            {
                using var stream = new System.IO.MemoryStream(bytes);
                typeface = SKTypeface.FromStream(stream);
            }
        }

        typeface ??= SKTypeface.FromFamilyName(font.FontFamily,
            font.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal,
            font.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);

        return new SKFont { Size = (float)font.Size, Typeface = typeface };
    }
}
