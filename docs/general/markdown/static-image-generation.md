# Generating Static Images

A common use case for Mapsui is generating static map images programmatically — for example, to embed a map in a PDF report, attach a map thumbnail to an email, or produce a batch of map exports without running a UI application.

## The challenge: async data fetching

Unlike a live map control that continually refreshes as data arrives, a static image requires all layer data to be **fully loaded before rendering**. Mapsui fetches tile data, WMS responses, and other remote sources asynchronously. If you call the renderer immediately after creating the map, most layers will be empty — the rendering completes, but the image is blank.

The solution is to await `Map.RefreshDataAsync()` before rendering. This method drives the full fetch cycle for the current viewport and only returns once every pending fetch job has completed. It handles both the new `DataFetcher` path and any legacy `IAsyncDataFetcher` layers.

## Basic pattern

```csharp
using Mapsui;
using Mapsui.Rendering.Skia;
using Mapsui.Tiling;

// 1. Create the map and add layers
var map = new Map();
map.Layers.Add(OpenStreetMap.CreateTileLayer());

// 2. Set the viewport size and position
map.Navigator.SetSize(800, 600);
map.Navigator.ZoomToBox(map.Extent);

// 3. Await all data fetches for the current viewport
await map.RefreshDataAsync();

// 4. Render to a stream
var renderer = new MapRenderer();
using var stream = renderer.RenderToBitmapStream(map);

// 5. Save or use the stream
await File.WriteAllBytesAsync("map.png", stream.ToArray());
```

## Output formats

`RenderToBitmapStream` supports four formats via the `RenderFormat` enum:

| Format | Notes |
|--------|-------|
| `RenderFormat.Png` | Default. Lossless, best for general use. |
| `RenderFormat.Jpeg` | Lossy. Smaller files; a white background is applied automatically. |
| `RenderFormat.WebP` | Lossy or lossless depending on the `quality` parameter (`100` = lossless). |
| `RenderFormat.Skp` | Skia picture format. Vector; useful for further processing. |

```csharp
// JPEG at 85% quality
using var stream = renderer.RenderToBitmapStream(map, renderFormat: RenderFormat.Jpeg, quality: 85);
```

## Pixel density (HiDPI)

Pass a `pixelDensity` greater than `1` to produce a higher-resolution image while keeping the logical viewport size the same. A value of `2` doubles the pixel dimensions, matching a typical HiDPI display:

```csharp
// 800×600 logical pixels → 1600×1200 physical pixels
using var stream = renderer.RenderToBitmapStream(map, pixelDensity: 2);
```

## Setting the viewport

Before calling `RefreshDataAsync`, you need to configure both the **size** and the **position** of the viewport. Two common approaches:

**Zoom to the extent of a layer:**

```csharp
map.Navigator.SetSize(800, 600);
map.Navigator.ZoomToBox(myLayer.Extent);
await map.RefreshDataAsync();
```

**Set a specific center and resolution:**

```csharp
map.Navigator.SetSize(800, 600);
var center = SphericalMercator.FromLonLat(lon: 4.9, lat: 52.4); // Amsterdam
map.Navigator.CenterOnAndZoomTo(center, resolution: 20);
await map.RefreshDataAsync();
```

## Complete example

The following example creates a PNG thumbnail of an OpenStreetMap tile layer centred on a specific location:

```csharp
using Mapsui;
using Mapsui.Rendering.Skia;
using Mapsui.Tiling;
using Mapsui.Projections;

public static async Task<byte[]> RenderMapImageAsync(double lon, double lat, double resolution, int width, int height)
{
    var map = new Map();
    map.Layers.Add(OpenStreetMap.CreateTileLayer());

    map.Navigator.SetSize(width, height);
    var center = SphericalMercator.FromLonLat(lon, lat);
    map.Navigator.CenterOnAndZoomTo(center, resolution);

    // Wait for all layer data to load at the current viewport
    await map.RefreshDataAsync();

    var renderer = new MapRenderer();
    using var stream = renderer.RenderToBitmapStream(map, pixelDensity: 2);
    return stream.ToArray();
}
```

## How `RefreshDataAsync` works

Internally, `RefreshDataAsync` calls `DataFetcher.ViewportChangedAsync()`, which:

1. Determines which fetch jobs each `IFetchableSource` layer needs for the current viewport.
2. Runs all fetch jobs concurrently (respecting a concurrency limit).
3. Loops until no active or pending fetch jobs remain.
4. Returns — at which point every layer's features and tiles are fully loaded.

For layers that implement the older `IAsyncDataFetcher` interface, their fetch tasks are collected and awaited alongside the `DataFetcher` work. You do not need to interact with `DataFetcher` directly.

!!! note

    `RefreshDataAsync` only fetches data for the **current viewport**. If you change the viewport after calling it, call it again before rendering.
