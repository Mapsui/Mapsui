---
title: Mapsui Architecture Principles
description: Mapsui-specific architectural decisions: lon/lat ordering, separating rendering from drawing, coordinate system flexibility, and the RenderService pattern for disposable Skia resources.
---

# Mapsui Architecture Principles

## Ordering of lon/lat

In Mapsui code, always use **lon, lat** order, consistent with the x, y order of most cartographic projections.

The official geographic notation is lat, lon — but in map projections, lat corresponds to y and lon to x. To stay consistent with x, y ordering, Mapsui uses lon, lat throughout. Where possible, avoid ordering ambiguity entirely by using named properties (`Longitude`, `Latitude`) or encoding the order in the method name (e.g. `SphericalMercator.FromLonLat(lon, lat)`).

## No rendering in the draw/paint loop

Separate *rendering* (creating platform-specific resources) from *drawing* (using them on the canvas). Resources should be prepared before the paint loop, not inside it.

```csharp
// Rendering — create a platform-specific resource
SKPath path = ToSKPath(feature, style);

// Drawing — use the resource on the canvas
canvas.DrawPath(path, paint);
```

Mapsui strives for optimal performance: in the rendering loop, objects should be ready to paint directly to the canvas without any preparation step.

## Mapsui should not be limited to a single coordinate system

The `Map` can operate in any coordinate system. If no coordinate system is specified on `Map` or `Layers`, they are assumed to share the same system and only a world-to-screen transform is applied. A full coordinate transformation pipeline can be configured via `Map.CRS`, `DataSource.CRS`, and `Map.Transformation`.

## Disposable resources belong in RenderService, not in the renderer

Skia objects such as `SKSurface`, `SKPaint`, and `SKPath` are `IDisposable`. Renderers that create these objects must not become disposable themselves — that would propagate `IDisposable` up through `IMapRenderer` and into `MapControl`. Instead, renderer-owned resources with a lifetime tied to the map should be stored in `RenderService`, which is already disposable and is owned by `Map`. Use `RenderService.GetPersistentRenderSurface(...)` as the pattern; `RenderService.Dispose()` handles cleanup.
