using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

public static class CalloutBalloonDefinitionExtensions
{
    public static SKPicture CreateCallout(this CalloutBalloonDefinition balloonDefinition, SKPicture content)
    {
        Size contentSize = new(content.CullRect.Width, content.CullRect.Height);

        (var width, var height) = CalcSize(balloonDefinition, contentSize);

        // Create a canvas for drawing
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, (float)width, (float)height));

        using var outline = balloonDefinition.CreateCalloutOutline(contentSize);
        // Draw outline
        DrawOutline(balloonDefinition, canvas, outline);

        // Draw content
        DrawContent(balloonDefinition, canvas, content);

        // Create SKPicture from canvas
        return recorder.EndRecording();
    }

    public static CalloutBalloonBounds GetBalloonBounds(this CalloutBalloonDefinition callout, Size contentSize)
    {
        // This method is public because the location of the TailTip is needed when drawing
        // the callout. It may be possible to reorganize things so that this is not necessary.

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

    private static void DrawOutline(this CalloutBalloonDefinition balloonDefinition, SKCanvas canvas, SKPath path)
    {
        using var shadow = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, Color = SKColors.Gray, MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, (float)balloonDefinition.ShadowWidth) };
        using var fill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = balloonDefinition.BackgroundColor.ToSkia() };
        using var stroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = balloonDefinition.Color.ToSkia(), StrokeWidth = (float)balloonDefinition.StrokeWidth };

        canvas.DrawPath(path, shadow);
        canvas.DrawPath(path, fill);
        canvas.DrawPath(path, stroke);
    }

    private static void DrawContent(CalloutBalloonDefinition balloonDefinition, SKCanvas canvas, SKPicture content)
    {
        var strokeWidth = balloonDefinition.StrokeWidth < 1 ? 1 : balloonDefinition.StrokeWidth;
        var offsetX = balloonDefinition.ShadowWidth + strokeWidth + (balloonDefinition.Padding.Left < balloonDefinition.RectRadius * 0.5 ? balloonDefinition.RectRadius * 0.5 : balloonDefinition.Padding.Left);
        var offsetY = balloonDefinition.ShadowWidth + strokeWidth + (balloonDefinition.Padding.Top < balloonDefinition.RectRadius * 0.5 ? balloonDefinition.RectRadius * 0.5 : balloonDefinition.Padding.Top);

        switch (balloonDefinition.TailAlignment)
        {
            case TailAlignment.Left:
                offsetX += balloonDefinition.TailHeight;
                break;
            case TailAlignment.Top:
                offsetY += balloonDefinition.TailHeight;
                break;
        }

        var offset = new SKPoint((float)offsetX, (float)offsetY);

        using var skPaint = new SKPaint() { IsAntialias = true };
        canvas.DrawPicture(content, offset, skPaint);
    }

    /// <summary>
    /// Update path
    /// </summary>
    private static SKPath CreateCalloutOutline(this CalloutBalloonDefinition balloonDefinition, Size contentSize)
    {
        var rect = balloonDefinition.GetBalloonBounds(contentSize);

        // Create path
        var path = new SKPath();

        // Move to start point at left/top
        path.MoveTo((float)(rect.Left + balloonDefinition.RectRadius), (float)rect.Top);

        // Top horizontal line
        if (balloonDefinition.TailAlignment == TailAlignment.Top)
            DrawTrail(path, rect.TailStart, rect.TailTip, rect.TailEnd);

        // Top right arc
        path.ArcTo(new SKRect((float)(rect.Right - balloonDefinition.RectRadius), (float)rect.Top, (float)rect.Right, (float)(rect.Top + balloonDefinition.RectRadius)), 270, 90, false);

        // Right vertical line
        if (balloonDefinition.TailAlignment == TailAlignment.Right)
            DrawTrail(path, rect.TailStart, rect.TailTip, rect.TailEnd);

        // Bottom right arc
        path.ArcTo(new SKRect((float)(rect.Right - balloonDefinition.RectRadius), (float)(rect.Bottom - balloonDefinition.RectRadius), (float)rect.Right, (float)rect.Bottom), 0, 90, false);

        // Bottom horizontal line
        if (balloonDefinition.TailAlignment == TailAlignment.Bottom)
            DrawTrail(path, rect.TailStart, rect.TailTip, rect.TailEnd);

        // Bottom left arc
        path.ArcTo(new SKRect((float)rect.Left, (float)(rect.Bottom - balloonDefinition.RectRadius), (float)(rect.Left + balloonDefinition.RectRadius), (float)rect.Bottom), 90, 90, false);

        // Left vertical line
        if (balloonDefinition.TailAlignment == TailAlignment.Left)
            DrawTrail(path, rect.TailStart, rect.TailTip, rect.TailEnd);

        // Top left arc
        path.ArcTo(new SKRect((float)rect.Left, (float)rect.Top, (float)(rect.Left + balloonDefinition.RectRadius), (float)(rect.Top + balloonDefinition.RectRadius)), 180, 90, false);

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
