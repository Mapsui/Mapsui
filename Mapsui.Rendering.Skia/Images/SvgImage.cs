using Mapsui.Rendering.Skia.Extensions;
using SkiaSharp;
using Svg.Skia;

namespace Mapsui.Rendering.Skia.Images;

public sealed class SvgImage : IDrawableImage
{
    private bool _disposed;
    private readonly SKSvg? _skSvg;
    private readonly SKPicture? _picture;

    public SvgImage(byte[] bytes)
    {
        _skSvg = bytes.LoadSvg();
        // Perhaps we should dispose the SKSvg but I fear this will dispose the SKSvg.Picture as well. Todo: investigate
        OriginalStream = bytes;
    }

    public SvgImage(SKPicture picture)
    {
        _picture = picture;
    }

    public SKPicture Picture => _skSvg?.Picture is null ? _picture! : _skSvg.Picture!;
    public byte[]? OriginalStream { get; }
    public float Width => Picture.CullRect.Width;
    public float Height => Picture.CullRect.Height;

    public void Dispose()
    {
        if (_disposed)
            return;

        _skSvg?.Dispose();
#pragma warning disable IDISP007
        _picture?.Dispose();
#pragma warning restore IDISP007

        _disposed = true;
    }
}
