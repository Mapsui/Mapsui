using Mapsui.Extensions;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Layers;

/// <summary>
/// The MemoryLayer has all features in memory.
/// </summary>
/// <remarks>
/// Create layer with name
/// </remarks>
/// <param name="layerName">Name to use for layer</param>
public class MemoryLayer(string layerName) : BaseLayer(layerName)
{
    /// <summary>
    /// Create a new layer
    /// </summary>
    public MemoryLayer() : this(nameof(MemoryLayer)) { }

    public IEnumerable<IFeature> Features { get; set; } = [];

    public override IEnumerable<IFeature> GetFeatures(MRect? rect, double resolution)
    {
        // Safeguard in case BoundingBox is null, most likely due to no features in layer
        if (rect == null) { return []; }

        var biggerRect = rect.Grow(
                SymbolStyle.DefaultWidth * 2 * resolution,
                SymbolStyle.DefaultHeight * 2 * resolution);

        return Features.Where(f => f.Extent?.Intersects(biggerRect) == true);
    }

    public override Func<IEnumerable<IFeature>, IEnumerable<IFeature>> SortFeatures { get; set; } = (features) => features.OrderBy(f => f.ZOrder).ThenBy(f => f.Id);

    public override MRect? Extent => Features.GetExtent();
}
