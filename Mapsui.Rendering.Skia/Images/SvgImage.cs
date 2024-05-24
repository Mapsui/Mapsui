using SkiaSharp;
using Svg.Skia;
using System.IO;

namespace Mapsui.Rendering.Skia.Images;

internal sealed class SvgImage(Stream stream) : IDrawableImage
{
    private bool _disposed;
    private readonly SKSvg _skSvg = stream.LoadSvg();

    public SKPicture Picture => _skSvg.Picture!;
    public Stream OriginalStream { get; } = stream;
    public float Width => Picture.CullRect.Width;
    public float Height => Picture.CullRect.Height;

    public bool IsDisposed() => _disposed;

    public void Dispose()
    {
        if (_disposed)
            return;

        _skSvg.Dispose();
        OriginalStream.Dispose();

        _disposed = true;
    }
}
