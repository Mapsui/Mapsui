using System;
using System.IO;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class BitmapHelper
    {
        // The field below is static for performance. Effect has not been measured.
        // Note that the default FilterQuality is None. Setting it explicitly to Low increases the quality.
        private static readonly SKPaint DefaultPaint = new SKPaint { FilterQuality = SKFilterQuality.Low }; 

        public static BitmapInfo LoadBitmap(object bitmapStream)
        {
            if (bitmapStream is Stream stream)
            {
                byte[] buffer = new byte[4];

                stream.Position = 0;
                stream.Read(buffer, 0, 4);
                stream.Position = 0;

                if (System.Text.Encoding.UTF8.GetString(buffer, 0, 4).ToLower().Equals("<svg"))
                {
                    var svg = new SkiaSharp.Extended.Svg.SKSvg();
                    svg.Load(stream);

                    return new BitmapInfo {Svg = svg};
                }

                var image = SKImage.FromEncodedData(SKData.CreateCopy(stream.ToBytes()));
                return new BitmapInfo {Bitmap = image};
            }

            if (bitmapStream is Sprite sprite)
            {
                return new BitmapInfo {Sprite = sprite};
            }

            return null;
        }

        public static void RenderBitmap(SKCanvas canvas, SKImage bitmap, float x, float y, float orientation = 0,
            float offsetX = 0, float offsetY = 0,
            LabelStyle.HorizontalAlignmentEnum horizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
            LabelStyle.VerticalAlignmentEnum verticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
            float opacity = 1f,
            float scale = 1f)
        {
            canvas.Save();

            canvas.Translate(x, y);
            if (orientation != 0)
                canvas.RotateDegrees(orientation, 0, 0); // todo: degrees or radians?
            canvas.Scale(scale, scale);

            var width = bitmap.Width;
            var height = bitmap.Height;

            x = offsetX + DetermineHorizontalAlignmentCorrection(horizontalAlignment, width);
            y = -offsetY + DetermineVerticalAlignmentCorrection(verticalAlignment, height);

            var halfWidth = width >> 1;
            var halfHeight = height >> 1;

            var rect = new SKRect(x - halfWidth, y - halfHeight, x + halfWidth, y + halfHeight);

            RenderBitmap(canvas, bitmap, rect, opacity);

            canvas.Restore();
        }

        public static void RenderSvg(SKCanvas canvas, SkiaSharp.Extended.Svg.SKSvg svg, float x, float y, float orientation = 0,
            float offsetX = 0, float offsetY = 0,
            LabelStyle.HorizontalAlignmentEnum horizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left,
            LabelStyle.VerticalAlignmentEnum verticalAlignment = LabelStyle.VerticalAlignmentEnum.Top,
            float opacity = 1f,
            float scale = 1f)
        {
            canvas.Save();

            canvas.Translate(x, y);
            canvas.RotateDegrees(orientation, 0, 0); // todo: degrees or radians?
            canvas.Scale(scale, scale);

            var halfWidth = svg.CanvasSize.Width / 2;
            var halfHeight = svg.CanvasSize.Height / 2;

            // 0/0 are assumed at center of image, but Svg has 0/0 at left top position
            canvas.Translate(-halfWidth + offsetX, -halfHeight - offsetY);

            canvas.DrawPicture(svg.Picture);

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

        public static void RenderRaster(SKCanvas canvas, SKImage bitmap, SKRect rect, float layerOpacity)
        {
            canvas.DrawImage(bitmap, rect, GetPaint(layerOpacity));
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
                    Color = new SKColor(255, 255, 255, (byte) (255 * layerOpacity))
                };
            }
            return DefaultPaint;
        }


        public static void RenderBitmap(SKCanvas canvas, SKImage bitmap, SKRect rect, float opacity = 1f)
        {
            canvas.DrawImage(bitmap, rect, GetPaint(opacity));
        }
    }
}