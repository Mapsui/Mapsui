using Mapsui.Geometries;
using Mapsui.Providers;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mapsui.UI.Objects
{
    public class ObservableCollectionProvider<T> : IProvider where T : IFeatureProvider
    {
        public ObservableCollection<T> Collection { get; }

        public string CRS { get; set; } = "";

        public ObservableCollectionProvider(ObservableCollection<T> collection)
        {
            Collection = collection;
        }

        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            var list = new List<IFeature>();

            if (Collection == null || Collection.Count == 0)
                return list;

            foreach (T item in Collection)
            {
                if (box.Intersects(item.Feature.Geometry.GetBoundingBox()))
                    list.Add(item.Feature);
            }

            return list;
        }

        public BoundingBox GetExtents()
        {
            if (Collection == null || Collection.Count == 0)
                return null;

            BoundingBox extents = null;

            foreach(T item in Collection)
            {
                if (item.Feature != null)
                {
                    if (extents == null)
                        extents = new BoundingBox(item.Feature.Geometry.GetBoundingBox());
                    else
                        extents = extents.Join(item.Feature.Geometry.GetBoundingBox());
                }
            }

            return extents;
        }
    }
}
