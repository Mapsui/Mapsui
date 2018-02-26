using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    // ReSharper disable once InconsistentNaming
    public class BitmapInfo 
    {
        public SKImage Bitmap { get; set; }
        public long IterationUsed { get; set; }
        public int Width => Bitmap.Width;
        public int Height => Bitmap.Height;
    }
}
