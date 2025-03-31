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
    private IEnumerable<IFeature> _features = [];
    private IFeature[] _localFeatures = [];
    private MRect? _extent;

    /// <summary>
    /// Create a new layer
    /// </summary>
    public MemoryLayer() : this(nameof(MemoryLayer)) { }

    public IEnumerable<IFeature> Features
    {
        get => _features;
        set
        {
            _features = value;
            FeaturesWereModified();
        }
    }

    public void FeaturesWereModified()
    {
        _localFeatures = _features.ToArray();
        _extent = _localFeatures.GetExtent();
    }

    public override IEnumerable<IFeature> GetFeatures(MRect? rect, double resolution)
    {
        if (rect == null)
            yield break;

        var biggerRect = rect.Grow(
                SymbolStyle.DefaultWidth * 2 * resolution,
                SymbolStyle.DefaultHeight * 2 * resolution);
        foreach (var feature in _localFeatures)
        {
            if (feature?.Extent?.Intersects(biggerRect) == true)
                yield return feature;
        }
    }

    public override Func<IEnumerable<IFeature>, IEnumerable<IFeature>> SortFeatures { get; set; } = (_localFeatures) => _localFeatures.OrderBy(f => f.Id);

    public override MRect? Extent => _extent;
}
