using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Styles;
using SkiaSharp;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.Rendering.Skia;

internal class PictureRenderer
{
    // The field below is static for performance. Effect has not been measured.
    // Note that setting the FilterQuality to Low increases the quality because the default is None.
    private static readonly SKPaint DefaultPaint = new() { FilterQuality = SKFilterQuality.Low };

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
    public static void Draw(SKCanvas canvas, SKPicture picture, SKRect rect, float layerOpacity = 1f, Color? blendModeColor = null)
    {
        var skPaint = GetPaint(layerOpacity, blendModeColor, out var dispose);

        var scaleX = rect.Width / picture.CullRect.Width;
        var scaleY = rect.Height / picture.CullRect.Height;

        SKMatrix matrix;
        if (scaleX == 1 && scaleY == 1)
        {
            matrix = SKMatrix.CreateTranslation(rect.Left, rect.Top);
        }
        else
        {
            matrix = SKMatrix.CreateScaleTranslation(scaleX, scaleY, rect.Left, rect.Top);
        }

        canvas.DrawPicture(picture, ref matrix, skPaint);
        if (dispose)
        {
            skPaint.Dispose();
        }
    }

    public static void Draw(SKCanvas canvas, SKPicture? picture, float x, float y, float rotation = 0,
        float offsetX = 0, float offsetY = 0,
        LabelStyle.HorizontalAlignmentEnum horizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
        LabelStyle.VerticalAlignmentEnum verticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
        float opacity = 1f,
        float scale = 1f,
        Color? blendModeColor = null)
    {
        if (picture == null)
            return;

        canvas.Save();

        canvas.Translate(x, y);
        if (rotation != 0)
            canvas.RotateDegrees(rotation, 0, 0);
        canvas.Scale(scale, scale);

        var width = picture.CullRect.Width;
        var height = picture.CullRect.Height;

        x = offsetX + DetermineHorizontalAlignmentCorrection(horizontalAlignment, width);
        y = -offsetY + DetermineVerticalAlignmentCorrection(verticalAlignment, height);

        var halfWidth = width / 2;
        var halfHeight = height / 2;

        var rect = new SKRect(x - halfWidth, y - halfHeight, x + halfWidth, y + halfHeight);

        Draw(canvas, picture, rect, opacity, blendModeColor);

        canvas.Restore();
    }
    private static float DetermineHorizontalAlignmentCorrection(
        LabelStyle.HorizontalAlignmentEnum horizontalAlignment, float width)
    {
        if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Left) return width / 2;
        if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Right) return -(width / 2);
        return 0; // center
    }

    private static float DetermineVerticalAlignmentCorrection(
        LabelStyle.VerticalAlignmentEnum verticalAlignment, float height)
    {
        if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Top) return -(height / 2);
        if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Bottom) return height / 2;
        return 0; // center
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP015:Member should not return created and cached instance")]
    private static SKPaint GetPaint(float layerOpacity, Color? blendModeColor, out bool dispose)
    {
        if (blendModeColor is not null)
        {
            // Unfortunately when blendModeColor is set we need to create a new SKPaint for
            // possible individually different color arguments. 
            dispose = true;
            return new SKPaint
            {
                FilterQuality = SKFilterQuality.Low,
                ColorFilter = SKColorFilter.CreateBlendMode(blendModeColor.ToSkia(layerOpacity), SKBlendMode.SrcIn)
            };
        };

        if (Math.Abs(layerOpacity - 1) > Utilities.Constants.Epsilon)
        {
            // Unfortunately when opacity is set we need to create a new SKPaint for
            // possible individually different opacity arguments. 
            dispose = true;
            return new SKPaint
            {
                FilterQuality = SKFilterQuality.Low,
                Color = new SKColor(255, 255, 255, (byte)(255 * layerOpacity))
            };
        };

        dispose = false;
        return DefaultPaint;
    }
}
