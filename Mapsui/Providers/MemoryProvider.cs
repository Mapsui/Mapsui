using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Layers;

namespace Mapsui.Providers;

public class MemoryProvider : IProvider
{
    private readonly MRect? _boundingBox;
    /// <summary>
    /// Gets or sets the geometries this data source contains
    /// </summary>

    public MemoryProvider()
    {
        Features = [];
        _boundingBox = GetExtent(Features);
    }

    /// <summary>
    /// Initializes a new instance of the MemoryProvider
    /// </summary>
    /// <param name="feature">Feature to be in this dataSource</param>
    public MemoryProvider(IFeature feature)
    {
        Features = [feature];
        _boundingBox = GetExtent(Features);
    }

    public IReadOnlyList<IFeature> Features { get; private set; }
    public double SymbolSize { get; set; } = 64;

    /// <summary>
    /// The spatial reference ID (CRS)
    /// </summary>
    public string? CRS { get; set; }


    /// <summary>
    /// Initializes a new instance of the MemoryProvider
    /// </summary>
    /// <param name="features">Features to be included in this dataSource</param>
    public MemoryProvider(IEnumerable<IFeature> features)
    {
        Features = features.ToList();
        _boundingBox = GetExtent(Features);
    }


    public virtual Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(fetchInfo);
        ArgumentNullException.ThrowIfNull(fetchInfo.Extent);

        var features = Features.ToArray(); // An Array is faster than a List

        fetchInfo = new FetchInfo(fetchInfo);
        // Use a larger extent so that symbols partially outside of the extent are included
        var biggerBox = fetchInfo.Extent?.Grow(fetchInfo.Resolution * SymbolSize * 0.5);
        var result = features.Where(f => f != null && (f.Extent?.Intersects(biggerBox) ?? false)).ToList();
        return Task.FromResult((IEnumerable<IFeature>)result);
    }

    /// <summary>
    /// Search for a feature
    /// </summary>
    /// <param name="value">Value to search for</param>
    /// <param name="fieldName">Name of the field to search in. This is the key of the T dictionary</param>
    /// <returns></returns>
    public IFeature? Find(object? value, string fieldName)
    {
        return Features.FirstOrDefault(f => value != null && f[fieldName] == value);
    }

    /// <summary>
    /// BoundingBox of data set
    /// </summary>
    /// <returns>BoundingBox</returns>
    public MRect? GetExtent()
    {
        return _boundingBox;
    }

    internal static MRect? GetExtent(IReadOnlyList<IFeature> features)
    {
        MRect? box = null;
        foreach (var feature in features)
        {
            if (feature.Extent == null) continue;
            box = box == null
                ? feature.Extent
                : box.Join(feature.Extent);
        }
        return box;
    }

    public void Clear()
    {
        Features = [];
    }
}
