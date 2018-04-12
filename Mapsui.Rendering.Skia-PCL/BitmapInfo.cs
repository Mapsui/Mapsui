using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public enum BitmapType
    {
        Bitmap,
        Svg,
        Atlas
    }

    public class BitmapInfo
    {
        private object data;

        public BitmapType Type { get; private set; }

        public SKImage Bitmap
        {
            get
            {
                if (Type == BitmapType.Bitmap)
                    return (SKImage) data;
                else
                    return null;
            }
            set
            {
                data = value;
                Type = BitmapType.Bitmap;
            }
        }

        public SkiaSharp.Extended.Svg.SKSvg Svg
        {
            get
            {
                if (Type == BitmapType.Svg)
                    return (SkiaSharp.Extended.Svg.SKSvg) data;
                else
                    return null;
            }
            set
            {
                data = value;
                Type = BitmapType.Svg;
            }
        }

        public Atlas Atlas
        {
            get
            {
                if (Type == BitmapType.Atlas)
                    return (Atlas)data;
                else
                    return null;
            }
            set
            {
                data = value;
                Type = BitmapType.Atlas;
            }
        }

        public long IterationUsed { get; set; }

        public float Width
        {
            get
            {
                switch (Type)
                {
                    case BitmapType.Bitmap:
                        return Bitmap.Width;
                    case BitmapType.Svg:
                        return Svg.CanvasSize.Width;
                    case BitmapType.Atlas:
                        return ((Atlas) data).Width;
                    default:
                        return 0;
                }
            }
        }

        public float Height
        {
            get
            {
                switch (Type)
                {
                    case BitmapType.Bitmap:
                        return Bitmap.Height;
                    case BitmapType.Svg:
                        return Svg.CanvasSize.Height;
                    case BitmapType.Atlas:
                        return ((Atlas) data).Height;
                    default:
                        return 0;
                }
            }
        }
    }
}
