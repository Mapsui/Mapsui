using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Fetcher;

namespace Mapsui.Layers;

/// <summary>
/// Represents a collection of map layers with support for grouping, ordering, and change notifications.
/// </summary>
public class LayerCollection : IEnumerable<ILayer>
{
    private ConcurrentQueue<LayerEntry> _entries = new();

    public delegate void LayerCollectionChangedEventHandler(object sender, LayerCollectionChangedEventArgs args);

    /// <summary>
    /// Occurs when the layer collection has changed (layers are added, removed, or moved).
    /// </summary>
    public event LayerCollectionChangedEventHandler? Changed;

    /// <summary>
    /// Gets the number of layers in the collection.
    /// </summary>
    public int Count => _entries.Count;

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator for the layers in the collection.</returns>
    public IEnumerator<ILayer> GetEnumerator()
    {
        var snapshot = _entries.ToArray();
        return snapshot
            .OrderBy(e => e.Group)
            .ThenBy(e => e.Index)
            .Select(e => e.Layer)
            .ToList()
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        var snapshot = _entries.ToArray();
        return snapshot
            .OrderBy(e => e.Group)
            .ThenBy(e => e.Index)
            .Select(e => e.Layer)
            .ToList()
            .GetEnumerator();
    }

    /// <summary>
    /// Retrieves all layers in a specific group.
    /// </summary>
    /// <param name="group">The group identifier (default is 0).</param>
    /// <returns>All the layers in the specified group.</returns>
    public IEnumerable<ILayer> GetLayers(int group = 0)
    {
        var snapshot = _entries.ToArray();
        return snapshot.Where(e => e.Group == group).OrderBy(e => e.Index).Select(e => e.Layer).ToArray();
    }

    /// <summary>
    /// Retrieves all layers from all groups.
    /// </summary>
    /// <returns>All the layers of all groups.</returns>
    public IEnumerable<ILayer> GetLayersOfAllGroups()
    {
        var snapshot = _entries.ToArray();
        return snapshot
            .OrderBy(e => e.Group)
            .ThenBy(e => e.Index)
            .Select(e => e.Layer)
            .ToArray();
    }

    /// <summary>
    /// Clears layers from a specific group.
    /// </summary>
    /// <param name="group">The group identifier (default is 0).</param>
    public void Clear(int group = 0)
    {
        var snapshot = _entries.ToArray();
        var layersToRemove = snapshot.Where(e => e.Group == group).Select(e => e.Layer).ToArray();
        _ = RemoveInternal(layersToRemove);
        OnChanged([], layersToRemove, []);
    }

    /// <summary>
    /// Clears all layers from all groups.
    /// </summary>
    public void ClearAllGroups()
    {
        var snapshot = _entries.ToArray();
        var entries = new ConcurrentQueue<LayerEntry>();

        foreach (var entry in snapshot)
        {
            var layer = entry.Layer;
            if (layer is IAsyncDataFetcher asyncLayer)
            {
                asyncLayer.AbortFetch();
                asyncLayer.ClearCache();
            }
            if (layer is IFetchableSource fetchableSource)
            {
                fetchableSource.ClearCache();
            }
        }
        var layersToRemove = snapshot.Select(e => e.Layer).ToArray();
        _entries = entries;
        OnChanged([], layersToRemove, []);
    }

    /// <summary>
    /// Gets a layer at a specific index in a group.
    /// </summary>
    /// <param name="index">The index of the layer.</param>
    /// <param name="group">The group identifier (default is 0).</param>
    /// <returns>The layer at the specified index and group.</returns>
    public ILayer Get(int index, int group = 0)
    {
        return GetEntriesOfGroup(group)[index].Layer;
    }

    /// <summary>
    /// Adds multiple layers to a specific group.
    /// </summary>
    /// <param name="layers">The layers to add.</param>
    /// <param name="group">The group identifier (default is 0).</param>
    public void Add(IEnumerable<ILayer> layers, int group = 0)
    {
        AddInternal(layers, group);
        OnChanged(layers, [], []);
    }

    /// <summary>
    /// Adds a layer to a specific group.
    /// </summary>
    /// <param name="layer">The layer to add.</param>
    /// <param name="group">The group identifier (default is 0).</param>
    public void Add(ILayer layer, int group = 0)
    {
        AddInternal([layer], group);
        OnChanged([layer], [], []);
    }

    /// <summary>
    /// Adds a layer on top of the collection in a specific group.
    /// </summary>
    /// <param name="layer">The layer to add.</param>
    /// <param name="group">The group identifier (default is 0).</param>
    public void AddOnTop(ILayer layer, int group = 0)
    {
        AddInternal([layer], group);
        OnChanged([layer], [], []);
    }

    /// <summary>
    /// Adds a layer at the bottom of the collection in a specific group.
    /// </summary>
    /// <param name="layer">The layer to add.</param>
    /// <param name="group">The group identifier (default is 0).</param>
    public void AddOnBottom(ILayer layer, int group = 0)
    {
        IEnumerable<ILayer> layers = [layer];
        InsertInternal(0, layers, group);
        OnChanged(layers, [], []);
    }

    /// <summary>
    /// Moves a layer to a specific index in the collection.
    /// </summary>
    /// <param name="index">The target index.</param>
    /// <param name="layer">The layer to move.</param>
    public void Move(int index, ILayer layer)
    {
        MoveInternal(index, layer);
        OnChanged([], [], [layer]);
    }

    /// <summary>
    /// Moves a layer to the bottom of it's current group. This means the other layers will be drawn on top of it.
    /// </summary>
    /// <param name="layer">The layer to move.</param>
    public void MoveToBottom(ILayer layer)
    {
        MoveInternal(0, layer);
        OnChanged([], [], [layer]);
    }

    /// <summary>
    /// Moves a layer to the top of it's current group. This means this layer will be drawn on top of the other layers.
    /// </summary>
    /// <param name="layer">The layer to move.</param>
    public void MoveToTop(ILayer layer)
    {
        var snapshot = _entries.ToArray();
        var group = snapshot.First(e => e.Layer == layer).Group;
        var maxIndex = GetEntriesOfGroup(group).Length - 1;
        MoveInternal(maxIndex, layer);
        OnChanged([], [], [layer]);
    }

    public void MoveDown(ILayer layer)
    {
        var snapshot = _entries.ToArray();
        var entry = snapshot.First(e => e.Layer == layer);
        var index = entry.Index;
        if (index <= 0)
            return;
        MoveInternal(index - 1, layer);
        OnChanged([], [], [layer]);
    }

    public void MoveUp(ILayer layer)
    {
        var snapshot = _entries.ToArray();
        var entry = snapshot.First(e => e.Layer == layer);
        var group = entry.Group;
        var index = entry.Index;
        var maxIndex = GetEntriesOfGroup(group).Length - 1;
        if (index >= maxIndex)
            return;
        MoveInternal(index + 1, layer);
        OnChanged([], [], [layer]);
    }

    /// <summary>
    /// Inserts a layer at a specific index in a group.
    /// </summary>
    /// <param name="index">The target index.</param>
    /// <param name="layer">The layer to insert.</param>
    /// <param name="group">The group identifier (default is 0).</param>
    public void Insert(int index, ILayer layer, int group = 0)
    {
        InsertInternal(index, [layer], group);
        OnChanged([layer], [], []);
    }

    /// <summary>
    /// Inserts multiple layers at a specific index in a group.
    /// </summary>
    /// <param name="index">The target index.</param>
    /// <param name="layers">The layers to insert.</param>
    /// <param name="group">The group identifier (default is 0).</param>
    public void Insert(int index, IEnumerable<ILayer> layers, int group = 0)
    {
        InsertInternal(index, layers, group);
        OnChanged(layers, [], []);
    }

    /// <summary>
    /// Removes multiple layers from the collection.
    /// </summary>
    /// <param name="layers">The layers to remove.</param>
    /// <returns>True if all layers were removed successfully; otherwise, false.</returns>
    public bool Remove(ILayer[] layers)
    {
        var success = RemoveInternal(layers);
        OnChanged([], layers, []);
        return success;
    }

    /// <summary>
    /// Removes a specific layer from the collection.
    /// </summary>
    /// <param name="layer">The layer to remove.</param>
    /// <returns>True if the layer was removed successfully; otherwise, false.</returns>
    public bool Remove(ILayer layer)
    {
        var success = RemoveInternal([layer]);
        OnChanged([], [layer], []);
        return success;
    }

    /// <summary>
    /// Removes layers that match a specific predicate.
    /// </summary>
    /// <param name="predicate">The condition to match for removal.</param>
    /// <returns>True if all matching layers were removed successfully; otherwise, false.</returns>
    public bool Remove(Func<ILayer, bool> predicate)
    {
        var snapshot = _entries.ToArray();
        var layersToRemove = snapshot.Select(e => e.Layer).Where(predicate).ToArray();
        var success = RemoveInternal(layersToRemove);

        OnChanged([], layersToRemove, []);
        return success;
    }

    /// <summary>
    /// Modifies the layer collection by removing specified layers and adding new layers.
    /// </summary>
    /// <param name="layersToRemove">The layers to remove from the collection.</param>
    /// <param name="layersToAdd">The layers to add to the collection.</param>
    public void Modify(IEnumerable<ILayer> layersToRemove, IEnumerable<ILayer> layersToAdd)
    {
        var layersToRemoveSnapshot = layersToRemove.ToArray();
        var layersToAddSnapshot = layersToAdd.ToArray();

        _ = RemoveInternal(layersToRemoveSnapshot);
        AddInternal(layersToAddSnapshot);

        OnChanged(layersToAddSnapshot, layersToRemoveSnapshot, []);
    }

    /// <summary>
    /// Modifies the layer collection by removing layers that match a specified predicate and adding new layers.
    /// </summary>
    /// <param name="removePredicate">The predicate to determine which layers to remove from the collection.</param>
    /// <param name="layersToAdd">The layers to add to the collection.</param>
    public void Modify(Func<ILayer, bool> removePredicate, IEnumerable<ILayer> layersToAdd)
    {
        var snapshot = _entries.ToArray();
        var layersToRemove = snapshot.Select(e => e.Layer).Where(removePredicate).ToArray();
        var layersToAddSnapshot = layersToAdd.ToArray();

        _ = RemoveInternal(layersToRemove);
        AddInternal(layersToAddSnapshot);

        OnChanged(layersToAddSnapshot, layersToRemove, []);
    }

    /// <summary>
    /// Finds layers in the collection by their name.
    /// </summary>
    /// <param name="layerName">The name of the layer to find.</param>
    /// <returns>The layers with the specified name.</returns>
    public IEnumerable<ILayer> FindLayer(string layerName)
    {
        var snapshot = _entries.ToArray();
        return snapshot.Where(e => e.Layer.Name == layerName).Select(e => e.Layer).ToArray();
    }

    private void MoveInternal(int index, ILayer layer)
    {
        var snapshot = _entries.ToArray();
        var entryToMove = snapshot.First(e => e.Layer == layer);
        var group = entryToMove.Group;

        var groupEntries = GetEntriesOfGroup(group);
        if (groupEntries.Length == 0)
            return;

        // Clamp target index to the valid range within the group
        var maxIndex = groupEntries.Length - 1;
        index = Math.Clamp(index, 0, maxIndex);

        entryToMove.Index = index;

        var counter = 0;
        foreach (var entry in GetEntriesOfGroup(group))
        {
            if (entryToMove == entry) // Skip the entry we are moving
                continue;

            if (counter < index)
                entry.Index = counter;
            else
                entry.Index = counter + 1;

            counter++;
        }
    }

    private LayerEntry[] GetEntriesOfGroup(int group)
    {
        var snapshot = _entries.ToArray();
        return snapshot.Where(e => e.Group == group).OrderBy(e => e.Index).ToArray();
    }

    private void InsertInternal(int index, IEnumerable<ILayer> layers, int group)
    {
        if (layers == null || !layers.Any())
            throw new ArgumentException("Layers cannot be null or empty");

        foreach (var layer in layers)
        {
            InsertInternal(layer, index, group);
            index++;
        }
    }

    private void InsertInternal(ILayer layer, int index, int group)
    {
        var groupEntries = GetEntriesOfGroup(group);
        index = Math.Clamp(index, 0, groupEntries.Length);

        var entryToInsert = new LayerEntry(layer, index, group);
        var counter = 0;
        foreach (var entry in groupEntries)
        {
            if (counter < index)
                entry.Index = counter;
            else
                entry.Index = counter + 1;

            counter++;
        }
        _entries.Enqueue(entryToInsert);
    }

    private void AddInternal(IEnumerable<ILayer> layers, int group = 0)
    {
        if (layers == null || !layers.Any())
            throw new ArgumentException("Layers cannot be null or empty");

        foreach (var layer in layers)
        {
            var index = _entries.Count(e => e.Group == group);
            _entries.Enqueue(new LayerEntry(layer, index, group));
        }
    }

    private bool RemoveInternal(ILayer[] layers)
    {
        var workingEntries = _entries.ToList();
        var success = true;

        foreach (var layer in layers)
        {
            var entryToRemove = workingEntries.FirstOrDefault(e => e.Layer == layer);
            if (entryToRemove == null)
            {
                success = false;
                continue;
            }
            if (!workingEntries.Remove(entryToRemove))
                success = false;

            UpdateIndexes(workingEntries, entryToRemove.Group);

            if (layer is IAsyncDataFetcher asyncLayer)
            {
                asyncLayer.AbortFetch();
                asyncLayer.ClearCache();
            }
            if (layer is IFetchableSource fetchableSource)
            {
                fetchableSource.ClearCache();
            }
        }

        _entries = new ConcurrentQueue<LayerEntry>(workingEntries);

        return success;
    }

    private static void UpdateIndexes(List<LayerEntry> entries, int group)
    {
        var counter = 0;
        foreach (var entry in entries.Where(e => e.Group == group).OrderBy(e => e.Index))
        {
            entry.Index = counter;
            counter++;
        }
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
