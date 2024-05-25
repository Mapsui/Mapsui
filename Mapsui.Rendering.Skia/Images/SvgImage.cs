using SkiaSharp;
using Svg.Skia;

namespace Mapsui.Rendering.Skia.Images;

internal sealed class SvgImage(byte[] bytes) : IDrawableImage
{
    private bool _disposed;
    private readonly SKSvg _skSvg = bytes.LoadSvg();

    public SKPicture Picture => _skSvg.Picture!;
    public byte[] OriginalStream { get; } = bytes;
    public float Width => Picture.CullRect.Width;
    public float Height => Picture.CullRect.Height;

    public bool IsDisposed() => _disposed;

    public void Dispose()
    {
        if (_disposed)
            return;

        _skSvg.Dispose();

        _disposed = true;
    }
}
