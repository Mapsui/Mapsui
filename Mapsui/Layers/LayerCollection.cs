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

    public delegate void LayerCollectionChangedEventHandler(object sender, LayerCollectionChangedEventArgs args);

    public event LayerCollectionChangedEventHandler? Changed;

    public int Count => _entries.Count;

    public IEnumerator<ILayer> GetEnumerator()
    {
        return _entries.OrderBy(e => e.Index).OrderBy(e => e.Group).Select(e => e.Layer).ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _entries.OrderBy(e => e.Index).OrderBy(e => e.Group).Select(e => e.Layer).ToList().GetEnumerator();
    }

    public IEnumerable<ILayer> GetLayers(int group = 0)
    {
        return _entries.Where(e => e.Group == group).OrderBy(e => e.Index).Select(e => e.Layer).ToArray();
    }

    public IEnumerable<ILayer> GetLayersOfAllGroups()
    {
        return _entries.OrderBy(e => e.Index).Select(e => e.Layer).ToArray();
    }

    public void Clear(int group = 0)
    {
        var layersToRemove = _entries.Where(e => e.Group != group).Select(e => e.Layer).ToArray();
        RemoveInternal(layersToRemove);
        OnChanged([], layersToRemove, []);
    }

    public void ClearAllGroups()
    {
        var entries = new ConcurrentQueue<LayerEntry>();

        foreach (var entry in _entries.ToArray())
        {
            if (entry is IAsyncDataFetcher asyncLayer)
            {
                asyncLayer.AbortFetch();
                asyncLayer.ClearCache();
            }
        }
        var layersToRemove = _entries.Select(e => e.Layer).ToArray();
        _entries = entries;
        OnChanged([], layersToRemove, []);
    }

    public ILayer Get(int index, int group = 0)
    {
        return GetEntriesOfGroup(group)[index].Layer;
    }
    public void Add(IEnumerable<ILayer> layers, int group = 0)
    {
        AddInternal(layers, group);
        OnChanged(layers, [], []);
    }

    public void Add(ILayer layer, int group = 0)
    {
        AddInternal([layer], group);
        OnChanged([layer], [], []);
    }

    public void AddOnTop(ILayer layer, int group = 0)
    {
        AddInternal([layer], group);
        OnChanged([layer], [], []);
    }

    public void AddOnBottom(ILayer layer, int group = 0)
    {
        IEnumerable<ILayer> layers = [layer];
        InsertInternal(0, layers, group);
        OnChanged(layers, [], []);
    }

    public void Move(int index, ILayer layer)
    {
        MoveInternal(index, layer);
        OnChanged([], [], [layer]);
    }

    public void MoveToBottom(ILayer layer)
    {
        MoveInternal(0, layer);
        OnChanged([], [], [layer]);
    }

    public void MoveToTop(ILayer layer)
    {
        MoveInternal(_entries.Count - 1, layer);
        OnChanged([], [], [layer]);
    }

    public void Insert(int index, ILayer layer, int group = 0)
    {
        IEnumerable<ILayer> layers = [layer];
        InsertInternal(index, layers, group);
        OnChanged(layers, [], []);

    }

    public void Insert(int index, IEnumerable<ILayer> layers, int group = 0)
    {
        InsertInternal(index, layers, group);
        OnChanged(layers, [], []);
    }

    private void MoveInternal(int index, ILayer layer)
    {
        var entryToMove = _entries.First(e => e.Layer == layer);

        entryToMove.Index = index;

        var counter = 0;
        foreach (var entry in GetEntriesOfGroup(entryToMove.Group))
        {
            if (entryToMove == entry) // Skip the entry we are moving
                continue;

            if (counter < index)
                entry.Index = counter;
            else
                entry.Index = counter + 1;

            counter++;
        };
    }

    private LayerEntry[] GetEntriesOfGroup(int group)
    {
        return _entries.Where(e => e.Group == group).OrderBy(e => e.Index).ToArray();
    }

    private void InsertInternal(int index, IEnumerable<ILayer> layers, int group)
    {
        if (layers == null || layers.Count() == 0)
            throw new ArgumentException("Layers cannot be null or empty");

        foreach (var layer in layers)
        {
            InsertInternal(layer, index, group);
            index++;
        }
    }

    private void InsertInternal(ILayer layer, int index, int group)
    {
        var entryToInsert = new LayerEntry(layer, index, group);
        var counter = 0;
        foreach (var entry in GetEntriesOfGroup(entryToInsert.Group))
        {
            if (entry.Group != entryToInsert.Group) // Skip entries in other groups
                continue;

            if (counter < index)
                entry.Index = counter;
            else
                entry.Index = counter + 1;

            counter++;
        };
        _entries.Enqueue(entryToInsert);
    }


    public bool Remove(ILayer[] layers)
    {
        var success = RemoveInternal(layers);
        OnChanged([], layers, []);
        return success;
    }


    public bool Remove(ILayer layer)
    {
        var success = RemoveInternal([layer]);
        OnChanged([], [layer], []);
        return success;
    }

    public bool Remove(Func<ILayer, bool> predicate)
    {
        var copyLayers = _entries.Select(e => e.Layer).ToArray().Where(predicate).ToArray();
        var success = RemoveInternal(copyLayers);

        OnChanged([], copyLayers, []);
        return success;
    }

    public void Modify(IEnumerable<ILayer> layersToRemove, IEnumerable<ILayer> layersToAdd)
    {
        var copyLayersToRemove = layersToRemove.ToArray();
        var copyLayersToAdd = layersToAdd.ToArray();

        RemoveInternal(copyLayersToRemove);
        AddInternal(copyLayersToAdd);

        OnChanged(copyLayersToAdd, copyLayersToRemove, []);
    }

    public void Modify(Func<ILayer, bool> removePredicate, IEnumerable<ILayer> layersToAdd)
    {
        var copyLayersToRemove = _entries.Select(e => e.Layer).ToArray().Where(removePredicate).ToArray();
        var copyLayersToAdd = layersToAdd.ToArray();

        RemoveInternal(copyLayersToRemove);
        AddInternal(copyLayersToAdd);

        OnChanged(copyLayersToAdd, copyLayersToRemove, []);
    }

    public IEnumerable<ILayer> FindLayer(string layerName)
    {
        return _entries.Where(e => e.Layer.Name == layerName).Select(e => e.Layer).ToArray();
    }

    private void AddInternal(IEnumerable<ILayer> layers, int group = 0)
    {
        if (layers == null || layers.Count() == 0)
            throw new ArgumentException("Layers cannot be null or empty");

        foreach (var layer in layers)
        {
            var index = _entries.Count(e => e.Group == group);
            _entries.Enqueue(new LayerEntry(layer, index, group));
        }
    }

    private bool RemoveInternal(ILayer[] layers)
    {
        var copy = _entries.ToList();
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

                entry.Index = counter;
                counter++;
            }

            if (layer is IAsyncDataFetcher asyncLayer)
            {
                asyncLayer.AbortFetch();
                asyncLayer.ClearCache();
            }
        }

        _entries = new ConcurrentQueue<LayerEntry>(copy);

        return success;
    }

    private void OnChanged(IEnumerable<ILayer> added, IEnumerable<ILayer> removed, IEnumerable<ILayer> moved)
    {
        Changed?.Invoke(this, new LayerCollectionChangedEventArgs(added, removed, moved));
    }

    private class LayerEntry(ILayer layer, int index, int group = 0)
    {
        public ILayer Layer { get; set; } = layer;
        public int Group { get; set; } = group;
        public int Index { get; set; } = index;
    }
}
