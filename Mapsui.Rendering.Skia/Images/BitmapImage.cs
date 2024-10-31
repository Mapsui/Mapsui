using SkiaSharp;

namespace Mapsui.Rendering.Skia.Images;

internal sealed class BitmapImage : IDrawableImage
{
    private bool _disposed;
    private readonly SKImage _image;

    public BitmapImage(SKImage image)
    {
        _image = image;
    }

    public BitmapImage(byte[] bytes)
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
