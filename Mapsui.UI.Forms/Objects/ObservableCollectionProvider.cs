using Mapsui.Geometries;
using Mapsui.Providers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapsui.Extensions;

namespace Mapsui.UI.Objects
{
    public class ObservableCollectionProvider<T, U> : IProvider<U> where T : IFeatureProvider where U : IFeature
    {
        private readonly object _syncRoot = new();

        public ObservableCollection<T> Collection { get; }

        public string CRS { get; set; } = "";

        public ObservableCollectionProvider(ObservableCollection<T> collection)
        {
            Collection = collection;
        }

        public IEnumerable<U> GetFeaturesInView(BoundingBox box, double resolution)
        {
            var list = new List<U>();

            if (Collection == null || Collection.Count == 0)
                return list;

            lock (_syncRoot)
            {
                foreach (T item in Collection)
                {
                    if (box.Intersects(item.Feature.BoundingBox.ToBoundingBox()))
                        list.Add((U)item.Feature);
                }
            }

            return list;
        }

        public BoundingBox GetExtents()
        {
            if (Collection == null || Collection.Count == 0)
                return null;

            BoundingBox extents = null;

            lock (_syncRoot)
            {
                foreach (T item in Collection)
                {
                    if (item.Feature != null)
                    {
                        if (item.Feature.BoundingBox != null)
                        {
                            if (extents == null)
                                extents = new BoundingBox(item.Feature.BoundingBox.ToBoundingBox());
                            else
                                extents = extents.Join(item.Feature.BoundingBox.ToBoundingBox());
                        }
                    }
                }
            }

            return extents;
        }
    }
}
