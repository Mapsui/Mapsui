using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.UI.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (Collection == null || Collection.Count == 0)
                return null;

            var list = new List<IFeature>();

            foreach (T item in Collection)
            {
                if (box.Contains(item.GetFeature().Geometry.GetBoundingBox()))
                    list.Add(item.GetFeature());
            }

            return list;
        }

        public BoundingBox GetExtents()
        {
            if (Collection == null || Collection.Count == 0)
                return null;

            BoundingBox extend = new BoundingBox(Collection[0].GetFeature().Geometry.GetBoundingBox());

            foreach(T item in Collection)
            {
                extend.Join(item.GetFeature().Geometry.GetBoundingBox());
            }

            return extend;
        }
    }
}
