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
        return _entries.OrderBy(e => e.Order).OrderBy(e => e.Group).Select(e => e.Layer).ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _entries.OrderBy(e => e.Order).OrderBy(e => e.Group).Select(e => e.Layer).ToList().GetEnumerator();
    }

    public Span<ILayer> GetLayersOfGroup(int group)
    {
        return _entries.Where(e => e.Group == group).OrderBy(e => e.Order).Select(e => e.Layer).ToArray();
    }

    private Span<LayerEntry> GetEntriesOfGroup(int group)
    {
        return _entries.Where(e => e.Group == group).OrderBy(e => e.Order).ToArray();
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

    public void Move(int order, ILayer layer)
    {
        MoveLayer(order, layer);
    }

    public void MoveToBottom(ILayer layer)
    {
        MoveLayer(0, layer);
    }

    public void MoveToTop(ILayer layer)
    {
        MoveLayer(_entries.Count - 1, layer);
    }

    private void MoveLayer(int order, ILayer layer)
    {
        var entryToMove = _entries.First(e => e.Layer == layer);

        entryToMove.Order = order;

        var counter = 0;
        foreach (var entry in GetEntriesOfGroup(entryToMove.Group))
        {
            if (entryToMove == entry) // Skip the entry we are moving
                continue;

            if (counter < order)
                entry.Order = counter;
            else
                entry.Order = counter + 1;

            counter++;
        };

        OnLayerMoved(layer);
        OnChanged(null, null, [layer]);
    }

    public void Insert(int order, params ILayer[] layers)
    {
        InsertLayers(order, layers, 0);
    }

    public void Insert(int order, ILayer[] layers, int group)
    {
        InsertLayers(order, layers, group);
    }

    public void Insert(int order, ILayer layer, int group)
    {
        InsertLayers(order, [layer], group);
    }

    private void InsertLayers(int order, ILayer[] layers, int group)
    {
        if (layers == null || layers.Length == 0)
            throw new ArgumentException("Layers cannot be null or empty");

        foreach (var layer in layers)
        {
            InsertLayer(layer, order, group);
            order++;
        }

        foreach (var layer in layers)
            OnLayerAdded(layer);

        OnChanged(layers, null);
    }

    private void InsertLayer(ILayer layer, int order, int group)
    {
        var entryToInsert = new LayerEntry(layer, order, group);
        var counter = 0;
        foreach (var entry in GetEntriesOfGroup(entryToInsert.Group))
        {
            if (entry.Group != entryToInsert.Group) // Skip entries in other groups
                continue;

            if (counter < order)
                entry.Order = counter;
            else
                entry.Order = counter + 1;

            counter++;
        };
        _entries.Enqueue(entryToInsert);
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
            var order = _entries.Count(e => e.Group == group);
            _entries.Enqueue(new LayerEntry(layer, order, group));
            OnLayerAdded(layer);
        }
    }

    private bool RemoveLayers(ILayer[] layers)
    {
        var copy = _entries.ToArray().ToList();
        var success = true;

        foreach (var layer in layers)
        {
            var entryToRemove = copy.First(e => e.Layer == layer);
            if (!copy.Remove(entryToRemove))
                success = false;

            var counter = 0;
            foreach (var entry in copy)
            {
                if (entry.Group != entryToRemove.Group) // Skip entries in other groups
                    continue;

                entry.Order = counter;
                counter++;
            }

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

    private class LayerEntry(ILayer layer, int order, int group = 0)
    {
        public ILayer Layer { get; set; } = layer;
        public int Group { get; set; } = group;
        public int Order { get; set; } = order;
    }
}
