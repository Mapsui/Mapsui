using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Fetcher;

namespace Mapsui.Layers;

public class LayerCollection : IEnumerable<ILayer>
{
    private ConcurrentQueue<LayerEntry> _entries = new();

    public delegate void LayerRemovedEventHandler(ILayer layer);
    public delegate void LayerAddedEventHandler(ILayer layer);
    public delegate void LayerMovedEventHandler(ILayer layer);

    public delegate void LayerCollectionChangedEventHandler(object sender, LayerCollectionChangedEventArgs args);

    public event LayerRemovedEventHandler? LayerRemoved;
    public event LayerAddedEventHandler? LayerAdded;
    public event LayerMovedEventHandler? LayerMoved;

    public event LayerCollectionChangedEventHandler? Changed;

    public int Count => _entries.Count;

    public IEnumerator<ILayer> GetEnumerator()
    {
        return (IEnumerator<ILayer>)_entries.OrderBy(l => l.Group).Select(l => l.Layer).ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _entries.OrderBy(l => l.Group).Select(l => l.Layer).ToList().GetEnumerator();
    }

    public void ClearGroup(int group)
    {
        var copy = _entries.ToArray().ToList();

        var entries = copy.Where(e => e.Group != group).Select(e => e.Layer).ToArray();

        RemoveLayers(entries);
    }

    public void Clear()
    {
        var copy = _entries.ToArray().ToList();

        var entries = new ConcurrentQueue<LayerEntry>();

        foreach (var entry in copy)
        {
            if (entry is IAsyncDataFetcher asyncLayer)
            {
                asyncLayer.AbortFetch();
                asyncLayer.ClearCache();
            }
            OnLayerRemoved(entry.Layer);
        }

        _entries = entries;
    }

    public bool Contains(ILayer item)
    {
        return _entries.Any(l => l.Layer == item);
    }

    public void CopyTo(ILayer[] array, int arrayIndex)
    {
        var copy = _entries.ToArray().ToList();

        var maxCount = Math.Min(array.Length, copy.Count);
        var count = maxCount - arrayIndex;
        var entries = new ConcurrentQueue<LayerEntry>(array.Select(l => new LayerEntry(l))).ToArray();
        copy.CopyTo(0, entries, arrayIndex, count);

        _entries = new ConcurrentQueue<LayerEntry>(copy);
    }

    public ILayer this[int index] => _entries.ToArray()[index].Layer;

    public void Add(params ILayer[] layers)
    {
        AddLayers(layers);
        OnChanged(layers, null);
    }

    public void Add(ILayer[] layers, int group = 0)
    {
        AddLayers(layers, group);
        OnChanged(layers, null);
    }

    public void Add(ILayer layer, int group = 0)
    {
        AddLayers([layer], group);
        OnChanged([layer], null);
    }

    public void AddOnTop(ILayer layer, int group = 0)
    {
        Add(layer, group);
    }

    public void AddOnBottom(ILayer layer, int group = 0)
    {
        Insert(0, layer, group);
    }

    public void Move(int index, ILayer layer)
    {
        MoveLayer(index, layer);
    }

    public void MoveToBottom(int index, ILayer layer)
    {
        MoveLayer(0, layer);
    }

    public void MoveToTop(int index, ILayer layer)
    {
        MoveLayer(_entries.Count - 1, layer);
    }

    private void MoveLayer(int index, ILayer layer)
    {
        var copy = _entries.ToArray().ToList();
        copy.Remove(copy.First(e => e.Layer == layer));

        if (copy.Count > index)
            copy.Insert(index, new LayerEntry(layer));
        else
            copy.Add(new LayerEntry(layer));

        _entries = new ConcurrentQueue<LayerEntry>(copy);
        OnLayerMoved(layer);
        OnChanged(null, null, [layer]);
    }

    public void Insert(int index, params ILayer[] layers)
    {
        InsertLayers(index, layers, 0);
    }
    public void Insert(int index, ILayer[] layers, int group)
    {
        InsertLayers(index, layers, group);
    }

    public void Insert(int index, ILayer layer, int group)
    {
        InsertLayers(index, [layer], group);
    }

    private void InsertLayers(int index, ILayer[] layers, int group)
    {
        if (layers == null || layers.Length == 0)
            throw new ArgumentException("Layers cannot be null or empty");

        var entries = layers.Select(l => new LayerEntry(l, group)).ToArray();
        var copy = _entries.ToArray().ToList();
        if (copy.Count > index)
            copy.InsertRange(index, entries);
        else
            copy.AddRange(entries);

        _entries = new ConcurrentQueue<LayerEntry>(copy);
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
        var copyLayers = _entries.Select(e => e.Layer).ToArray().Where(predicate).ToArray();
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
        var copyLayersToRemove = _entries.Select(e => e.Layer).ToArray().Where(removePredicate).ToArray();
        var copyLayersToAdd = layersToAdd.ToArray();

        RemoveLayers(copyLayersToRemove);
        AddLayers(copyLayersToAdd);

        OnChanged(copyLayersToAdd, copyLayersToRemove);
    }

    private void AddLayers(ILayer[] layers, int group = 0)
    {
        if (layers == null || layers.Length == 0)
            throw new ArgumentException("Layers cannot be null or empty");

        foreach (var layer in layers)
        {
            _entries.Enqueue(new LayerEntry(layer, group));
            OnLayerAdded(layer);
        }
    }

    private bool RemoveLayers(ILayer[] layers)
    {
        var copy = _entries.ToArray().ToList();
        var success = true;

        foreach (var layer in layers)
        {
            if (!copy.Remove(new LayerEntry(layer)))
                success = false;

            if (layer is IAsyncDataFetcher asyncLayer)
            {
                asyncLayer.AbortFetch();
                asyncLayer.ClearCache();
            }
        }

        _entries = new ConcurrentQueue<LayerEntry>(copy);
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

    public IEnumerable<ILayer> FindLayer(string layerName)
    {
        return _entries.Where(e => e.Layer.Name == layerName).Select(e => e.Layer).ToArray();
    }

    private class LayerEntry(ILayer layer, int group = 0)
    {
        public ILayer Layer { get; set; } = layer;
        public int Group { get; set; } = group;
    }
}
