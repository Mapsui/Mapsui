using Mapsui.Rendering;
using Mapsui.Styles;
using SkiaSharp;
using System;

namespace Mapsui.Experimental.Rendering.Skia.DrawableRenderers;

/// <summary>
/// Drawable for a decoded raster tile (SKImage or SKPicture).
/// The <see cref="Data"/> is owned by this drawable and will be disposed when evicted
/// from the <see cref="TileDrawableCache"/>.
/// </summary>
public sealed class RasterStyleDrawable : IDrawable
{
    /// <summary>
    /// The decoded tile data — either an <see cref="SKImage"/> or an <see cref="SKPicture"/>.
    /// Owned by this drawable.
    /// </summary>
    public SKObject Data { get; }

    /// <summary>
    /// The world extent of the tile (for positioning on the map).
    /// </summary>
    public MRect Extent { get; }

    /// <summary>
    /// Combined opacity (layer × style).
    /// </summary>
    public float Opacity { get; }

    /// <summary>
    /// Optional outline pen for drawing tile borders.
    /// </summary>
    public Pen? Outline { get; }

    /// <summary>
    /// Initializes a new <see cref="RasterStyleDrawable"/>.
    /// </summary>
    /// <param name="data">The decoded tile data (SKImage or SKPicture). Owned by this drawable.</param>
    /// <param name="extent">The world extent of the tile for positioning on the map.</param>
    /// <param name="opacity">The combined opacity (layer × style).</param>
    /// <param name="outline">Optional outline pen for tile borders.</param>
    public RasterStyleDrawable(SKObject data, MRect extent, float opacity, Pen? outline)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Extent = extent;
        Opacity = opacity;
        Outline = outline;
        WorldX = extent.Centroid.X;
        WorldY = extent.Centroid.Y;
    }

    /// <inheritdoc />
    public double WorldX { get; }

    /// <inheritdoc />
    public double WorldY { get; }

    /// <summary>
    /// Disposes the owned <see cref="Data"/> (SKImage or SKPicture).
    /// </summary>
    public void Dispose()
    {
#pragma warning disable IDISP007 // Don't dispose injected - Data is created by the renderer and owned by this drawable
        Data.Dispose();
#pragma warning restore IDISP007
    }
}
