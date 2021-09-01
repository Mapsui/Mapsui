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

        public delegate void MultipleLayersRemovedEventHandler(IEnumerable<ILayer> layers);
        public delegate void MultipleLayersAddedEventHandler(IEnumerable<ILayer> layers);
        public delegate void MultipleLayersModifiedEventHandler(IEnumerable<ILayer> layersRemoved, IEnumerable<ILayer> layersAdded);

        public event LayerRemovedEventHandler LayerRemoved;
        public event LayerAddedEventHandler LayerAdded;
        public event LayerMovedEventHandler LayerMoved;

        public event MultipleLayersRemovedEventHandler MultipleLayersRemoved;
        public event MultipleLayersAddedEventHandler MultipleLayersAdded;
        public event MultipleLayersModifiedEventHandler MultipleLayersModified;

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

            _layers = new ConcurrentQueue<ILayer>();

            foreach (var layer in copy)
            {
                if (layer is IAsyncDataFetcher asyncLayer)
                {
                    asyncLayer.AbortFetch();
                    asyncLayer.ClearCache();
                }
                OnLayerRemoved(layer);
            }
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
        
        public void AddMultiple(IEnumerable<ILayer> layers)
        {
            var copy = layers?.ToArray().ToList();

            MassAddLayers(copy);
            OnMultipleLayersAdded(copy);
        }

        public bool RemoveMultiple(IEnumerable<ILayer> layers)
        {
            var inputCopy = layers?.ToArray().ToList();

            var success = inputCopy != null;
            if (inputCopy != null)
                success = MassRemoveLayers(inputCopy);
            
            OnMultipleLayersRemoved(inputCopy);
            return success;
        }

        public bool RemoveMultiple(Func<ILayer, bool> predicate)
        {
            var inputCopy = _layers.ToArray().Where(predicate).ToList();
            var success = MassRemoveLayers(inputCopy);

            OnMultipleLayersRemoved(inputCopy);
            return success;
        }

        public void ModifyMultiple(IEnumerable<ILayer> layersToRemove, IEnumerable<ILayer> layersToAdd)
        {
            var copyLayersToRemove = layersToRemove?.ToArray().ToList();
            var copyLayersToAdd = layersToAdd?.ToArray().ToList();

            MassRemoveLayers(copyLayersToRemove);
            MassAddLayers(copyLayersToAdd);

            OnMultipleLayersModified(copyLayersToRemove, copyLayersToAdd);
        }

        public void ModifyMultiple(Func<ILayer, bool> removePredicate, IEnumerable<ILayer> layersToAdd)
        {
            var copyLayersToRemove = _layers.ToArray().Where(removePredicate).ToList();
            var copyLayersToAdd = layersToAdd?.ToArray().ToList();

            MassRemoveLayers(copyLayersToRemove);
            MassAddLayers(copyLayersToAdd);

            OnMultipleLayersModified(copyLayersToRemove, copyLayersToAdd);
        }

        private void MassAddLayers(IList<ILayer> layers)
        {
            if (layers == null || !layers.Any()) 
                throw new ArgumentException("Layers cannot be null or empty");

            foreach (var layer in layers)
                _layers.Enqueue(layer);
        }

        private bool MassRemoveLayers(IList<ILayer> layers)
        {
            var copy = _layers.ToArray().ToList();
            var success = true;

            foreach (var layer in layers)
            {
                if (!copy.Remove(layer))
                    success = false;

                if (layer is IAsyncDataFetcher asyncLayer)
                {
                    asyncLayer.AbortFetch();
                    asyncLayer.ClearCache();
                }
            }

            _layers = new ConcurrentQueue<ILayer>(copy);
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

        private void OnMultipleLayersRemoved(IEnumerable<ILayer> layers)
        {
            MultipleLayersRemoved?.Invoke(layers);
        }

        private void OnMultipleLayersAdded(IEnumerable<ILayer> layers)
        {
            MultipleLayersAdded?.Invoke(layers);
        }

        private void OnMultipleLayersModified(IEnumerable<ILayer> layersRemoved, IEnumerable<ILayer> layersAdded)
        {
            MultipleLayersModified?.Invoke(layersRemoved, layersAdded);
        }

        public IEnumerable<ILayer> FindLayer(string layername)
        {
            return _layers.Where(layer => layer.Name.Contains(layername));
        }
    }
}