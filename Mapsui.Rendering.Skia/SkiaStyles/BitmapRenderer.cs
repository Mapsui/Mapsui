using Mapsui.Styles;
using SkiaSharp;
using System;
using System.Runtime.CompilerServices;

namespace Mapsui.Rendering.Skia;

internal class BitmapRenderer
{
    // The field below is static for performance. Effect has not been measured.
    // Note that the default FilterQuality is None. Setting it explicitly to Low increases the quality.
    private static readonly SKPaint _defaultPaint = new();
    private static readonly SKSamplingOptions _defaultSamplingOptions = new(SKFilterMode.Linear, SKMipmapMode.None);

    public static void Draw(SKCanvas canvas, SKImage bitmap, SKRect rect, float layerOpacity = 1f)
    {
        if (IsSemiTransparent(layerOpacity)) // Unfortunately for opacity we need to set the Color and the Color is part of the LinePaint object. So we need to recreate the paint on every draw. 
        {
            using var skPaint = new SKPaint { Color = new SKColor(255, 255, 255, (byte)(255 * layerOpacity)) };
            canvas.DrawImage(bitmap, rect, _defaultSamplingOptions, skPaint);
        }
        else
        {
            // If the layerOpacity is 1, we can use the default paint without creating a new one.
            canvas.DrawImage(bitmap, rect, _defaultSamplingOptions, _defaultPaint);
        }
    }

    public static void Draw(SKCanvas canvas, SKImage? bitmap, float x, float y, float rotation = 0,
        float offsetX = 0, float offsetY = 0,
        LabelStyle.HorizontalAlignmentEnum horizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
        LabelStyle.VerticalAlignmentEnum verticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
        float opacity = 1f,
        float scale = 1f)
    {
        if (bitmap == null)
            return;

        canvas.Save();

        canvas.Translate(x, y);
        if (rotation != 0)
            canvas.RotateDegrees(rotation, 0, 0);
        canvas.Scale(scale, scale);

        var width = bitmap.Width;
        var height = bitmap.Height;

        x = offsetX + DetermineHorizontalAlignmentCorrection(horizontalAlignment, width);
        y = -offsetY + DetermineVerticalAlignmentCorrection(verticalAlignment, height);

        var halfWidth = width >> 1;
        var halfHeight = height >> 1;

        var rect = new SKRect(x - halfWidth, y - halfHeight, x + halfWidth, y + halfHeight);

        Draw(canvas, bitmap, rect, opacity);

        canvas.Restore();
    }
    private static int DetermineHorizontalAlignmentCorrection(
        LabelStyle.HorizontalAlignmentEnum horizontalAlignment, int width)
    {
        if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Left) return width >> 1;
        if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Right) return -(width >> 1);
        return 0; // center
    }

    private static int DetermineVerticalAlignmentCorrection(
        LabelStyle.VerticalAlignmentEnum verticalAlignment, int height)
    {
        if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Top) return -(height >> 1);
        if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Bottom) return height >> 1;
        return 0; // center
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSemiTransparent(float layerOpacity) =>
        Math.Abs(layerOpacity - 1) > Utilities.Constants.Epsilon;
}
