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
            () => new SvgImage(CreateCalloutContent(calloutStyle, renderService.DrawableImageCache)))!;
        var drawableImage = (SvgImage)renderService.DrawableImageCache.GetOrCreate(calloutStyle.ImageIdOfCallout,
            () => new SvgImage(CreateCallout(calloutStyle.ToCalloutOptions(), contentDrawableImage.Picture)))!;

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

        var balloonBounds = GetBalloonBounds(calloutStyle.ToCalloutOptions(), contentDrawableImage.Picture.GetSize());
        canvas.Translate((float)-balloonBounds.TailTip.X, (float)-balloonBounds.TailTip.Y);

        using var skPaint = new SKPaint() { IsAntialias = true };
        canvas.DrawPicture(drawableImage.Picture, skPaint);

        canvas.Restore();

        return true;
    }

    public static SKPicture CreateCallout(CalloutBalloonStyle callout, SKPicture content)
    {
        Size contentSize = new(content.CullRect.Width, content.CullRect.Height);

        (var width, var height) = CalcSize(callout, contentSize);

        // Create a canvas for drawing
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, (float)width, (float)height));

        var outline = CreateCalloutOutline(callout, contentSize);
        // Draw outline
        DrawOutline(callout, canvas, outline);

        // Draw content
        DrawContent(callout, canvas, content);

        // Create SKPicture from canvas
        return recorder.EndRecording();
    }

    /// <summary>
    /// Calc the size which is needed for the canvas
    /// </summary>
    /// <returns></returns>
    public static (double, double) CalcSize(CalloutBalloonStyle callout, Size contentSize)
    {
        var strokeWidth = callout.StrokeWidth < 1 ? 1 : callout.StrokeWidth;
        // Add padding around the content
        var paddingLeft = callout.Padding.Left < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Left;
        var paddingTop = callout.Padding.Top < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Top;
        var paddingRight = callout.Padding.Right < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Right;
        var paddingBottom = callout.Padding.Bottom < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Bottom;
        var width = contentSize.Width + paddingLeft + paddingRight + 1;
        var height = contentSize.Height + paddingTop + paddingBottom + 1;

        // Add length of arrow
        switch (callout.TailAlignment)
        {
            case TailAlignment.Bottom:
            case TailAlignment.Top:
                height += callout.TailHeight;
                break;
            case TailAlignment.Left:
            case TailAlignment.Right:
                width += callout.TailHeight;
                break;
        }

        // Add StrokeWidth to all sides
        width += strokeWidth * 2;
        height += strokeWidth * 2;

        // Add shadow to all sides
        width += callout.ShadowWidth * 2;
        height += callout.ShadowWidth * 2;

        return (width, height);
    }

    private static void DrawOutline(CalloutBalloonStyle balloonStyle, SKCanvas canvas, SKPath path)
    {
        using var shadow = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, Color = SKColors.Gray, MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, (float)balloonStyle.ShadowWidth) };
        using var fill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = balloonStyle.BackgroundColor.ToSkia() };
        using var stroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = balloonStyle.Color.ToSkia(), StrokeWidth = (float)balloonStyle.StrokeWidth };

        canvas.DrawPath(path, shadow);
        canvas.DrawPath(path, fill);
        canvas.DrawPath(path, stroke);
    }

    /// <summary>
    /// Update content for single and detail
    /// </summary>
    public static SKPicture CreateCalloutContent(CalloutStyle callout, DrawableImageCache drawableImageCache)
    {
        if (callout.Type == CalloutType.Image && callout.ImageSource is not null)
        {
            using var recorder = new SKPictureRecorder();
            var image = drawableImageCache.GetOrCreate(callout.ImageSource,
                () => SymbolStyleRenderer.TryCreateDrawableImage(callout.ImageSource));
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

    private static void DrawContent(CalloutBalloonStyle options, SKCanvas canvas, SKPicture content)
    {
        var strokeWidth = options.StrokeWidth < 1 ? 1 : options.StrokeWidth;
        var offsetX = options.ShadowWidth + strokeWidth + (options.Padding.Left < options.RectRadius * 0.5 ? options.RectRadius * 0.5 : options.Padding.Left);
        var offsetY = options.ShadowWidth + strokeWidth + (options.Padding.Top < options.RectRadius * 0.5 ? options.RectRadius * 0.5 : options.Padding.Top);

        switch (options.TailAlignment)
        {
            case TailAlignment.Left:
                offsetX += options.TailHeight;
                break;
            case TailAlignment.Top:
                offsetY += options.TailHeight;
                break;
        }

        var offset = new SKPoint((float)offsetX, (float)offsetY);

        using var skPaint = new SKPaint() { IsAntialias = true };
        canvas.DrawPicture(content, offset, skPaint);
    }

    /// <summary>
    /// Update path
    /// </summary>
    private static SKPath CreateCalloutOutline(CalloutBalloonStyle callout, Size contentSize)
    {
        var rect = GetBalloonBounds(callout, contentSize);

        // Create path
        var path = new SKPath();

        // Move to start point at left/top
        path.MoveTo((float)(rect.Left + callout.RectRadius), (float)rect.Top);

        // Top horizontal line
        if (callout.TailAlignment == TailAlignment.Top)
            DrawTrail(path, rect.TailStart, rect.TailTip, rect.TailEnd);

        // Top right arc
        path.ArcTo(new SKRect((float)(rect.Right - callout.RectRadius), (float)rect.Top, (float)rect.Right, (float)(rect.Top + callout.RectRadius)), 270, 90, false);

        // Right vertical line
        if (callout.TailAlignment == TailAlignment.Right)
            DrawTrail(path, rect.TailStart, rect.TailTip, rect.TailEnd);

        // Bottom right arc
        path.ArcTo(new SKRect((float)(rect.Right - callout.RectRadius), (float)(rect.Bottom - callout.RectRadius), (float)rect.Right, (float)rect.Bottom), 0, 90, false);

        // Bottom horizontal line
        if (callout.TailAlignment == TailAlignment.Bottom)
            DrawTrail(path, rect.TailStart, rect.TailTip, rect.TailEnd);

        // Bottom left arc
        path.ArcTo(new SKRect((float)rect.Left, (float)(rect.Bottom - callout.RectRadius), (float)(rect.Left + callout.RectRadius), (float)rect.Bottom), 90, 90, false);

        // Left vertical line
        if (callout.TailAlignment == TailAlignment.Left)
            DrawTrail(path, rect.TailStart, rect.TailTip, rect.TailEnd);

        // Top left arc
        path.ArcTo(new SKRect((float)rect.Left, (float)rect.Top, (float)(rect.Left + callout.RectRadius), (float)(rect.Top + callout.RectRadius)), 180, 90, false);

        path.Close();

        return path;
    }

    public static CalloutBalloonBounds GetBalloonBounds(CalloutBalloonStyle callout, Size contentSize)
    {
        double bottom, left, top, right;
        var strokeWidth = callout.StrokeWidth < 1 ? 1 : callout.StrokeWidth;
        var paddingLeft = callout.Padding.Left < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Left;
        var paddingTop = callout.Padding.Top < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Top;
        var paddingRight = callout.Padding.Right < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Right;
        var paddingBottom = callout.Padding.Bottom < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Bottom;
        var width = contentSize.Width + paddingLeft + paddingRight;
        var height = contentSize.Height + paddingTop + paddingBottom;
        // Half width is distance from left/top to arrow position, so we have to add shadow and stroke
        var halfWidth = width * callout.TailPosition + callout.ShadowWidth + strokeWidth * 2;
        var halfHeight = height * callout.TailPosition + callout.ShadowWidth + strokeWidth * 2;
        bottom = height + callout.ShadowWidth + strokeWidth * 2;
        left = callout.ShadowWidth + strokeWidth;
        top = callout.ShadowWidth + strokeWidth;
        right = width + callout.ShadowWidth + strokeWidth * 2;
        var start = new SKPoint();
        var center = new SKPoint();
        var end = new SKPoint();

        // Check, if we are to near at corners
        if (halfWidth - callout.TailWidth * 0.5f - left < callout.RectRadius)
            halfWidth = callout.TailWidth * 0.5f + left + callout.RectRadius;
        else if (halfWidth + callout.TailWidth * 0.5f > width - callout.RectRadius)
            halfWidth = width - callout.TailWidth * 0.5f - callout.RectRadius;
        if (halfHeight - callout.TailWidth * 0.5f - top < callout.RectRadius)
            halfHeight = callout.TailWidth * 0.5f + top + callout.RectRadius;
        else if (halfHeight + callout.TailWidth * 0.5f > height - callout.RectRadius)
            halfHeight = height - callout.TailWidth * 0.5f - callout.RectRadius;

        switch (callout.TailAlignment)
        {
            case TailAlignment.Bottom:
                start = new SKPoint((float)(halfWidth + callout.TailWidth * 0.5), (float)bottom);
                center = new SKPoint((float)halfWidth, (float)(bottom + callout.TailHeight));
                end = new SKPoint((float)(halfWidth - callout.TailWidth * 0.5), (float)bottom);
                break;
            case TailAlignment.Top:
                top += callout.TailHeight;
                bottom += callout.TailHeight;
                start = new SKPoint((float)(halfWidth - callout.TailWidth * 0.5), (float)top);
                center = new SKPoint((float)halfWidth, (float)(top - callout.TailHeight));
                end = new SKPoint((float)(halfWidth + callout.TailWidth * 0.5), (float)top);
                break;
            case TailAlignment.Left:
                left += callout.TailHeight;
                right += callout.TailHeight;
                start = new SKPoint((float)left, (float)(halfHeight + callout.TailWidth * 0.5));
                center = new SKPoint((float)(left - callout.TailHeight), (float)halfHeight);
                end = new SKPoint((float)left, (float)(halfHeight - callout.TailWidth * 0.5));
                break;
            case TailAlignment.Right:
                start = new SKPoint((float)right, (float)(halfHeight - callout.TailWidth * 0.5));
                center = new SKPoint((float)(right + callout.TailHeight), (float)halfHeight);
                end = new SKPoint((float)right, (float)(halfHeight + callout.TailWidth * 0.5));
                break;
        }

        return new CalloutBalloonBounds(bottom, left, top, right, start, end, center);
    }

    /// <summary>
    /// Draw tail to path
    /// </summary>
    /// <param name="path">The tail path</param>
    /// <param name="start">Start of tail at bubble</param>
    /// <param name="center">Center of tail</param>
    /// <param name="end">End of tail at bubble</param>
    private static void DrawTrail(SKPath path, SKPoint start, SKPoint center, SKPoint end)
    {
        path.LineTo(start);
        path.LineTo(center);
        path.LineTo(end);
    }
}
