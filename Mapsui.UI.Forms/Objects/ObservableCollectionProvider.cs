using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.UI.Objects
{
    public class ObservableCollectionProvider<T> : IProvider where T : IFeatureProvider
    {
        public ObservableCollection<T>? Collection { get; }
        private readonly ConcurrentHashSet<T> _shadowCollection = new(); 

        public string? CRS { get; set; } = "";

        public ObservableCollectionProvider(ObservableCollection<T> collection)
        {
            Collection = collection;
            collection.CollectionChanged += Collection_CollectionChanged;
        }

        private void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                    {
                        foreach (var it in e.OldItems)
                        {
                            _shadowCollection.TryRemove((T)it);
                        }
                    }

                    if (e.NewItems != null)
                    {
                        foreach (var it in e.NewItems)
                        {
                            _shadowCollection.Add((T)it);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    _shadowCollection.Clear();
                    foreach (var it in Collection)
                    {
                        _shadowCollection.Add(it);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    // do nothing
                    break;
            }
        }

        public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
        {
            if (Collection == null || Collection.Count == 0)
                return Enumerable.Empty<IFeature>();

            var list = new List<IFeature>();
            foreach (var item in _shadowCollection)
            {
                if (fetchInfo.Extent?.Intersects(item.Feature?.Extent) ?? false)
                {
                    IFeature feature = item.Feature!;
                    list.Add(feature);
                }
            }

            return list;
        }

        public MRect? GetExtent()
        {
            if (Collection == null || Collection.Count == 0)
                return null;

            MRect? extent = null;

            foreach (var item in _shadowCollection)
            {
                if (item.Feature != null)
                {
                    if (item.Feature.Extent != null)
                    {
                        if (extent == null)
                            extent = new MRect(item.Feature.Extent);
                        else
                            extent = extent.Join(item.Feature.Extent);
                    }
                }
            }

            return extent;
        }
    }
}
