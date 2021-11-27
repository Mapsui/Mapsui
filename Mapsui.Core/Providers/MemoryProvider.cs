using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;

namespace Mapsui.Providers
{
    public class MemoryProvider<T> : IProvider<T> where T : IFeature
    {
        /// <summary>
        /// Gets or sets the geometries this data source contains
        /// </summary>
        public IReadOnlyList<T> Features { get; private set; }

        public double SymbolSize { get; set; } = 64;

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public string? CRS { get; set; }

        private readonly MRect? _boundingBox;

        public MemoryProvider()
        {
            Features = new List<T>();
            _boundingBox = GetExtent(Features);
        }

        /// <summary>
        /// Initializes a new instance of the MemoryProvider
        /// </summary>
        /// <param name="feature">Feature to be in this dataSource</param>
        public MemoryProvider(T feature)
        {
            Features = new List<T> { feature };
            _boundingBox = GetExtent(Features);
        }


        /// <summary>
        /// Initializes a new instance of the MemoryProvider
        /// </summary>
        /// <param name="features">Features to be included in this dataSource</param>
        public MemoryProvider(IEnumerable<T> features)
        {
            Features = features.ToList();
            _boundingBox = GetExtent(Features);
        }

        public virtual IEnumerable<T> GetFeatures(FetchInfo fetchInfo)
        {
            if (fetchInfo == null) throw new ArgumentNullException(nameof(fetchInfo));
            if (fetchInfo.Extent == null) throw new ArgumentNullException(nameof(fetchInfo.Extent));

            var features = Features.ToList();

            fetchInfo = new FetchInfo(fetchInfo);
            // Use a larger extent so that symbols partially outside of the extent are included
            var biggerBox = fetchInfo.Extent?.Grow(fetchInfo.Resolution * SymbolSize * 0.5);
            var grownFeatures = features.Where(f => f != null && (f.Extent?.Intersects(biggerBox) ?? false));

            return grownFeatures.ToList();
        }

        /// <summary>
        /// Search for a feature
        /// </summary>
        /// <param name="value">Value to search for</param>
        /// <param name="fieldName">Name of the field to search in. This is the key of the T dictionary</param>
        /// <returns></returns>
        public T? Find(object? value, string fieldName)
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

        private static MRect? GetExtent(IReadOnlyList<T> features)
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
            Features = new List<T>();
        }
    }
}