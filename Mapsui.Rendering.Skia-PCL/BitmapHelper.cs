using System;
using System.IO;
using Mapsui.Styles;
using SkiaSharp;
using SkiaSharp.Extended.Svg;

namespace Mapsui.Rendering.Skia
{
    public static class BitmapHelper
    {
        private static readonly SKPaint Paint = new SKPaint(); // Reuse for performance. Only for opacity

        public static BitmapInfo LoadBitmap(Stream bitmapStream, bool isSvg = false)
        {
            bitmapStream.Position = 0;
            if (isSvg)
            {
                var svg = new SkiaSharp.Extended.Svg.SKSvg();
                svg.Load(bitmapStream);

                return new BitmapInfo { Svg = svg };
            }

            var image = SKImage.FromEncodedData(SKData.CreateCopy(bitmapStream.ToBytes()));
            return new BitmapInfo { Bitmap = image };
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
            canvas.RotateDegrees(orientation, 0, 0); // todo: degrees or radians?
            canvas.Scale(scale, scale);

            x = offsetX + DetermineHorizontalAlignmentCorrection(horizontalAlignment, bitmap.Width);
            y = -offsetY + DetermineVerticalAlignmentCorrection(verticalAlignment, bitmap.Height);

            var halfWidth = bitmap.Width/2;
            var halfHeight = bitmap.Height/2;

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

            var rect = new SKRect(- halfWidth, - halfHeight, + halfWidth, + halfHeight);

            //var color = new SKColor(255, 255, 255, (byte)(255 * opacity));
            //var paint = new SKPaint { Color = color, FilterQuality = SKFilterQuality.High };

            canvas.DrawPicture(svg.Picture, null);

            canvas.Restore();
        }

        private static int DetermineHorizontalAlignmentCorrection(
            LabelStyle.HorizontalAlignmentEnum horizontalAlignment, int width)
        {
            if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Left) return width/2;
            if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Right) return -width/2;
            return 0; // center
        }

        private static float DetermineHorizontalAlignmentCorrection(
            LabelStyle.HorizontalAlignmentEnum horizontalAlignment, float width)
        {
            if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Left) return width / 2;
            if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Right) return -width / 2;
            return 0.0f; // center
        }

        private static int DetermineVerticalAlignmentCorrection(
            LabelStyle.VerticalAlignmentEnum verticalAlignment, int height)
        {
            if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Top) return -height/2;
            if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Bottom) return height/2;
            return 0; // center
        }

        private static float DetermineVerticalAlignmentCorrection(
            LabelStyle.VerticalAlignmentEnum verticalAlignment, float height)
        {
            if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Top) return -height / 2;
            if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Bottom) return height / 2;
            return 0.0f; // center
        }

        public static void RenderRaster(SKCanvas canvas, SKImage bitmap, SKRect rect, float layerOpacity)
        {
            // todo: Add some way to select one method or the other.
            // Method 1) Better for quality. Helps to compare to WPF
            //var color = new SKColor(255, 255, 255, (byte)(255 * opacity));
            //var paint = new SKPaint { Color = color, FilterQuality = SKFilterQuality.High };
            //canvas.DrawBitmap(bitmap, rect, paint);

            // Method 2) Better for performance:
            if (Math.Abs(layerOpacity - 1) >  Utilities.Constants.Epsilon)
            {
                Paint.Color = new SKColor(255, 255, 255, (byte) (255 * layerOpacity));
                canvas.DrawImage(bitmap, rect, Paint);
            }
            else
            {
                canvas.DrawImage(bitmap, rect);
            }
            
        }

        public static void RenderBitmap(SKCanvas canvas, SKImage bitmap, SKRect rect, float opacity = 1f)
        {
            var color = new SKColor(255, 255, 255, (byte) (255*opacity));
            var paint = new SKPaint {Color = color, FilterQuality = SKFilterQuality.High};
            canvas.DrawImage(bitmap, rect, paint);
        }
    }
}