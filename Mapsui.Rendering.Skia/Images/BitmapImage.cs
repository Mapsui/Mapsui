using SkiaSharp;

namespace Mapsui.Rendering.Skia.Images;

internal sealed class BitmapImage(SKImage skImage) : IDrawableImage
{
    private bool _disposed;

    public SKImage Image => skImage;
    public float Width => skImage.Width;
    public float Height => skImage.Height;

    public bool IsDisposed() => _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        skImage.Dispose();

        _disposed = true;
    }
}
