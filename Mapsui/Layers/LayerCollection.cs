using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Fetcher;

namespace Mapsui.Layers
{
    public class LayerCollection : ICollection<ILayer>
    {
        private readonly IList<ILayer> _layers = new List<ILayer>();
        
        public delegate void LayerRemovedEventHandler(ILayer layer);
        public delegate void LayerAddedEventHandler(ILayer layer);
        public delegate void LayerMovedEventHandler(ILayer layer);

        public event LayerRemovedEventHandler LayerRemoved;
        public event LayerAddedEventHandler LayerAdded;
        public event LayerMovedEventHandler LayerMoved;

        public int Count => _layers.Count;

        public bool IsReadOnly => _layers.IsReadOnly;

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
                if (layer is IAsyncDataFetcher asyncLayer)
                {
                    asyncLayer.AbortFetch();
                    asyncLayer.ClearCache();
                }
                OnLayerRemoved(layer);
            }
            _layers.Clear();
        }

        public bool Contains(ILayer item)
        {
            return _layers.Contains(item);
        }

        public void CopyTo(ILayer[] array, int arrayIndex)
        {
            _layers.CopyTo(array, arrayIndex);
        }

        public ILayer this[int index]
        {
            get { return _layers[index]; }
        }

        public void Add(ILayer layer)
        {
            if (layer == null) throw new ArgumentException("Layer cannot be null");
            _layers.Add(layer);
            OnLayerAdded(layer);
        }

        public void Move(int index, ILayer layer)
        {
            _layers.Remove(layer);
            _layers.Insert(index, layer);
            OnLayerMoved(layer);
        }

        public void Insert(int index, ILayer layer)
        {
            _layers.Insert(index, layer);
            OnLayerAdded(layer);
        }

        public bool Remove(ILayer layer)
        {
            var success = _layers.Remove(layer);
            if (layer is IAsyncDataFetcher asyncLayer)
            {
                asyncLayer.AbortFetch();
                asyncLayer.ClearCache();
            }
            OnLayerRemoved(layer);
            return success;
        }

        private void OnLayerRemoved(ILayer layer)
        {
            LayerRemoved?.Invoke(layer);
        }

        private void OnLayerAdded(ILayer layer)
        {
            LayerAdded?.Invoke(layer);
        }

        private void OnLayerMoved(ILayer layer)
        {
            LayerMoved?.Invoke(layer);
        }

        public IEnumerable<ILayer> FindLayer(string layername)
        {
            return _layers.Where(layer => layer.Name.Contains(layername));
        }
    }
}