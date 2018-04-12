using Mapsui.Geometries;

namespace Mapsui.Styles
{
    public class Atlas
    {
        public int BitmapId { get; }
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        public float PixelRatio { get; }
        public object Data { get; set; }

        public Atlas(int bitmapId, int x, int y, int width, int height, float pixelRatio)
        {
            BitmapId = bitmapId;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            PixelRatio = pixelRatio;
        }

        public Atlas(int bitmapId, Point p, Size s, float pixelRatio) : this(bitmapId, (int)p.X, (int)p.Y, (int)s.Width, (int)s.Height, pixelRatio)
        {
        }

    }
}
