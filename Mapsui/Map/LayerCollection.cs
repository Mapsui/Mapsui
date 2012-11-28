using System.Linq;
using System.Collections.Generic;
using SharpMap.Layers;
using System.Collections;

namespace SharpMap
{
    public class LayerCollection : IEnumerable<ILayer>
    {
        private readonly IList<ILayer> layers = new List<ILayer>();
        
        public delegate void LayerRemovedEventHandler(ILayer layer);
        public delegate void LayerAddedEventHandler(ILayer layer);
        public delegate void LayerMovedEventHandler(ILayer layer);

        public event LayerRemovedEventHandler LayerRemoved;
        public event LayerAddedEventHandler LayerAdded;
        public event LayerMovedEventHandler LayerMoved;

        public int Count
        {
            get { return layers.Count(); }
        }

        public IEnumerator<ILayer> GetEnumerator()
        {
            return layers.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return layers.GetEnumerator();
        }

        public void Clear()
        {
            foreach (var layer in layers)
            {
                layer.AbortFetch();
                OnLayerRemoved(layer);
            }
            layers.Clear();
        }

        public ILayer this[int index]
        {
            get { return layers[index]; }
        }

        public void Add(ILayer layer)
        {
            layers.Add(layer);
            OnLayerAdded(layer);
        }

        public void Move(int index, ILayer layer)
        {
            layers.Remove(layer);
            layers.Insert(index, layer);
            OnLayerMoved(layer);
        }

        public void Insert(int index, ILayer layer)
        {
            layers.Insert(index, layer);
            OnLayerAdded(layer);
        }

        public void Remove(ILayer layer)
        {
            layers.Remove(layer);
            layer.AbortFetch();
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

        private void OnLayerMoved(ILayer layer)
        {
            if (LayerMoved != null) LayerMoved(layer);
        }

        public IEnumerable<ILayer> FindLayer(string layername)
        {
            return layers.Where(layer => layer.LayerName.Contains(layername));
        }
    }
}
