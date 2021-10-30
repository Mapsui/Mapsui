using Mapsui.Geometries;
using Mapsui.Providers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers;

namespace Mapsui.UI.Objects
{
    public class ObservableCollectionProvider<T, TU> : IProvider<TU> where T : IFeatureProvider where TU : IFeature
    {
        private readonly object _syncRoot = new();

        public ObservableCollection<T> Collection { get; }

        public string CRS { get; set; } = "";

        public ObservableCollectionProvider(ObservableCollection<T> collection)
        {
            Collection = collection;
        }

        public IEnumerable<TU> GetFeatures(FetchInfo fetchInfo)
        {
            var list = new List<TU>();

            if (Collection == null || Collection.Count == 0)
                return list;

            lock (_syncRoot)
            {
                foreach (T item in Collection)
                {
                    if (fetchInfo.Extent.Intersects(item.Feature.BoundingBox))
                        list.Add((TU)item.Feature);
                }
            }

            return list;
        }

        public BoundingBox GetExtent()
        {
            if (Collection == null || Collection.Count == 0)
                return null;

            BoundingBox? extent = null;

            lock (_syncRoot)
            {
                foreach (T item in Collection)
                {
                    if (item.Feature != null)
                    {
                        if (item.Feature.BoundingBox != null)
                        {
                            if (extent == null)
                                extent = new BoundingBox(item.Feature.BoundingBox.ToBoundingBox());
                            else
                                extent = extent.Join(item.Feature.BoundingBox.ToBoundingBox());
                        }
                    }
                }
            }

            return extent;
        }
    }
}
