using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Fetcher;

namespace Mapsui.Layers;

public class LayerCollection : IEnumerable<ILayer>
{
    private ConcurrentQueue<ILayer> _layers = new();

    public delegate void LayerRemovedEventHandler(ILayer layer);
    public delegate void LayerAddedEventHandler(ILayer layer);
    public delegate void LayerMovedEventHandler(ILayer layer);

    public delegate void LayerCollectionChangedEventHandler(object sender, LayerCollectionChangedEventArgs args);

    public event LayerRemovedEventHandler? LayerRemoved;
    public event LayerAddedEventHandler? LayerAdded;
    public event LayerMovedEventHandler? LayerMoved;

    public event LayerCollectionChangedEventHandler? Changed;

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

        var maxCount = Math.Min(array.Length, copy.Count);
        var count = maxCount - arrayIndex;
        copy.CopyTo(0, array, arrayIndex, count);

        _layers = new ConcurrentQueue<ILayer>(copy);
    }

    public ILayer this[int index] => _layers.ToArray()[index];

    public void Add(params ILayer[] layers)
    {
        AddLayers(layers);
        OnChanged(layers, null);
    }

    public void Move(int index, ILayer layer)
    {
        var copy = _layers.ToArray().ToList();
        copy.Remove(layer);

        if (copy.Count > index)
            copy.Insert(index, layer);
        else
            copy.Add(layer);

        _layers = new ConcurrentQueue<ILayer>(copy);
        OnLayerMoved(layer);
        OnChanged(null, null, new[] { layer });
    }

    public void Insert(int index, params ILayer[] layers)
    {
        if (layers == null || !layers.Any())
            throw new ArgumentException("Layers cannot be null or empty");

        var copy = _layers.ToArray().ToList();
        if (copy.Count > index)
            copy.InsertRange(index, layers);
        else
            copy.AddRange(layers);

        _layers = new ConcurrentQueue<ILayer>(copy);
        foreach (var layer in layers)
            OnLayerAdded(layer);

        OnChanged(layers, null);
    }

    public bool Remove(params ILayer[] layers)
    {
        var success = RemoveLayers(layers);
        OnChanged(null, layers);

        return success;
    }

    public bool Remove(Func<ILayer, bool> predicate)
    {
        var copyLayers = _layers.ToArray().Where(predicate).ToArray();
        var success = RemoveLayers(copyLayers);

        OnChanged(null, copyLayers);
        return success;
    }

    public void Modify(IEnumerable<ILayer> layersToRemove, IEnumerable<ILayer> layersToAdd)
    {
        var copyLayersToRemove = layersToRemove.ToArray();
        var copyLayersToAdd = layersToAdd.ToArray();

        RemoveLayers(copyLayersToRemove);
        AddLayers(copyLayersToAdd);

        OnChanged(copyLayersToAdd, copyLayersToRemove);
    }

    public void Modify(Func<ILayer, bool> removePredicate, IEnumerable<ILayer> layersToAdd)
    {
        var copyLayersToRemove = _layers.ToArray().Where(removePredicate).ToArray();
        var copyLayersToAdd = layersToAdd.ToArray();

        RemoveLayers(copyLayersToRemove);
        AddLayers(copyLayersToAdd);

        OnChanged(copyLayersToAdd, copyLayersToRemove);
    }

    private void AddLayers(ILayer[] layers)
    {
        if (layers == null || !layers.Any())
            throw new ArgumentException("Layers cannot be null or empty");

        foreach (var layer in layers)
        {
            _layers.Enqueue(layer);
            OnLayerAdded(layer);
        }
    }

    private bool RemoveLayers(ILayer[] layers)
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
        foreach (var layer in layers)
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

    private void OnChanged(IEnumerable<ILayer>? added, IEnumerable<ILayer>? removed, IEnumerable<ILayer>? moved = null)
    {
        Changed?.Invoke(this, new LayerCollectionChangedEventArgs(added, removed, moved));
    }

    public IEnumerable<ILayer> FindLayer(string layername)
    {
        return _layers.Where(layer => layer.Name.Contains(layername));
    }
}
