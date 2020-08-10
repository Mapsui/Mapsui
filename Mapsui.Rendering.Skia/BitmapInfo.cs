using Mapsui.Styles;
using SkiaSharp;
using Svg.Skia;

namespace Mapsui.Rendering.Skia
{
    public enum BitmapType
    {
        Bitmap,
        Svg,
        Sprite
    }

    public class BitmapInfo
    {
        private object _data;

        public BitmapType Type { get; private set; }

        public SKImage Bitmap
        {
            get
            {
                if (Type == BitmapType.Bitmap)
                    return (SKImage) _data;
                else
                    return null;
            }
            set
            {
                _data = value;
                Type = BitmapType.Bitmap;
            }
        }

        public SKSvg Svg
        {
            get
            {
                if (Type == BitmapType.Svg)
                    return (SKSvg) _data;
                else
                    return null;
            }
            set
            {
                _data = value;
                Type = BitmapType.Svg;
            }
        }

        public Sprite Sprite
        {
            get
            {
                if (Type == BitmapType.Sprite)
                    return (Sprite)_data;
                else
                    return null;
            }
            set
            {
                _data = value;
                Type = BitmapType.Sprite;
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
                        return Svg.Picture.CullRect.Width;
                    case BitmapType.Sprite:
                        return ((Sprite) _data).Width;
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
                        return Svg.Picture.CullRect.Height;
                    case BitmapType.Sprite:
                        return ((Sprite) _data).Height;
                    default:
                        return 0;
                }
            }
        }
    }
}
