---
title: Mapsui Architecture Principles
description: Mapsui-specific architectural decisions: lon/lat ordering, separating rendering from drawing, coordinate system flexibility, the RenderService pattern for disposable Skia resources, and the threading rule for layer state.
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

The `Map` can operate in any coordinate system. If no coordinate system is specified on `Map` or `Layers`, they are assumed to share the same system and only a world-to-screen transform is applied. A full coordinate transformation pipeline can be configured via `Map.CRS`, `DataSource.CRS`, and `ProjectionDefaults.Projection` (see [Projections](projections.md)).

## Disposable resources belong in RenderService, not in the renderer

Skia objects such as `SKSurface`, `SKPaint`, and `SKPath` are `IDisposable`. Renderers that create these objects must not become disposable themselves — that would propagate `IDisposable` up through `IMapRenderer` and into `MapControl`. Instead, renderer-owned resources with a lifetime tied to the map should be stored in `RenderService`, which is already disposable and is owned by `Map`. Use `RenderService.GetPersistentRenderSurface(...)` as the pattern; `RenderService.Dispose()` handles cleanup.

## Threading: the render pipeline owns mutable layer state

Mapsui apps call into layers from many threads: a GPS callback, a timer, a background task. At the same time the render pipeline reads the layer's styles and features on every frame. Locks are not the answer: they make the renderer wait, and the more of them there are, the harder it is to see what can block what.

The rule is **one owner, messages in**:

- The **render pipeline** is the only code that touches a layer's mutable render state: styles, features, animations. The pipeline is `RenderController.Render`: it applies pending layer state and advances animations (`Map.UpdateAnimations`, `Navigator.UpdateAnimations`) and then draws, all on the draw thread. That is the UI thread on every platform except Android's GL surface, where it is the GL thread. The invalidate loop only decides when a frame happens; it touches no map state.
- **Public members** of a layer that change what is shown never touch that state directly. They describe the requested state in an **immutable record** and publish it with `StateHandover<T>.Update`, then ask for a redraw with `Map.RefreshGraphics` (which is thread-safe). Writes collapse: when several updates arrive between two frames, the pipeline sees only the last one.
- The pipeline applies the latest record at the start of `UpdateAnimations()`, which `Map` calls on every layer once per frame, through `StateHandover<T>.TryTake`. It diffs the applied and the latest record and changes only what differs. A redraw requested from inside `UpdateAnimations` is drawn in the same frame, because `Render` takes the pending refresh after updating; so the pipeline is the place to request the exact area, and a request from the caller's thread only needs to wake the pipeline. Animation ticks run in `UpdateAnimations` too and request their own areas.
- Keep running animations in an `AnimationList<T>`. `Animation.UpdateAnimations` never removes a finished entry, and a finished entry left in a plain list runs its final step on every frame; `AnimationList<T>.Update` removes finished entries itself.
- **Getters return the latest requested state**, not the applied one, so a caller that sets a value and reads it back sees its own value immediately, on any thread. What the screen shows catches up on the next frame.

`Mapsui.Layers.MyLocationLayer` is the worked example. Its `State` record holds everything a caller can ask for; `Publish` is the only writer; `Apply` is the only code that touches `_locStyle`, `_dirStyle`, `_coStyle` and the feature. When writing or reviewing a layer that is updated from outside the render pipeline, follow the same shape: a record, a `StateHandover<T>`, `Publish` in the public members, `Apply` in `UpdateAnimations`.

What this rule does not cover yet: the fetch pipeline (`DataChanged`, feature caches), `Map.Layers` changes and widgets. They have their own, older synchronisation. `Mapsui.UI.MapView` is legacy and is deliberately left out. On Android's GL surface, touch handling is on the UI thread while the pipeline is on the GL thread, so a tap can hit-test a feature while the pipeline moves it; that is a one-tap inaccuracy, not a crash.
