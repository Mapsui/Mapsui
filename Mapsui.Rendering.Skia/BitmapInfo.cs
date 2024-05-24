using Mapsui.Extensions;
using SkiaSharp;
using Svg.Skia;
using System.IO;

namespace Mapsui.Rendering.Skia;

public enum BitmapType
{
    Bitmap,
    Svg,
}

public sealed class BitmapInfo
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

    public SvgWithStream? Svg
    {
        get
        {
            if (Type == BitmapType.Svg)
                return _data as SvgWithStream;
            else
                return null;
        }
        set
        {
            _data = value;
            Type = BitmapType.Svg;
        }
    }

    public long IterationUsed { get; set; }

    public float Width
    {
        get
        {
            return Type switch
            {
                BitmapType.Bitmap => Bitmap?.Width ?? 0,
                BitmapType.Svg => Svg?.SKSvg.Picture?.CullRect.Width ?? 0,
                _ => 0,
            };
        }
    }

    public float Height
    {
        get
        {
            return Type switch
            {
                BitmapType.Bitmap => Bitmap?.Height ?? 0,
                BitmapType.Svg => Svg?.SKSvg.Picture?.CullRect.Height ?? 0,
                _ => 0,
            };
        }
    }

    public bool IsDisposed => _data == null;

    public void Dispose()
    {
        DisposableExtension.DisposeAndNullify(ref _data);
    }
}

// This class should be removed before the next beta
public record SvgWithStream(SKSvg SKSvg, Stream OriginalStream)
{ }
