using Mapsui.Rendering;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.DrawableRenderers;

/// <summary>
/// Drawable for a rendered VexTile image.
/// The <see cref="Image"/> is owned by this drawable and will be disposed when evicted
/// from the <see cref="TileDrawableCache"/>.
/// </summary>
public sealed class VexTileStyleDrawable : IDrawable
{
    /// <summary>
    /// The pre-rendered tile image. Owned by this drawable.
    /// </summary>
    public SKImage Image { get; }

    /// <summary>
    /// The world extent of the tile (for positioning on the map).
    /// </summary>
    public MRect Extent { get; }

    /// <summary>
    /// Combined opacity (layer × style).
    /// </summary>
    public float Opacity { get; }

    /// <summary>
    /// Initializes a new <see cref="VexTileStyleDrawable"/>.
    /// </summary>
    /// <param name="image">The pre-rendered tile image (owned by this drawable).</param>
    /// <param name="extent">The world extent of the tile for positioning on the map.</param>
    /// <param name="opacity">The combined opacity (layer × style).</param>
    public VexTileStyleDrawable(SKImage image, MRect extent, float opacity)
    {
        Image = image;
        Extent = extent;
        Opacity = opacity;
        WorldX = extent.Centroid.X;
        WorldY = extent.Centroid.Y;
    }

    /// <inheritdoc />
    public double WorldX { get; }

    /// <inheritdoc />
    public double WorldY { get; }

    /// <summary>
    /// Disposes the owned <see cref="SKImage"/>.
    /// </summary>
    public void Dispose()
    {
#pragma warning disable IDISP007 // Don't dispose injected - Image IS owned by this drawable
        Image.Dispose();
#pragma warning restore IDISP007
    }
}
