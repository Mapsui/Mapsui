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

    public static CalloutBalloonBounds GetBalloonBounds(this CalloutBalloonDefinition balloonDefinition, Size contentSize)
    {
        // This method is public because the location of the TailTip is needed when drawing
        // the callout. It may be possible to reorganize things so that this is not necessary.

        double bottom, left, top, right;
        var strokeWidth = balloonDefinition.StrokeWidth < 1 ? 1 : balloonDefinition.StrokeWidth;
        var paddingLeft = balloonDefinition.Padding.Left < balloonDefinition.RectRadius * 0.5 ? balloonDefinition.RectRadius * 0.5 : balloonDefinition.Padding.Left;
        var paddingTop = balloonDefinition.Padding.Top < balloonDefinition.RectRadius * 0.5 ? balloonDefinition.RectRadius * 0.5 : balloonDefinition.Padding.Top;
        var paddingRight = balloonDefinition.Padding.Right < balloonDefinition.RectRadius * 0.5 ? balloonDefinition.RectRadius * 0.5 : balloonDefinition.Padding.Right;
        var paddingBottom = balloonDefinition.Padding.Bottom < balloonDefinition.RectRadius * 0.5 ? balloonDefinition.RectRadius * 0.5 : balloonDefinition.Padding.Bottom;
        var width = contentSize.Width + paddingLeft + paddingRight;
        var height = contentSize.Height + paddingTop + paddingBottom;
        // Half width is distance from left/top to tail position, so we have to add shadow and stroke
        var halfWidth = width * balloonDefinition.TailPosition + balloonDefinition.ShadowWidth + strokeWidth * 2;
        var halfHeight = height * balloonDefinition.TailPosition + balloonDefinition.ShadowWidth + strokeWidth * 2;
        bottom = height + balloonDefinition.ShadowWidth + strokeWidth * 2;
        left = balloonDefinition.ShadowWidth + strokeWidth;
        top = balloonDefinition.ShadowWidth + strokeWidth;
        right = width + balloonDefinition.ShadowWidth + strokeWidth * 2;
        var start = new SKPoint();
        var center = new SKPoint();
        var end = new SKPoint();

        // Check, if we are to near at corners
        if (halfWidth - balloonDefinition.TailWidth * 0.5f - left < balloonDefinition.RectRadius)
            halfWidth = balloonDefinition.TailWidth * 0.5f + left + balloonDefinition.RectRadius;
        else if (halfWidth + balloonDefinition.TailWidth * 0.5f > width - balloonDefinition.RectRadius)
            halfWidth = width - balloonDefinition.TailWidth * 0.5f - balloonDefinition.RectRadius;
        if (halfHeight - balloonDefinition.TailWidth * 0.5f - top < balloonDefinition.RectRadius)
            halfHeight = balloonDefinition.TailWidth * 0.5f + top + balloonDefinition.RectRadius;
        else if (halfHeight + balloonDefinition.TailWidth * 0.5f > height - balloonDefinition.RectRadius)
            halfHeight = height - balloonDefinition.TailWidth * 0.5f - balloonDefinition.RectRadius;

        switch (balloonDefinition.TailAlignment)
        {
            case TailAlignment.Bottom:
                start = new SKPoint((float)(halfWidth + balloonDefinition.TailWidth * 0.5), (float)bottom);
                center = new SKPoint((float)halfWidth, (float)(bottom + balloonDefinition.TailHeight));
                end = new SKPoint((float)(halfWidth - balloonDefinition.TailWidth * 0.5), (float)bottom);
                break;
            case TailAlignment.Top:
                top += balloonDefinition.TailHeight;
                bottom += balloonDefinition.TailHeight;
                start = new SKPoint((float)(halfWidth - balloonDefinition.TailWidth * 0.5), (float)top);
                center = new SKPoint((float)halfWidth, (float)(top - balloonDefinition.TailHeight));
                end = new SKPoint((float)(halfWidth + balloonDefinition.TailWidth * 0.5), (float)top);
                break;
            case TailAlignment.Left:
                left += balloonDefinition.TailHeight;
                right += balloonDefinition.TailHeight;
                start = new SKPoint((float)left, (float)(halfHeight + balloonDefinition.TailWidth * 0.5));
                center = new SKPoint((float)(left - balloonDefinition.TailHeight), (float)halfHeight);
                end = new SKPoint((float)left, (float)(halfHeight - balloonDefinition.TailWidth * 0.5));
                break;
            case TailAlignment.Right:
                start = new SKPoint((float)right, (float)(halfHeight - balloonDefinition.TailWidth * 0.5));
                center = new SKPoint((float)(right + balloonDefinition.TailHeight), (float)halfHeight);
                end = new SKPoint((float)right, (float)(halfHeight + balloonDefinition.TailWidth * 0.5));
                break;
        }

        return new CalloutBalloonBounds(bottom, left, top, right, start, end, center);
    }

    /// <summary>
    /// Calc the size which is needed for the canvas
    /// </summary>
    /// <returns></returns>
    private static (double, double) CalcSize(this CalloutBalloonDefinition balloonDefinition, Size contentSize)
    {
        var strokeWidth = balloonDefinition.StrokeWidth < 1 ? 1 : balloonDefinition.StrokeWidth;
        // Add padding around the content
        var paddingLeft = balloonDefinition.Padding.Left < balloonDefinition.RectRadius * 0.5 ? balloonDefinition.RectRadius * 0.5 : balloonDefinition.Padding.Left;
        var paddingTop = balloonDefinition.Padding.Top < balloonDefinition.RectRadius * 0.5 ? balloonDefinition.RectRadius * 0.5 : balloonDefinition.Padding.Top;
        var paddingRight = balloonDefinition.Padding.Right < balloonDefinition.RectRadius * 0.5 ? balloonDefinition.RectRadius * 0.5 : balloonDefinition.Padding.Right;
        var paddingBottom = balloonDefinition.Padding.Bottom < balloonDefinition.RectRadius * 0.5 ? balloonDefinition.RectRadius * 0.5 : balloonDefinition.Padding.Bottom;
        var width = contentSize.Width + paddingLeft + paddingRight + 1;
        var height = contentSize.Height + paddingTop + paddingBottom + 1;

        // Add length of tail
        switch (balloonDefinition.TailAlignment)
        {
            case TailAlignment.Bottom:
            case TailAlignment.Top:
                height += balloonDefinition.TailHeight;
                break;
            case TailAlignment.Left:
            case TailAlignment.Right:
                width += balloonDefinition.TailHeight;
                break;
        }

        // Add StrokeWidth to all sides
        width += strokeWidth * 2;
        height += strokeWidth * 2;

        // Add shadow to all sides
        width += balloonDefinition.ShadowWidth * 2;
        height += balloonDefinition.ShadowWidth * 2;

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
        var balloonBounds = balloonDefinition.GetBalloonBounds(contentSize);

        // Create path
        var path = new SKPath();

        // Move to start point at left/top
        path.MoveTo((float)(balloonBounds.Left + balloonDefinition.RectRadius), (float)balloonBounds.Top);

        // Top horizontal line
        if (balloonDefinition.TailAlignment == TailAlignment.Top)
            DrawTrail(path, balloonBounds.TailStart, balloonBounds.TailTip, balloonBounds.TailEnd);

        // Top right arc
        path.ArcTo(new SKRect((float)(balloonBounds.Right - balloonDefinition.RectRadius), (float)balloonBounds.Top, (float)balloonBounds.Right, (float)(balloonBounds.Top + balloonDefinition.RectRadius)), 270, 90, false);

        // Right vertical line
        if (balloonDefinition.TailAlignment == TailAlignment.Right)
            DrawTrail(path, balloonBounds.TailStart, balloonBounds.TailTip, balloonBounds.TailEnd);

        // Bottom right arc
        path.ArcTo(new SKRect((float)(balloonBounds.Right - balloonDefinition.RectRadius), (float)(balloonBounds.Bottom - balloonDefinition.RectRadius), (float)balloonBounds.Right, (float)balloonBounds.Bottom), 0, 90, false);

        // Bottom horizontal line
        if (balloonDefinition.TailAlignment == TailAlignment.Bottom)
            DrawTrail(path, balloonBounds.TailStart, balloonBounds.TailTip, balloonBounds.TailEnd);

        // Bottom left arc
        path.ArcTo(new SKRect((float)balloonBounds.Left, (float)(balloonBounds.Bottom - balloonDefinition.RectRadius), (float)(balloonBounds.Left + balloonDefinition.RectRadius), (float)balloonBounds.Bottom), 90, 90, false);

        // Left vertical line
        if (balloonDefinition.TailAlignment == TailAlignment.Left)
            DrawTrail(path, balloonBounds.TailStart, balloonBounds.TailTip, balloonBounds.TailEnd);

        // Top left arc
        path.ArcTo(new SKRect((float)balloonBounds.Left, (float)balloonBounds.Top, (float)(balloonBounds.Left + balloonDefinition.RectRadius), (float)(balloonBounds.Top + balloonDefinition.RectRadius)), 180, 90, false);

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
