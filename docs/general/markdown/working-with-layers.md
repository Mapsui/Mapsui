---
title: Working with Layers
description: How the Mapsui layer system works, including LayerCollection, layer types, and how layers are rendered in order.
---

# Working with Layers

A Mapsui map is built up out of layers. The map holds a `LayerCollection` (`map.Layers`) and the renderer draws those layers one by one. Understanding how the layer list works is the key to controlling what your map looks like.

## A minimal map

```csharp
map.Layers.Add(OpenStreetMap.CreateTileLayer());
```

That is enough for a working map.

## Layer types you will commonly use

- **`Layer`** — yes, the name is a bit confusing, but this is the general-purpose layer for showing features (points, lines and polygons) from a data source.
- **`TileLayer`** — shows a tiled raster background such as OpenStreetMap or a WMTS source.
- **`ImageLayer`** — shows a single image, typically retrieved from a WMS.

A custom layer is just a class that implements `ILayer`. Most of the time deriving from `BaseLayer` or using one of the layers above is enough.

## Adding, removing and replacing layers

```csharp
map.Layers.Add(layer);                       // append
map.Layers.Insert(0, layer);                 // insert at a specific index
map.Layers.Remove(layer);                    // remove a single layer
map.Layers.Remove(l => l.Name == "Cities");  // remove by predicate
map.Layers.Clear();                          // remove all layers

map.Layers.Modify(layersToRemove, layersToAdd); // remove + add as one change
```

Each operation raises a single change notification. `Modify` exists so a batch of removes and adds counts as one change instead of many, which avoids unnecessary refreshes — see [Reacting to changes](#reacting-to-changes) below.

## Rendering order

Layers are drawn in the order they appear in the collection: the **first layer is drawn first** and ends up at the **bottom**, the **last layer is drawn last** and ends up at the **top**. A typical map therefore puts the background tile layer first and overlays after it.

To change the order at runtime, use the move methods:

```csharp
map.Layers.MoveToTop(layer);
map.Layers.MoveToBottom(layer);
map.Layers.MoveUp(layer);
map.Layers.MoveDown(layer);
map.Layers.Move(targetIndex, layer);
```

## Layer properties that affect drawing

Every layer exposes a few properties that control whether and how it is drawn:

- **`Enabled`** — set to `false` to hide the layer without removing it.
- **`Opacity`** — value between 0 and 1.
- **`MinVisible` / `MaxVisible`** — resolution range in which the layer is drawn. Useful to only show detail layers when zoomed in.
- **`Style`** — the default style applied to all features in the layer. Set to `null` if you only want per-feature styles.
- **`VisibilityMargin`** — extra margin in pixels added around the viewport extent when querying and fetching features. The default is 64 px. Increase it when your layer's style renders visually larger than the feature's geometry — for example wide strokes, large point symbols, labels, or custom renderers that overflow the geometry bounds. Without a large enough margin, features near the viewport edge can disappear before their rendered symbol leaves the screen.
- **`Name`** — used for lookups (`FindLayer`) and for display in things like a legend.

## Layer groups

Groups let you keep background tiles, vector content and interactive layers (selection, location, drawing) in separate bands, so a newly added layer can't accidentally end up on top of, say, your selection layer.

`LayerCollection` supports this with an integer **group**. Every `Add`, `Insert`, `Get`, `Clear` and `GetLayers` method accepts an optional `group` parameter (default `0`). Groups are drawn in numeric order — lower first, higher last. Within a group, layers follow the insertion/index order described under [Rendering order](#rendering-order).

A common convention is:

| Group | Used for                                                |
|------:|---------------------------------------------------------|
|   -1  | Background (tile layers)                                |
|    0  | Content (your data layers — the default)                |
|    1  | UI / interaction layers (selection, location, drawing)  |

### Example

```csharp
const int background = -1;
const int content = 0;
const int overlay = 1;

map.Layers.Add(OpenStreetMap.CreateTileLayer(), background);
map.Layers.Add(citiesLayer, content);
map.Layers.Add(roadsLayer, content);
map.Layers.Add(selectionLayer, overlay);
```

The order of the `Add` calls above does not matter — each layer ends up in the right band because of its group.

One thing to watch out for once you use multiple groups: `Clear()` only removes the default group. Use `Clear(group)` for a specific group, or `ClearAllGroups()` to remove everything.

## Reacting to changes

`LayerCollection` raises a `Changed` event whenever layers are added, removed or moved. The `MapControl` subscribes to this event to refresh the screen, but you can also subscribe yourself, for example to update a legend:

```csharp
map.Layers.Changed += (s, e) =>
{
    foreach (var added in e.AddedLayers) { /* ... */ }
    foreach (var removed in e.RemovedLayers) { /* ... */ }
    foreach (var moved in e.MovedLayers) { /* ... */ }
};
```

## See also

- [MapInfo](mapinfo.md) — querying what is under a screen position
- [Asynchronous Data Fetching](async-fetching.md) — how layers fetch data on a background thread
- [Custom Layer Renderer](custom-layer-renderer.md) — taking full control over how a layer is drawn
