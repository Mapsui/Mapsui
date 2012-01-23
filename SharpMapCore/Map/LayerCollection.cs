using System.Linq;
using System.Collections.Generic;
using SharpMap.Fetcher;
using SharpMap.Layers;
using System.Collections;

namespace SharpMap
{
    public class LayerCollection : IEnumerable<ILayer>
    {
        private readonly IList<ILayer> _layers = new List<ILayer>();
        
        public delegate void LayerRemovedEventHandler(ILayer layer);
        public delegate void LayerAddedEventHandler(ILayer layer);

        public event LayerRemovedEventHandler LayerRemoved;
        public event LayerAddedEventHandler LayerAdded;

        public int Count
        {
            get { return _layers.Count(); }
        }

        public IEnumerator<ILayer> GetEnumerator()
        {
            return _layers.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _layers.GetEnumerator();
        }

        public void Clear()
        {
            foreach (var layer in _layers)
            {
                if (layer is IAsyncDataFetcher)
                {
                    (layer as IAsyncDataFetcher).AbortFetch();
                }
                OnLayerRemoved(layer);
            }
            _layers.Clear();
        }

        public ILayer this[int index]
        {
            get { return _layers[index]; }
        }

        public void Add(ILayer layer)
        {
            _layers.Add(layer);
            OnLayerAdded(layer);
        }

        public void Insert(int index, ILayer layer)
        {
            _layers.Insert(index, layer);
            OnLayerAdded(layer);
        }

        public void Remove(ILayer layer)
        {
            _layers.Remove(layer);
            if (layer is IAsyncDataFetcher)
            {
                (layer as IAsyncDataFetcher).AbortFetch();
            }

            OnLayerRemoved(layer);
        }

        private void OnLayerRemoved(ILayer layer)
        {
            if (LayerRemoved != null) LayerRemoved(layer);
        }

        private void OnLayerAdded(ILayer layer)
        {
            if (LayerAdded != null) LayerAdded(layer);
        }

        public IEnumerable<ILayer> FindLayer(string layername)
        {
            return _layers.Where(layer => layer.LayerName.Contains(layername));
        }
    }
}
