using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Layers;

public class WritableLayer : BaseLayer
{
    private readonly ConcurrentHashSet<IFeature> _cache = [];

    public override IEnumerable<IFeature> GetFeatures(MRect? box, double resolution)
    {
        // Safeguard in case MRect is null, most likely due to no features in layer
        if (box == null) return new List<IFeature>();
        var cache = _cache;
        var biggerBox = box.Grow(SymbolStyle.DefaultWidth * 2 * resolution, SymbolStyle.DefaultHeight * 2 * resolution);
        var result = cache.Where(f => biggerBox.Intersects(f.Extent));
        return result;
    }

    private MRect? GetExtent()
    {
        // Todo: Calculate extent only once. Use a _modified field to determine when this is needed.

        var extents = _cache
            .Where(f => f.Extent is not null)
            .Select(f => f.Extent!)
            .ToList();

        if (extents.Count == 0) return null;

        var minX = extents.Min(e => e.MinX);
        var minY = extents.Min(e => e.MinY);
        var maxX = extents.Max(e => e.MaxX);
        var maxY = extents.Max(e => e.MaxY);

        return new MRect(minX, minY, maxX, maxY);
    }

    public override MRect? Extent => GetExtent();

    public IEnumerable<IFeature> GetFeatures()
    {
        return _cache;
    }

    public void Clear()
    {
        _cache.Clear();
    }

    public void Add(IFeature feature)
    {
        _cache.Add(feature);
    }

    public void AddRange(IEnumerable<IFeature> features)
    {
        foreach (var feature in features)
        {
            _cache.Add(feature);
        }
    }

    public IFeature? Find(IFeature feature)
    {
        return _cache.FirstOrDefault(f => f == feature);
    }

    /// <summary>
    /// Tries to remove a feature.
    /// </summary>
    /// <param name="feature">Feature to remove</param>
    /// <param name="compare">Optional method to compare the feature with any of the other 
    /// features in the list. If omitted a reference compare is done.</param>
    /// <returns></returns>
    public bool TryRemove(IFeature feature, Func<IFeature, IFeature, bool>? compare = null)
    {
        if (compare == null) return _cache.TryRemove(feature);
        var item = _cache.FirstOrDefault(f => compare(f, feature));
        if (item == null) return false;
        return _cache.TryRemove(item);
    }
}
