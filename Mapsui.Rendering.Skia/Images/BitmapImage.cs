using Mapsui.Extensions;
using SkiaSharp;
using System.IO;

namespace Mapsui.Rendering.Skia.Images;

internal sealed class BitmapImage : IDrawableImage
{
    private bool _disposed;
    private readonly SKImage _image;

    public BitmapImage(SKImage image)
    {
        _image = image;
    }

    public BitmapImage(Stream stream)
    {
        using var skData = SKData.CreateCopy(stream.ToBytes());
        _image = SKImage.FromEncodedData(skData);
    }

    public SKImage Image => _image;
    public float Width => _image.Width;
    public float Height => _image.Height;

    public bool IsDisposed() => _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _image.Dispose();

        _disposed = true;
    }
}
