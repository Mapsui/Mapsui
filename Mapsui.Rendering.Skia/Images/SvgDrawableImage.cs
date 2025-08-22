using Mapsui.Rendering.Skia.Extensions;
using SkiaSharp;
using Svg.Skia;

#pragma warning disable IDISP008 // SvgDrawableImage is responsible for disposing
#pragma warning disable IDISP007 // SvgDrawableImage is responsible for disposing

namespace Mapsui.Rendering.Skia.Images;

public sealed class SvgDrawableImage : IDrawableImage
{
    private bool _disposed;
    private readonly SKSvg? _skSvg;
    private readonly SKPicture? _picture;

    public SvgDrawableImage(byte[] bytes)
    {
        _skSvg = bytes.LoadSvg();
        // Perhaps we should dispose the SKSvg but I fear this will dispose the SKSvg.Picture as well. Todo: investigate
        OriginalStream = bytes;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SvgDrawableImage"/> class with the specified SKPicture.
    /// </summary>
    /// <param name="picture">The SKPicture to be used for the image. SvgDrawableImage will dispose it.</param>
    public SvgDrawableImage(SKPicture picture)
    {
        _picture = picture;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SvgDrawableImage"/> class with the specified SKPicture.
    /// </summary>
    /// <param name="skSvg">The SKSvg to be used for the image. SvgDrawableImage will dispose it. </param>
    public SvgDrawableImage(SKSvg skSvg)
    {
        _skSvg = skSvg;
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
        _picture?.Dispose();
        _disposed = true;
    }
}
