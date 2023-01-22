using Mapsui.Styles;
using SkiaSharp;
using Svg.Skia;

namespace Mapsui.Rendering.Skia;

public enum BitmapType
{
    Bitmap,
    Svg,
    Sprite,
    Picture
}

public class BitmapInfo : IBitmapInfo
{
    private object? _data;

    public BitmapType Type { get; private set; }

    public SKImage? Bitmap
    {
        get
        {
            if (Type == BitmapType.Bitmap)
                return _data as SKImage;
            else
                return null;
        }
        set
        {
            _data = value;
            Type = BitmapType.Bitmap;
        }
    }

    public SKPicture? Picture
    {
        get
        {
            if (Type == BitmapType.Picture)
                return _data as SKPicture;
            else
                return null;
        }
        set
        {
            _data = value;
            Type = BitmapType.Picture;
        }
    }

    public SKSvg? Svg
    {
        get
        {
            if (Type == BitmapType.Svg)
                return _data as SKSvg;
            else
                return null;
        }
        set
        {
            _data = value;
            Type = BitmapType.Svg;
        }
    }

    public Sprite? Sprite
    {
        get
        {
            if (Type == BitmapType.Sprite)
                return _data as Sprite;
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
                    return Bitmap?.Width ?? 0;
                case BitmapType.Svg:
                    return Svg?.Picture?.CullRect.Width ?? 0;
                case BitmapType.Sprite:
                    return Sprite?.Width ?? 0;
                case BitmapType.Picture:
                    return Picture?.CullRect.Width ?? 0;
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
                    return Bitmap?.Height ?? 0;
                case BitmapType.Svg:
                    return Svg?.Picture?.CullRect.Height ?? 0;
                case BitmapType.Sprite:
                    return Sprite?.Height ?? 0;
                case BitmapType.Picture:
                    return Picture?.CullRect.Height ?? 0;
                default:
                    return 0;
            }
        }
    }
}
