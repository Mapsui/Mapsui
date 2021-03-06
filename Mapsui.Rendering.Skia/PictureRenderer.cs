﻿using Mapsui.Styles;
using SkiaSharp;
using System;

namespace Mapsui.Rendering.Skia
{
    class PictureRenderer
    {
        // The field below is static for performance. Effect has not been measured.
        // Note that the default FilterQuality is None. Setting it explicitly to Low increases the quality.
        private static readonly SKPaint DefaultPaint = new SKPaint { FilterQuality = SKFilterQuality.Low };

        public static void Draw(SKCanvas canvas, SKPicture picture, SKRect rect, float layerOpacity = 1f)
        {
            canvas.DrawPicture(picture, rect.Left, rect.Top, GetPaint(layerOpacity));
        }

        public static void Draw(SKCanvas canvas, SKPicture picture, float x, float y, float rotation = 0,
            float offsetX = 0, float offsetY = 0,
            LabelStyle.HorizontalAlignmentEnum horizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
            LabelStyle.VerticalAlignmentEnum verticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
            float opacity = 1f,
            float scale = 1f)
        {
            canvas.Save();

            canvas.Translate(x, y);
            if (rotation != 0)
                canvas.RotateDegrees(rotation, 0, 0);
            canvas.Scale(scale, scale);

            var width = (int)picture.CullRect.Width;
            var height = (int)picture.CullRect.Height;

            x = offsetX + DetermineHorizontalAlignmentCorrection(horizontalAlignment, width);
            y = -offsetY + DetermineVerticalAlignmentCorrection(verticalAlignment, height);

            var halfWidth = width >> 1;
            var halfHeight = height >> 1;

            var rect = new SKRect(x - halfWidth, y - halfHeight, x + halfWidth, y + halfHeight);

            Draw(canvas, picture, rect, opacity);

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

        private static SKPaint GetPaint(float layerOpacity)
        {
            if (Math.Abs(layerOpacity - 1) > Utilities.Constants.Epsilon)
            {
                // Unfortunately for opacity we need to set the Color and the Color
                // is part of the Paint object. So we need to recreate the paint on
                // every draw. 
                return new SKPaint
                {
                    FilterQuality = SKFilterQuality.Low,
                    Color = new SKColor(255, 255, 255, (byte)(255 * layerOpacity))
                };
            }
            return DefaultPaint;
        }
    }
}
