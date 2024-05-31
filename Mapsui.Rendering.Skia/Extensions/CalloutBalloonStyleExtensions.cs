using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

public static class CalloutBalloonStyleExtensions
{
    public static SKPicture CreateCallout(this CalloutBalloonDefinition balloonStyle, SKPicture content)
    {
        Size contentSize = new(content.CullRect.Width, content.CullRect.Height);

        (var width, var height) = CalcSize(balloonStyle, contentSize);

        // Create a canvas for drawing
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, (float)width, (float)height));

        using var outline = balloonStyle.CreateCalloutOutline(contentSize);
        // Draw outline
        DrawOutline(balloonStyle, canvas, outline);

        // Draw content
        DrawContent(balloonStyle, canvas, content);

        // Create SKPicture from canvas
        return recorder.EndRecording();
    }

    public static CalloutBalloonBounds GetBalloonBounds(this CalloutBalloonDefinition callout, Size contentSize)
    {
        double bottom, left, top, right;
        var strokeWidth = callout.StrokeWidth < 1 ? 1 : callout.StrokeWidth;
        var paddingLeft = callout.Padding.Left < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Left;
        var paddingTop = callout.Padding.Top < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Top;
        var paddingRight = callout.Padding.Right < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Right;
        var paddingBottom = callout.Padding.Bottom < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Bottom;
        var width = contentSize.Width + paddingLeft + paddingRight;
        var height = contentSize.Height + paddingTop + paddingBottom;
        // Half width is distance from left/top to tail position, so we have to add shadow and stroke
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
    /// Calc the size which is needed for the canvas
    /// </summary>
    /// <returns></returns>
    private static (double, double) CalcSize(this CalloutBalloonDefinition callout, Size contentSize)
    {
        var strokeWidth = callout.StrokeWidth < 1 ? 1 : callout.StrokeWidth;
        // Add padding around the content
        var paddingLeft = callout.Padding.Left < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Left;
        var paddingTop = callout.Padding.Top < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Top;
        var paddingRight = callout.Padding.Right < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Right;
        var paddingBottom = callout.Padding.Bottom < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Bottom;
        var width = contentSize.Width + paddingLeft + paddingRight + 1;
        var height = contentSize.Height + paddingTop + paddingBottom + 1;

        // Add length of tail
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

    private static void DrawOutline(this CalloutBalloonDefinition balloonStyle, SKCanvas canvas, SKPath path)
    {
        using var shadow = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, Color = SKColors.Gray, MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, (float)balloonStyle.ShadowWidth) };
        using var fill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = balloonStyle.BackgroundColor.ToSkia() };
        using var stroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = balloonStyle.Color.ToSkia(), StrokeWidth = (float)balloonStyle.StrokeWidth };

        canvas.DrawPath(path, shadow);
        canvas.DrawPath(path, fill);
        canvas.DrawPath(path, stroke);
    }

    private static void DrawContent(CalloutBalloonDefinition balloonStyle, SKCanvas canvas, SKPicture content)
    {
        var strokeWidth = balloonStyle.StrokeWidth < 1 ? 1 : balloonStyle.StrokeWidth;
        var offsetX = balloonStyle.ShadowWidth + strokeWidth + (balloonStyle.Padding.Left < balloonStyle.RectRadius * 0.5 ? balloonStyle.RectRadius * 0.5 : balloonStyle.Padding.Left);
        var offsetY = balloonStyle.ShadowWidth + strokeWidth + (balloonStyle.Padding.Top < balloonStyle.RectRadius * 0.5 ? balloonStyle.RectRadius * 0.5 : balloonStyle.Padding.Top);

        switch (balloonStyle.TailAlignment)
        {
            case TailAlignment.Left:
                offsetX += balloonStyle.TailHeight;
                break;
            case TailAlignment.Top:
                offsetY += balloonStyle.TailHeight;
                break;
        }

        var offset = new SKPoint((float)offsetX, (float)offsetY);

        using var skPaint = new SKPaint() { IsAntialias = true };
        canvas.DrawPicture(content, offset, skPaint);
    }

    /// <summary>
    /// Update path
    /// </summary>
    private static SKPath CreateCalloutOutline(this CalloutBalloonDefinition balloonStyle, Size contentSize)
    {
        var rect = balloonStyle.GetBalloonBounds(contentSize);

        // Create path
        var path = new SKPath();

        // Move to start point at left/top
        path.MoveTo((float)(rect.Left + balloonStyle.RectRadius), (float)rect.Top);

        // Top horizontal line
        if (balloonStyle.TailAlignment == TailAlignment.Top)
            DrawTrail(path, rect.TailStart, rect.TailTip, rect.TailEnd);

        // Top right arc
        path.ArcTo(new SKRect((float)(rect.Right - balloonStyle.RectRadius), (float)rect.Top, (float)rect.Right, (float)(rect.Top + balloonStyle.RectRadius)), 270, 90, false);

        // Right vertical line
        if (balloonStyle.TailAlignment == TailAlignment.Right)
            DrawTrail(path, rect.TailStart, rect.TailTip, rect.TailEnd);

        // Bottom right arc
        path.ArcTo(new SKRect((float)(rect.Right - balloonStyle.RectRadius), (float)(rect.Bottom - balloonStyle.RectRadius), (float)rect.Right, (float)rect.Bottom), 0, 90, false);

        // Bottom horizontal line
        if (balloonStyle.TailAlignment == TailAlignment.Bottom)
            DrawTrail(path, rect.TailStart, rect.TailTip, rect.TailEnd);

        // Bottom left arc
        path.ArcTo(new SKRect((float)rect.Left, (float)(rect.Bottom - balloonStyle.RectRadius), (float)(rect.Left + balloonStyle.RectRadius), (float)rect.Bottom), 90, 90, false);

        // Left vertical line
        if (balloonStyle.TailAlignment == TailAlignment.Left)
            DrawTrail(path, rect.TailStart, rect.TailTip, rect.TailEnd);

        // Top left arc
        path.ArcTo(new SKRect((float)rect.Left, (float)rect.Top, (float)(rect.Left + balloonStyle.RectRadius), (float)(rect.Top + balloonStyle.RectRadius)), 180, 90, false);

        path.Close();

        return path;
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
