using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Layers;

/// <summary>
/// The MemoryLayer has all features in memory.
/// </summary>
public class MemoryLayer : BaseLayer
{
    /// <summary>
    /// Create a new layer
    /// </summary>
    public MemoryLayer() : this(nameof(MemoryLayer)) { }

    /// <summary>
    /// Create layer with name
    /// </summary>
    /// <param name="layerName">Name to use for layer</param>
    public MemoryLayer(string layerName) : base(layerName) { }

    public IEnumerable<IFeature> Features { get; set; } = new ConcurrentBag<IFeature>();


    public override IEnumerable<IFeature> GetFeatures(MRect? rect, double resolution)
    {
        // Safeguard in case BoundingBox is null, most likely due to no features in layer
        if (rect == null) { return new List<IFeature>(); }

        var biggerRect = rect.Grow(
                SymbolStyle.DefaultWidth * 2 * resolution,
                SymbolStyle.DefaultHeight * 2 * resolution);

        return Features.Where(f => f.Extent?.Intersects(biggerRect) == true);
    }

    public override MRect? Extent => Features.GetExtent();
}
