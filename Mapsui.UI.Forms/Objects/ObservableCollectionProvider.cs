﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.UI.Objects
{
    public class ObservableCollectionProvider<T, TU> : IProvider<TU> where T : IFeatureProvider where TU : IFeature
    {
        private readonly object _syncRoot = new();

        public ObservableCollection<T>? Collection { get; }

        public string? CRS { get; set; } = "";

        public ObservableCollectionProvider(ObservableCollection<T> collection)
        {
            Collection = collection;
        }

        public IAsyncEnumerable<TU> GetFeatures(FetchInfo fetchInfo)
        {
            var list = new List<TU>();

            if (Collection == null || Collection.Count == 0)
                return list.ToAsyncEnumerable();

            lock (_syncRoot)
            {
                foreach (var item in Collection)
                {
                    if (fetchInfo.Extent?.Intersects(item.Feature?.Extent) ?? false)
                    {
                        IFeature feature = item.Feature!;
                        list.Add((TU)feature);
                    }
                }
            }

            return list.ToAsyncEnumerable();
        }

        public MRect? GetExtent()
        {
            if (Collection == null || Collection.Count == 0)
                return null;

            MRect? extent = null;

            lock (_syncRoot)
            {
                foreach (var item in Collection)
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
            }

            return extent;
        }
    }
}
