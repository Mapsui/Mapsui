using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.Images;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using SkiaSharp;
using System;
using Topten.RichTextKit;

namespace Mapsui.Rendering.Skia;

public class CalloutStyleRenderer : ISkiaStyleRenderer
{
    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, Styles.IStyle style,
        RenderService renderService, long iteration)
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

        var contentDrawableImage = (SvgImage)renderService.DrawableImageCache.GetOrCreate(calloutStyle.ImageIdOfCalloutContent,
            () => new SvgImage(CreateCalloutContent(calloutStyle, renderService)))!;
        var drawableImage = (SvgImage)renderService.DrawableImageCache.GetOrCreate(calloutStyle.ImageIdOfCallout,
            () => new SvgImage(calloutStyle.BalloonDefinition.CreateCallout(contentDrawableImage.Picture)))!;

        // Calc offset (relative or absolute)
        var symbolOffset = calloutStyle.SymbolOffset.CalcOffset(drawableImage.Picture.CullRect.Width, drawableImage.Picture.CullRect.Height);

        var rotation = (float)calloutStyle.SymbolRotation;

        if (viewport.Rotation != 0)
        {
            if (calloutStyle.RotateWithMap)
                rotation += (float)viewport.Rotation;
            if (calloutStyle.SymbolOffsetRotatesWithMap)
                symbolOffset = new Offset(symbolOffset.ToPoint().Rotate(-viewport.Rotation));
        }

        // Save state of the canvas, so we could move and rotate the canvas
        canvas.Save();

        // Move 0/0 to the Anchor point of Callout
        canvas.Translate((float)(x - symbolOffset.X), (float)(y - symbolOffset.Y));
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
    public static SKPicture CreateCalloutContent(CalloutStyle callout, RenderService renderService)
    {
        if (callout.Type == CalloutType.Image && callout.ImageSource is not null)
        {
            using var recorder = new SKPictureRecorder();
            var image = renderService.DrawableImageCache.GetOrCreate(callout.ImageSource,
                () => SymbolStyleRenderer.TryCreateDrawableImage(callout.ImageSource, renderService.ImageSourceCache));
            if (image is null)
            {
                Logger.Log(LogLevel.Error, $"Image not found: {callout.ImageSource}");
                return recorder.EndRecording();
            }
            using var canvas = recorder.BeginRecording(new SKRect(0, 0, image.Width, image.Height));
            using var paint = new SKPaint();
            if (image is BitmapImage bitmapImage)
                canvas.DrawImage(bitmapImage.Image, 0, 0, paint);
            else if (image is SvgImage svgImage)
                canvas.DrawPicture(svgImage.Picture, paint);
            return recorder.EndRecording();
        }
        else
        {
            var styleSubtitle = new Topten.RichTextKit.Style();
            var styleTitle = new Topten.RichTextKit.Style();
            var textBlockTitle = new TextBlock();
            var textBlockSubtitle = new TextBlock();

            if (callout.Type == CalloutType.Detail)
            {
                styleSubtitle.FontFamily = callout.SubtitleFont.FontFamily;
                styleSubtitle.FontSize = (float)callout.SubtitleFont.Size;
                styleSubtitle.FontItalic = callout.SubtitleFont.Italic;
                styleSubtitle.FontWeight = callout.SubtitleFont.Bold ? 700 : 400;
                styleSubtitle.TextColor = callout.SubtitleFontColor.ToSkia();

                textBlockSubtitle.AddText(callout.Subtitle, styleSubtitle);
                textBlockSubtitle.Alignment = callout.SubtitleTextAlignment.ToRichTextKit();
            }
            styleTitle.FontFamily = callout.TitleFont.FontFamily;
            styleTitle.FontSize = (float)callout.TitleFont.Size;
            styleTitle.FontItalic = callout.TitleFont.Italic;
            styleTitle.FontWeight = callout.TitleFont.Bold ? 700 : 400;
            styleTitle.TextColor = callout.TitleFontColor.ToSkia();

            textBlockTitle.Alignment = callout.TitleTextAlignment.ToRichTextKit();
            textBlockTitle.AddText(callout.Title, styleTitle);

            textBlockTitle.MaxWidth = textBlockSubtitle.MaxWidth = (float)callout.MaxWidth;
            // Layout TextBlocks
            textBlockTitle.Layout();
            textBlockSubtitle.Layout();
            // Get sizes
            var width = Math.Max(textBlockTitle.MeasuredWidth, textBlockSubtitle.MeasuredWidth);
            var height = textBlockTitle.MeasuredHeight + (callout.Type == CalloutType.Detail ? textBlockSubtitle.MeasuredHeight + (float)callout.Spacing : 0f);
            // Now we have the correct width, so make a new layout cycle for text alignment
            textBlockTitle.MaxWidth = textBlockSubtitle.MaxWidth = width;
            textBlockTitle.Layout();
            textBlockSubtitle.Layout();
            // Create bitmap from TextBlock
            using var recorder = new SKPictureRecorder();
            using var canvas = recorder.BeginRecording(new SKRect(0, 0, width, height));
            // Draw text to canvas
            textBlockTitle.Paint(canvas, new TextPaintOptions() { Edging = SKFontEdging.Antialias });
            if (callout.Type == CalloutType.Detail)
                textBlockSubtitle.Paint(canvas, new SKPoint(0, textBlockTitle.MeasuredHeight + (float)callout.Spacing), new TextPaintOptions() { Edging = SKFontEdging.Antialias });
            return recorder.EndRecording();
        }
    }
}
