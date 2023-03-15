using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Providers;
using NetTopologySuite.Index.Strtree;

namespace Mapsui.Nts.Providers;

public class IndexedMemoryProvider : IProvider
{
    private readonly MRect? _boundingBox;
    
    private STRtree<IFeature>? _index;

    private int _itemsIndexed;
    private int _itemsLookedUp;
    
    // lock object
    private object _lock = new();
    private ConcurrentDictionary<string, Dictionary<object, IFeature>> _lookups = new();
    
    /// <summary>
    /// Gets or sets the geometries this data source contains
    /// </summary>

    public IndexedMemoryProvider()
    {
        Features = new List<IFeature>();
        _boundingBox = MemoryProvider.GetExtent(Features);
    }

    private STRtree<IFeature> GetIndex()
    {
        if (Features.Count != _itemsIndexed)
        {
            _index = null;
        }
        
        if (_index == null)
        {
            lock (_lock)
            {
                if (_index == null)
                {
                    _index = new();
                    foreach (var feature in Features)
                    {
                        var envelope = feature.Extent.ToEnvelope();
                        _index.Insert(envelope, feature);
                    }

                    _itemsIndexed = Features.Count;
                }
            }
        }

        return _index;
    }

    /// <summary>
    /// Initializes a new instance of the IndxedMemoryProvider
    /// </summary>
    /// <param name="feature">Feature to be in this dataSource</param>
    public IndexedMemoryProvider(IFeature feature)
    {
        Features = new List<IFeature> { feature };
        _boundingBox = MemoryProvider.GetExtent(Features);
    }

    public IReadOnlyList<IFeature> Features { get; private set; }
    public double SymbolSize { get; set; } = 64;

    /// <summary>
    /// The spatial reference ID (CRS)
    /// </summary>
    public string? CRS { get; set; }


    /// <summary>
    /// Initializes a new instance of the IndxedMemoryProvider
    /// </summary>
    /// <param name="features">Features to be included in this dataSource</param>
    public IndexedMemoryProvider(IEnumerable<IFeature> features)
    {
        Features = features.ToList();
        _boundingBox = MemoryProvider.GetExtent(Features);
    }

    public virtual Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        if (fetchInfo == null) throw new ArgumentNullException(nameof(fetchInfo));
        if (fetchInfo.Extent == null) throw new ArgumentNullException(nameof(fetchInfo.Extent));

        var index = GetIndex();
        
        fetchInfo = new FetchInfo(fetchInfo);
        // Use a larger extent so that symbols partially outside of the extent are included
        var biggerBox = fetchInfo.Extent?.Grow(fetchInfo.Resolution * SymbolSize * 0.5);
        
        var fetchExtent = biggerBox.ToEnvelope();
        IEnumerable<IFeature> result = index.Query(fetchExtent);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Search for a feature
    /// </summary>
    /// <param name="value">Value to search for</param>
    /// <param name="fieldName">Name of the field to search in. This is the key of the T dictionary</param>
    /// <returns></returns>
    public IFeature? Find(object? value, string fieldName)
    {
        if (value == null)
        {
            return null;
        }
        
        var lookup = GetLookup(fieldName);
        return lookup[value];
    }

    private Dictionary<object, IFeature> GetLookup(string fieldName)
    {
        if (Features.Count != _itemsLookedUp)
        {
            _lookups.Clear();
        }
        
        if (!_lookups.TryGetValue(fieldName, out var lookup))
        {
            lookup = new Dictionary<object, IFeature>();
            foreach (var feature in Features)
            {
                var val = feature[fieldName];
                if (val != null)
                {
                    lookup[val] = feature;
                }
            }

            _lookups = new();
            _itemsLookedUp = Features.Count;
        }

        return lookup;
    }

    /// <summary>
    /// BoundingBox of data set
    /// </summary>
    /// <returns>BoundingBox</returns>
    public MRect? GetExtent()
    {
        return _boundingBox;
    }

    public void Clear()
    {
        Features = new List<IFeature>();
        _index = null;
    }
}
