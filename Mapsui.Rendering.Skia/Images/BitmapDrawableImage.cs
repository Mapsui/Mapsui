using SkiaSharp;

namespace Mapsui.Rendering.Skia.Images;

public sealed class BitmapDrawableImage : IDrawableImage
{
    private bool _disposed;
    private readonly SKImage _image;

    public BitmapDrawableImage(SKImage image)
    {
        _image = image;
    }

    public BitmapDrawableImage(byte[] bytes)
    {
        using var skData = SKData.CreateCopy(bytes);
        _image = SKImage.FromEncodedData(skData);
    }

    public SKImage Image => _image;
    public float Width => _image.Width;
    public float Height => _image.Height;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

#pragma warning disable IDISP007
        _image.Dispose();
#pragma warning restore IDISP007

        _disposed = true;
    }
}
