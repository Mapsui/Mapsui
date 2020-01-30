using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public enum BitmapType
    {
        Bitmap,
        Svg,
        Sprite,
        Picture,
        Drawable
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

        public SkiaSharp.Extended.Svg.SKSvg Svg
        {
            get
            {
                if (Type == BitmapType.Svg)
                    return (SkiaSharp.Extended.Svg.SKSvg) _data;
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

        public SKPicture Picture
        {
            get
            {
                if (Type == BitmapType.Picture)
                    return (SKPicture)_data;
                else
                    return null;
            }
            set
            {
                _data = value;
                Type = BitmapType.Picture;
            }
        }

        public SKDrawable Drawable
        {
            get
            {
                if (Type == BitmapType.Drawable)
                    return (SKDrawable)_data;
                else
                    return null;
            }
            set
            {
                _data = value;
                Type = BitmapType.Drawable;
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
                    case BitmapType.Sprite:
                        return ((Sprite) _data).Width;
                    case BitmapType.Picture:
                        return ((SKPicture)_data).CullRect.Width;
                    case BitmapType.Drawable:
                        return ((SKDrawable)_data).Bounds.Width;
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
                    case BitmapType.Sprite:
                        return ((Sprite) _data).Height;
                    case BitmapType.Picture:
                        return ((SKPicture)_data).CullRect.Height;
                    case BitmapType.Drawable:
                        return ((SKDrawable)_data).Bounds.Height;
                    default:
                        return 0;
                }
            }
        }
    }
}
