using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Fetcher;

namespace Mapsui.Layers
{
    public class LayerCollection : IEnumerable<ILayer>
    {
        private ConcurrentQueue<ILayer> _layers = new ConcurrentQueue<ILayer>();
        
        public delegate void LayerRemovedEventHandler(ILayer layer);
        public delegate void LayerAddedEventHandler(ILayer layer);
        public delegate void LayerMovedEventHandler(ILayer layer);

        public event LayerRemovedEventHandler LayerRemoved;
        public event LayerAddedEventHandler LayerAdded;
        public event LayerMovedEventHandler LayerMoved;

        public int Count => _layers.Count;

        public bool IsReadOnly => false;

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
            var copy = _layers.ToArray().ToList();

            foreach (var layer in copy)
            {
                if (layer is IAsyncDataFetcher asyncLayer)
                {
                    asyncLayer.AbortFetch();
                    asyncLayer.ClearCache();
                }
                OnLayerRemoved(layer);
            }

            _layers = new ConcurrentQueue<ILayer>(copy);
        }

        public bool Contains(ILayer item)
        {
            return _layers.Contains(item);
        }

        public void CopyTo(ILayer[] array, int arrayIndex)
        {
            var copy = _layers.ToArray().ToList();

            var maxCount = Math.Min(array.Length, copy.Count());
            var count = maxCount - arrayIndex;
            copy.CopyTo(0, array, arrayIndex, count);

            _layers = new ConcurrentQueue<ILayer>(copy);
        }

        public ILayer this[int index] => _layers.ToArray()[index];

        public void Add(ILayer layer)
        {
            if (layer == null) throw new ArgumentException("Layer cannot be null");
            
            _layers.Enqueue(layer);
            OnLayerAdded(layer);
        }

        public void Move(int index, ILayer layer)
        {
            var copy = _layers.ToArray().ToList();
            copy.Remove(layer);
            if (copy.Count() > index)
            {
                copy.Insert(index, layer);
            }
            else
            {
                copy.Add(layer);
            }
            _layers = new ConcurrentQueue<ILayer>(copy);
            OnLayerMoved(layer);
        }

        public void Insert(int index, ILayer layer)
        {
            var copy = _layers.ToArray().ToList();
            if (copy.Count() > index)
            {
                copy.Insert(index, layer);
            }
            else
            {
                copy.Add(layer);
            }
            _layers = new ConcurrentQueue<ILayer>(copy);
            OnLayerAdded(layer);
        }

        public bool Remove(ILayer layer)
        {
            var copy = _layers.ToArray().ToList();
            var success = copy.Remove(layer);
            _layers = new ConcurrentQueue<ILayer>(copy);

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