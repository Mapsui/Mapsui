using Mapsui.Geometries;
using Mapsui.Providers;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mapsui.UI.Objects
{
    public class ObservableCollectionProvider<T, U> : IProvider<U> where T : IFeatureProvider where U : IFeature
    {
        private object _syncRoot = new object();

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
                    if (box.Intersects(item.Feature.BoundingBox))
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
                                extents = new BoundingBox(item.Feature.BoundingBox);
                            else
                                extents = extents.Join(item.Feature.BoundingBox);
                        }
                    }
                }
            }

            return extents;
        }
    }
}
