using System;
using System.Linq;
using System.Collections.Generic;
using Mapsui.Layers;
using System.Collections;

namespace Mapsui
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

        public int Count
        {
            get { return _layers.Count(); }
        }

        public bool IsReadOnly { get { return _layers.IsReadOnly;  } }

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
                layer.AbortFetch();
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
            layer.AbortFetch();
            OnLayerRemoved(layer);
            return success;
        }

        private void OnLayerRemoved(ILayer layer)
        {
            if (LayerRemoved != null) LayerRemoved(layer);
        }

        private void OnLayerAdded(ILayer layer)
        {
            if (LayerAdded != null) LayerAdded(layer);
        }

        private void OnLayerMoved(ILayer layer)
        {
            if (LayerMoved != null) LayerMoved(layer);
        }

        public IEnumerable<ILayer> FindLayer(string layername)
        {
            return _layers.Where(layer => layer.Name.Contains(layername));
        }
    }
}
