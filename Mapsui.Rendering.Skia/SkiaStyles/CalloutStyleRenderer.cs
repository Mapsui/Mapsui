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
    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, Styles.IStyle style, RenderService renderService, long iteration)
    {
        if (!style.Enabled)
            return false;

        var centroid = feature.Extent?.Centroid;

        if (centroid is null)
            return false;

        var calloutStyle = (CalloutStyle)style;

        var (x, y) = viewport.WorldToScreenXY(centroid.X, centroid.Y);

        if (calloutStyle.Invalidated)
        {
            // Todo: Move this update to the callout itself
            calloutStyle.ContentId = Guid.NewGuid().ToString();
            calloutStyle.FullCalloutId = Guid.NewGuid().ToString();
        }

        var contentDrawableImage = (SvgImage)renderService.DrawableImageCache.GetOrCreate(calloutStyle.ContentId, () => RenderContent(calloutStyle, renderService.DrawableImageCache));
        var drawableImage = (SvgImage)renderService.DrawableImageCache.GetOrCreate(calloutStyle.FullCalloutId, () => RenderCallout(calloutStyle, contentDrawableImage.Picture));

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
        canvas.Translate((float)calloutStyle.Offset.X, (float)calloutStyle.Offset.Y);

        using var skPaint = new SKPaint() { IsAntialias = true };
        canvas.DrawPicture(drawableImage.Picture, skPaint);

        canvas.Restore();

        return true;
    }

    private static SvgImage RenderCallout(CalloutStyle callout, SKPicture content)
    {
        var contentWidth = content.CullRect.Width;
        var contentHeight = content.CullRect.Height;

        (var width, var height) = CalcSize(callout, contentWidth, contentHeight);

        // Create a canvas for drawing
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, (float)width, (float)height));
        (var outline, var center) = CreateCalloutOutline(callout, contentWidth, contentHeight);
        // Now move Offset to the position of the arrow
        callout.Offset = new Offset(-center.X, -center.Y);

        // Draw outline
        DrawOutline(callout, canvas, outline);

        // Draw content
        DrawContent(callout, canvas, content);

        callout.Invalidated = false;
        // Create SKPicture from canvas
        return new SvgImage(recorder.EndRecording());
    }

    /// <summary>
    /// Calc the size which is needed for the canvas
    /// </summary>
    /// <returns></returns>
    private static (double, double) CalcSize(CalloutStyle callout, double contentWidth, double contentHeight)
    {
        var strokeWidth = callout.StrokeWidth < 1 ? 1 : callout.StrokeWidth;
        // Add padding around the content
        var paddingLeft = callout.Padding.Left < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Left;
        var paddingTop = callout.Padding.Top < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Top;
        var paddingRight = callout.Padding.Right < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Right;
        var paddingBottom = callout.Padding.Bottom < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Bottom;
        var width = contentWidth + paddingLeft + paddingRight + 1;
        var height = contentHeight + paddingTop + paddingBottom + 1;

        // Add length of arrow
        switch (callout.ArrowAlignment)
        {
            case ArrowAlignment.Bottom:
            case ArrowAlignment.Top:
                height += callout.ArrowHeight;
                break;
            case ArrowAlignment.Left:
            case ArrowAlignment.Right:
                width += callout.ArrowHeight;
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

    private static void DrawOutline(CalloutStyle callout, SKCanvas canvas, SKPath path)
    {
        using var shadow = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, Color = SKColors.Gray, MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, callout.ShadowWidth) };
        using var fill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = callout.BackgroundColor.ToSkia() };
        using var stroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = callout.Color.ToSkia(), StrokeWidth = callout.StrokeWidth };

        canvas.DrawPath(path, shadow);
        canvas.DrawPath(path, fill);
        canvas.DrawPath(path, stroke);
    }

    /// <summary>
    /// Update content for single and detail
    /// </summary>
    public static SvgImage RenderContent(CalloutStyle callout, DrawableImageCache drawableImageCache)
    {
        if (callout.Type == CalloutType.Image && callout.ImageSource is not null)
        {
            using var recorder = new SKPictureRecorder();
            var image = drawableImageCache.GetOrCreate(callout.ImageSource,
                () => SymbolStyleRenderer.TryCreateDrawableImage(callout.ImageSource));
            if (image is null)
            {
                Logger.Log(LogLevel.Error, $"Image not found: {callout.ImageSource}");
                return new SvgImage(recorder.EndRecording());
            }
            using var canvas = recorder.BeginRecording(new SKRect(0, 0, image.Width, image.Height));
            using var paint = new SKPaint();
            if (image is BitmapImage bitmapImage)
                canvas.DrawImage(bitmapImage.Image, 0, 0, paint);
            else if (image is SvgImage svgImage)
                canvas.DrawPicture(svgImage.Picture, paint);
            return new SvgImage(recorder.EndRecording());
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
            return new SvgImage(recorder.EndRecording());
        }
    }

    private static void DrawContent(CalloutStyle callout, SKCanvas canvas, SKPicture content)
    {
        var strokeWidth = callout.StrokeWidth < 1 ? 1 : callout.StrokeWidth;
        var offsetX = callout.ShadowWidth + strokeWidth + (callout.Padding.Left < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Left);
        var offsetY = callout.ShadowWidth + strokeWidth + (callout.Padding.Top < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Top);

        switch (callout.ArrowAlignment)
        {
            case ArrowAlignment.Left:
                offsetX += callout.ArrowHeight;
                break;
            case ArrowAlignment.Top:
                offsetY += callout.ArrowHeight;
                break;
        }

        var offset = new SKPoint((float)offsetX, (float)offsetY);

        using var skPaint = new SKPaint() { IsAntialias = true };
        canvas.DrawPicture(content, offset, skPaint);
    }

    /// <summary>
    /// Update path
    /// </summary>
    private static (SKPath, SKPoint) CreateCalloutOutline(CalloutStyle callout, double contentWidth, double contentHeight)
    {
        var strokeWidth = callout.StrokeWidth < 1 ? 1 : callout.StrokeWidth;
        var paddingLeft = callout.Padding.Left < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Left;
        var paddingTop = callout.Padding.Top < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Top;
        var paddingRight = callout.Padding.Right < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Right;
        var paddingBottom = callout.Padding.Bottom < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Bottom;
        var width = contentWidth + paddingLeft + paddingRight;
        var height = contentHeight + paddingTop + paddingBottom;
        // Half width is distance from left/top to arrow position, so we have to add shadow and stroke
        var halfWidth = width * callout.ArrowPosition + callout.ShadowWidth + strokeWidth * 2;
        var halfHeight = height * callout.ArrowPosition + callout.ShadowWidth + strokeWidth * 2;
        var bottom = height + callout.ShadowWidth + strokeWidth * 2;
        var left = callout.ShadowWidth + strokeWidth;
        var top = callout.ShadowWidth + strokeWidth;
        var right = width + callout.ShadowWidth + strokeWidth * 2;
        var start = new SKPoint();
        var center = new SKPoint();
        var end = new SKPoint();

        // Check, if we are to near at corners
        if (halfWidth - callout.ArrowWidth * 0.5f - left < callout.RectRadius)
            halfWidth = callout.ArrowWidth * 0.5f + left + callout.RectRadius;
        else if (halfWidth + callout.ArrowWidth * 0.5f > width - callout.RectRadius)
            halfWidth = width - callout.ArrowWidth * 0.5f - callout.RectRadius;
        if (halfHeight - callout.ArrowWidth * 0.5f - top < callout.RectRadius)
            halfHeight = callout.ArrowWidth * 0.5f + top + callout.RectRadius;
        else if (halfHeight + callout.ArrowWidth * 0.5f > height - callout.RectRadius)
            halfHeight = height - callout.ArrowWidth * 0.5f - callout.RectRadius;

        switch (callout.ArrowAlignment)
        {
            case ArrowAlignment.Bottom:
                start = new SKPoint((float)(halfWidth + callout.ArrowWidth * 0.5), (float)bottom);
                center = new SKPoint((float)halfWidth, (float)(bottom + callout.ArrowHeight));
                end = new SKPoint((float)(halfWidth - callout.ArrowWidth * 0.5), (float)bottom);
                break;
            case ArrowAlignment.Top:
                top += callout.ArrowHeight;
                bottom += callout.ArrowHeight;
                start = new SKPoint((float)(halfWidth - callout.ArrowWidth * 0.5), top);
                center = new SKPoint((float)halfWidth, (float)(top - callout.ArrowHeight));
                end = new SKPoint((float)(halfWidth + callout.ArrowWidth * 0.5), (float)top);
                break;
            case ArrowAlignment.Left:
                left += callout.ArrowHeight;
                right += callout.ArrowHeight;
                start = new SKPoint((float)(left), (float)(halfHeight + callout.ArrowWidth * 0.5));
                center = new SKPoint((float)(left - callout.ArrowHeight), (float)halfHeight);
                end = new SKPoint((float)left, (float)(halfHeight - callout.ArrowWidth * 0.5));
                break;
            case ArrowAlignment.Right:
                start = new SKPoint((float)(right), (float)(halfHeight - callout.ArrowWidth * 0.5));
                center = new SKPoint((float)(right + callout.ArrowHeight), (float)halfHeight);
                end = new SKPoint((float)right, (float)(halfHeight + callout.ArrowWidth * 0.5));
                break;
        }

        // Create path
        var path = new SKPath();

        // Move to start point at left/top
        path.MoveTo(left + callout.RectRadius, top);

        // Top horizontal line
        if (callout.ArrowAlignment == ArrowAlignment.Top)
            DrawArrow(path, start, center, end);

        // Top right arc
        path.ArcTo(new SKRect((float)(right - callout.RectRadius), (float)top, (float)right, (float)(top + callout.RectRadius)), 270, 90, false);

        // Right vertical line
        if (callout.ArrowAlignment == ArrowAlignment.Right)
            DrawArrow(path, start, center, end);

        // Bottom right arc
        path.ArcTo(new SKRect((float)(right - callout.RectRadius), (float)(bottom - callout.RectRadius), (float)right, (float)bottom), 0, 90, false);

        // Bottom horizontal line
        if (callout.ArrowAlignment == ArrowAlignment.Bottom)
            DrawArrow(path, start, center, end);

        // Bottom left arc
        path.ArcTo(new SKRect((float)left, (float)(bottom - callout.RectRadius), (float)(left + callout.RectRadius), (float)bottom), 90, 90, false);

        // Left vertical line
        if (callout.ArrowAlignment == ArrowAlignment.Left)
            DrawArrow(path, start, center, end);

        // Top left arc
        path.ArcTo(new SKRect(left, top, left + callout.RectRadius, top + callout.RectRadius), 180, 90, false);

        path.Close();

        return (path, center);
    }

    /// <summary>
    /// Draw arrow to path
    /// </summary>
    /// <param name="path">The arrow path</param>
    /// <param name="start">Start of arrow at bubble</param>
    /// <param name="center">Center of arrow</param>
    /// <param name="end">End of arrow at bubble</param>
    private static void DrawArrow(SKPath path, SKPoint start, SKPoint center, SKPoint end)
    {
        path.LineTo(start);
        path.LineTo(center);
        path.LineTo(end);
    }
}
