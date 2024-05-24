using SkiaSharp;

namespace Mapsui.Rendering.Skia.Tiling;

public sealed class ImageTile(SKImage image) : IRenderedTile
{
    private readonly SKImage _image = image;

    public long IterationUsed { get; set; }
    public SKImage Image => _image;

    public bool IsDisposed() => false;
    public void Dispose() => _image.Dispose();
}

