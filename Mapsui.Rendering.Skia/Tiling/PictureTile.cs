using SkiaSharp;

namespace Mapsui.Rendering.Skia.Tiling;

public sealed class PictureTile(SKPicture picture) : IRenderedTile
{
    private readonly SKPicture _picture = picture;

    public long IterationUsed { get; set; }
    public SKPicture Picture => _picture;

    public void Dispose() => _picture.Dispose();
}

