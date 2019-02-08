using System;
using System.Collections.Generic;
using System.Linq;
using ConcurrentCollections;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Layers
{
    public  class WritableLayer : BaseLayer
    {
        private readonly ConcurrentHashSet<IFeature> _cache = new ConcurrentHashSet<IFeature>();

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            // Safeguard in case BoundingBox is null, most likely due to no features in layer
            if (box == null) { return new List<IFeature>(); }
            var cache = _cache;
            var biggerBox = box.Grow(SymbolStyle.DefaultWidth * 2 * resolution, SymbolStyle.DefaultHeight * 2 * resolution);
            var result = cache.Where(f => biggerBox.Intersects(f.Geometry?.BoundingBox));
            return result;
        }

        private BoundingBox GetExtents()
        {
            // todo: Calculate extents only once. Use a _modified field to determine when this is needed.

            var geometries = _cache
                .Select(f => f.Geometry)
                .Where(g => g != null && !g.IsEmpty() && g.BoundingBox != null)
                .ToList();

            if (geometries.Count == 0) return null;

            var minX = geometries.Min(g => g.BoundingBox.MinX);
            var minY = geometries.Min(g => g.BoundingBox.MinY);
            var maxX = geometries.Max(g => g.BoundingBox.MaxX);
            var maxY = geometries.Max(g => g.BoundingBox.MaxY);

            return new BoundingBox(minX, minY, maxX, maxY);
        }

        public override BoundingBox Envelope => GetExtents();

        public override void RefreshData(BoundingBox extent, double resolution, bool majorChange)
        {
            //The MemoryLayer always has it's data ready so can fire a DataChanged event immediately so that listeners can act on it.
            OnDataChanged(new DataChangedEventArgs());
        }
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

        public IFeature Find(IFeature feature)
        {
            return _cache.FirstOrDefault(f => f == feature);
        }

        [Obsolete("Use DataHasChanged instead", true)]
        public void SignalDataChanged() {}

        /// <summary>
        /// Tries to remove a feature.
        /// </summary>
        /// <param name="feature">Feature to remove</param>
        /// <param name="compare">Optional method to compare the feature with any of the other 
        /// features in the list. If omitted a reference compare is done.</param>
        /// <returns></returns>
        public bool TryRemove(IFeature feature, Func<IFeature, IFeature, bool> compare = null)
        {
            if (compare == null) return _cache.TryRemove(feature);

            return _cache.TryRemove(_cache.FirstOrDefault(f => compare(f, feature)));
        }
    }
}
