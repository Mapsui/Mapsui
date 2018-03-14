using SkiaSharp;
using SkiaSharp.Extended.Svg;

namespace Mapsui.Rendering.Skia
{
    // ReSharper disable once InconsistentNaming
    public class BitmapInfo
    {
        private object image;
        private bool isBitmap;

        public SKImage Bitmap
        {
            get
            {
                return (SKImage)image;
            }
            set
            {
                image = value;
                isBitmap = true;
            }
        }

        public SkiaSharp.Extended.Svg.SKSvg Svg
        {
            get
            {
                return (SkiaSharp.Extended.Svg.SKSvg)image;
            }
            set
            {
                image = value;
                isBitmap = false;
            }
        }

        public long IterationUsed { get; set; }

        public float Width
        {
            get
            {
                if (isBitmap)
                    return Bitmap.Width;
                else
                    return Svg.CanvasSize.Width;
            }
        }

        public float Height
        {
            get
            {
                if (isBitmap)
                    return Bitmap.Height;
                else
                    return Svg.CanvasSize.Height;
            }
        }
    }
}
