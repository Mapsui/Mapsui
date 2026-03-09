# Partial Viewport Rendering: Architectural Analysis

## Problem Statement

Today, every change — whether a viewport pan, a single GPS point update, or a property toggle — triggers the exact same rendering path: **full canvas clear + full re-render of all layers, all features, all widgets**. When the viewport is static and only a small fraction of data changes (e.g. a GPS position updating), this wastes significant battery power.

## Current Architecture (How It Works Today)

The rendering pipeline follows a single path regardless of what changed:

> Any Change
>   → Map.RefreshGraphics()
>     → RefreshGraphicsRequest event
>       → RenderController.InvalidateLoopAsync()
>         → InvalidateCanvas()          ← platform full-invalidate
>           → RenderController.Render(canvas)
>             → MapRenderer.Render(canvas, viewport, ALL layers, ALL widgets)
>               → VisibleFeatureIterator iterates EVERY visible feature
>                 → RenderFeature() for each one

Key observations from the code:

1. **No dirty tracking exists** — there is no concept of dirty rectangles or changed regions anywhere in the codebase.

2. **RefreshGraphics() is a boolean signal** — it says "something changed, redraw everything" with zero information about *what* changed. See `Map.cs` `RefreshGraphicsRequest` event and `RenderController.cs` `_needsRefresh` (an `AsyncAutoResetEvent`).

3. **Viewport is a value type** — `Viewport` is a `readonly record struct`. Every change produces a new instance. There's no mutable state to diff against.

4. **Canvas clear is unconditional** — in `MapRenderer.cs` the canvas is cleared with `canvas.Clear(background.ToSkia())` before every render, wiping everything.

5. **Platform `InvalidateCanvas()` is always full-surface** — every platform (WPF, Blazor, Android, Avalonia, etc.) invalidates the entire control.

6. **No persistent `SKSurface`** — the platform's `SKGLView`/`SKCanvasView` provides a fresh `SKCanvas` pointing to the current framebuffer in its `PaintSurface` callback every frame. There is no saved surface anywhere in the codebase.

## Strategy: Persistent Surface + Dirty-Rect Rendering

The approach is to have `RenderController` own a single screen-sized `SKSurface`. All rendering goes to this surface. The platform's `PaintSurface` callback then just blits this surface to the platform canvas with a single `canvas.DrawImage()` call.

When only a small region changes (e.g. a GPS symbol), the render clip is set to the **dirty screen rectangle** instead of the full canvas. Only geometry intersecting that rectangle is drawn; everything outside it is untouched from the previous frame.

> GPS position update
>   → layer.DataChanged (world bbox of old symbol ∪ new symbol)
>     → Map.RefreshRegion(worldRect)
>       → RenderController stores dirty screen rect
>         → RenderController clips persistent surface to dirty rect
>           → canvas.Clear(background) within clip only
>             → MapRenderer renders ALL layers (Skia clips draw calls outside rect for free)
>               → VisibleFeatureIterator skips features whose screen bbox misses dirty rect
>                 → PaintSurface blits persistent surface → platform canvas

Key points:
- **One extra bitmap** at screen resolution (~8 MB at 1080p). Far cheaper than per-layer bitmaps.
- **Works with the existing `MapRenderer`** — no need for the experimental renderer.
- **Skia's clip is free at the GPU level** — draw calls outside the clip rect are discarded without pixel work.
- **`VisibleFeatureIterator` screen-bbox filter** — reduces C# iteration overhead in addition to GPU clipping.
- When the viewport changes (pan, zoom) or a full refresh is triggered, the dirty rect is simply the full canvas and the path degrades to the current behavior.

## Non-Breaking Change Constraint

Changes to non-experimental packages (`Mapsui` core, `Mapsui.Rendering.Skia`, `Mapsui.UI.Shared`, platform UI projects) must be **purely additive** — new methods, new overloads, new optional parameters, new events with null/default semantics. No existing method signature, interface member, or event signature may change. The implementation of each stage is designed around this constraint.

---

## Implementation Stages

---

### Stage 1 — Persistent Surface in the Experimental `MapRenderer` (non-behavioral) ✅

**What:** The experimental `MapRenderer` creates and owns one `SKSurface` matched to the current viewport size. On each `Render()` call it draws to this surface, then blits the surface to the platform-provided `SKCanvas`. `RenderController` and the platform `MapControl` files are **unchanged**.

**Where:** `Mapsui.Experimental.Rendering.Skia/MapRenderer.cs` only.

**New internal flow (experimental renderer only):**

> Experimental MapRenderer.Render(object target, viewport, ...)
>   → EnsurePersistentSurface(ref _persistentSurface, viewport)   ← static; creates/recreates PersistentSurface on size change
>   → RenderTypeSave(_persistentSurface.SKSurface.Canvas, ...)
>   → _persistentSurface.SKSurface.Canvas.Flush()
>   → ((SKCanvas)target).DrawImage(_persistentSurface.SKSurface.Snapshot(), 0, 0)

`PersistentSurface` is a private nested class that owns the `SKSurface` (created in its property initializer) and implements `IDisposable`. `EnsurePersistentSurface` is a `static void` method taking the field by `ref`; it disposes and recreates only when the viewport size changes.

`MapRenderer` implements `IDisposable` to dispose `_persistentSurface` on shutdown.

`RenderController.Render(object canvas)` still calls `_mapRenderer.Render(canvas, ...)` exactly as before. The platform's `PaintSurface` callback is unchanged. The persistent buffer is completely internal to the experimental renderer.

**Why `RenderController` does not need to change:** `IMapRenderer.Render()` already receives the platform canvas as `object target`. The experimental renderer can blit from its internal surface to that canvas without any contract change.

**Viewport size change:** The `Viewport` value is passed into every `Render()` call. If its width/height differs from the current surface, the surface is recreated. No extra lifecycle hooks needed.

**Risk:** Low. Purely internal to the experimental package; no non-experimental code changes.

---

### Stage 2 — `RefreshRegion` API + Dirty Rect Parameter ✅

**What:** Add a way for callers to say "only this world rect changed". The dirty rect is **accumulated in `RenderController`** — the timing/coordination layer — snapshotted atomically just before each paint, and **passed as an explicit parameter to `IMapRenderer.Render()`**. This keeps the renderer a stateless function of its inputs; it receives everything it needs at call time and holds no cross-call mutable state.

**Why accumulation belongs in `RenderController`, not the renderer:**
`RenderController` already owns `AsyncAutoResetEvent _needsRefresh`, which gates whether a render cycle happens at all. It is the natural place to accumulate "what changed since the last paint." Storing the dirty rect on the renderer instead would mean one method call (`SetDirtyRect`) mutates state that a different later call (`Render`) consumes — exactly the coupling between distant code that makes systems hard to reason about.

**Accumulated dirty-rect state machine (in `RenderController`):**
- `RefreshGraphics()` → full refresh; accumulated partial rect is discarded.
- `RefreshRegion(rect)` → if no full refresh is pending, union with accumulated rect.
- `RefreshRegion(rect)` when a full refresh is already pending → ignored (can't downgrade).
- Just before `Render()` is called: snapshot & reset atomically (`TakePendingDirtyRect()`).

**Changes required:**

| File | Change | Breaking? |
|------|--------|-----------|
| `Mapsui/Map.cs` | Add `public void RefreshRegion(MRect? worldRect)` + `event EventHandler<MRect?>? RefreshRegionRequest` | No — new members |
| `Mapsui/Rendering/IMapRenderer.cs` | Add `MRect? dirtyRegion = null` optional parameter to `Render()` | Source-compatible for callers; all in-repo implementors updated |
| `Mapsui.UI.Shared/MapControl.cs` | Subscribe to `Map.RefreshRegionRequest`; forward to `_renderController?.RefreshRegion(worldRect)` | No — new members |
| `Mapsui.UI.Shared/RenderController.cs` | Add `_fullRefreshPending`, `_pendingDirtyRect`, `_dirtyLock`; update `RefreshGraphics()`; add `RefreshRegion(MRect?)`; add `TakePendingDirtyRect()`; pass result to `_mapRenderer.Render()` | No — new method + internal update |
| `Mapsui.Rendering.Skia/MapRenderer.cs` | Accept new `dirtyRegion` parameter; ignore it (always full render) | No — adding optional param |

**Experimental-only changes:**

The experimental `MapRenderer.Render()` accepts `MRect? dirtyRegion = null`. In this stage the parameter is received but not yet used for clipping — that is Stage 3. No cross-call state remains on the renderer.

**What was removed vs the initial Stage 2 sketch:**
- `IPartialRenderingSupport` interface — unnecessary; the dirty rect is now a `Render()` parameter.
- `SetDirtyRect()` — same reason.
- State accumulation on the renderer — moved to `RenderController`.

**`RefreshRequest` record:**
Accumulation logic lives in a single `RefreshRequest` sealed record (in `Mapsui` core):
- `RefreshRequest.Full` — singleton for full-viewport refresh.
- `new RefreshRequest(MRect dirtyRect)` — partial refresh.
- `Accumulate(other)` — returns the union: full + anything = full; two rects = joined rect.

`RenderController` holds one `RefreshRequest? _pendingRefresh` field (null = nothing pending yet). Both `RefreshGraphics()` overloads call `Accumulate`; `TakePendingRefresh()` atomically snapshots and resets it. The result's `DirtyRect` (null for full) is passed directly to `_mapRenderer.Render()`.

**`RefreshGraphicsEventArgs`** now carries a `RefreshRequest Request` property instead of a raw `MRect?`. `Map.RefreshGraphics()` fires with `RefreshRequest.Full`; `Map.RefreshGraphics(MRect)` fires with `new RefreshRequest(rect)`.

**Risk:** Low. `RefreshGraphics()` and the full-render path are completely unchanged for all existing callers. The default `dirtyRegion = null` in `Render()` means every existing call site continues to trigger a full render.

---

### Stage 3 — Clip Rendering to Dirty Rect

**What:** When a dirty rect is set, the experimental renderer clips the persistent canvas to that screen rect, clears only that region, and passes the dirty *world* rect as the `GetFeatures` query extent so that only features intersecting the dirty area are iterated.

**Non-experimental additive changes required:**

| File | Change | Breaking? |
|------|--------|-----------|
| `Mapsui/Rendering/VisibleFeatureIterator.cs` | Add a **new overload** of `IterateLayers` with an extra `MRect? queryExtent` parameter. When non-null it is used as the extent passed to `layer.GetFeatures` instead of the full viewport extent. The existing overload is untouched. | No — new overload |

The existing `IterateLayers(viewport, layers, iteration, callback, customLayerCallback)` overload remains unchanged. All existing callers (the standard `MapRenderer`, tests, user code) continue to call it as before. Only the experimental renderer calls the new overload.

**Experimental-only changes:**

> canvas.Save()
> canvas.ClipRect(dirtyScreenRect)       ← Skia clips all draws outside this rect
> canvas.DrawColor(background)           ← clears only the dirty region (respects clip, unlike canvas.Clear)
> VisibleFeatureIterator.IterateLayers(..., queryExtent: dirtyWorldRect)
> Render(canvas, viewport, widgets, ...)  ← widgets inside dirty rect are redrawn; outside are no-ops (clip discards)
> canvas.Restore()                        ← pixels outside dirty rect from the persistent surface are untouched

**Why this is efficient:** `layer.GetFeatures(dirtyWorldRect, resolution)` queries the spatial index with the tiny dirty bbox — e.g. the union of the old and new GPS symbol envelopes. For the GPS scenario this returns 1–2 features. The tile layer, vector base layer, and all other layers return zero features for that tiny rect. The Skia clip provides a second safety net for any draw calls that might slip through.

**Note on `VisibleFeatureIterator` cascade:** `VisibleFeatureIterator` is a `static` class with one private `IterateLayer` method. Adding a new `IterateLayers` overload is a three-line change: the new overload passes `queryExtent` down to a new `IterateLayer(viewport, layer, iteration, callback, queryExtent)` private overload that substitutes the query extent for `viewport.ToExtent()`. No class copies needed.

**Risk:** Medium — core rendering change in the experimental renderer, but the full-refresh code path in the standard renderer is completely untouched.

---

### Stage 4 — Auto-Propagate Dirty Rect from Layer Data Changes

**What:** When features are added, removed, or replaced in a `WritableLayer`, the layer computes the affected world rect automatically and exposes it on the `DataChanged` event, so the `MapControl` can call `RefreshRegion` without callers having to compute it manually.

**Non-experimental additive changes required:**

| File | Change | Breaking? |
|------|--------|-----------|
| `Mapsui/Fetcher/DataChangedEventArgs.cs` | Add `public MRect? DirtyRegion { get; init; }` property (null = full refresh). No constructor changes — use `init` so existing `new DataChangedEventArgs(...)` callers are unaffected. | No — new nullable property with null default |
| `Mapsui/Layers/WritableLayer.cs` | In `Add`, `TryRemove`, `AddRange`, etc.: compute the bbox of affected features and pass it via `OnDataChanged(new DataChangedEventArgs(Name) { DirtyRegion = rect })`. | No — internal behavior change, public API unchanged |
| `Mapsui.UI.Shared/MapControl.cs` | In the `DataChanged` handler: if `e.DirtyRegion != null`, call `RefreshRegion(e.DirtyRegion)` instead of `RefreshGraphics()`. | No — extending existing event handler logic |

**For the GPS use case:** update the GPS feature's position in the `WritableLayer` → `WritableLayer` records the old extent before the swap and the new extent after → fires `DataChanged` with `DirtyRegion = oldBbox.Join(newBbox)` → `MapControl` calls `RefreshRegion(tinyRect)` → only the tiny dirty region re-renders.

**Layers that don't compute a dirty rect** (e.g. remote tile layers, custom layers) continue to fire `DataChanged` with `DirtyRegion = null`, which maps to `RefreshGraphics()` — full refresh, unchanged behavior.

**Risk:** Low — all changes are additive. Existing `DataChanged` subscribers simply ignore the new `DirtyRegion` property.

## Battery Impact Estimate

For the use case "static map + GPS position updating every second":

| Approach | Per-Frame Work | Relative Cost |
|----------|---------------|--------------|
| **Current** | Clear canvas + render ALL features across all layers | 100% |
| **With dirty-rect rendering (experimental renderer)** | Clear ~50×50 px region + `GetFeatures` returns 1–2 features | ~2–5% |

The tile layer is effectively skipped — `GetFeatures(tinyGpsBbox)` returns nothing for it, so it is never iterated. The Skia clip discards the background clear for everything outside the dirty region.

## Risks and Tradeoffs

| Risk | Mitigation |
|------|-----------|
| Memory: one extra screen-resolution bitmap | ~8 MB at 1080p; negligible on any modern device |
| Rotation: rotated viewport shifts pixel↔world mapping | Convert dirty world rect through the full `WorldToScreen` transform (including rotation) before clipping |
| Thread safety: dirty rect set from data thread, read from render thread | Use `Interlocked` / `volatile` for the accumulated dirty rect; established pattern in the codebase |
| Incorrect dirty rect leads to visual artifacts | Conservative fallback: union rect is always the safe choice; callers can always pass `null` for full refresh |
| Backward compatibility | `RefreshGraphics()` and the standard `MapRenderer` full-render path are completely unchanged; all new behavior is opt-in via the experimental renderer |

## Summary

All meaningful new logic lives in `Mapsui.Experimental.Rendering.Skia`. Non-experimental packages (`Mapsui` core, `Mapsui.UI.Shared`) receive only small **additive** changes:

| Package | Additive changes |
|---------|-----------------|
| `Mapsui` core | `Map.RefreshGraphics(MRect)` overload; `RefreshGraphicsEventArgs` with `RefreshRequest`; `RefreshRequest` record; `IMapRenderer.Render()` `dirtyRegion` optional param; new `VisibleFeatureIterator.IterateLayers` overload with `queryExtent`; `DataChangedEventArgs.DirtyRegion` nullable property |
| `Mapsui.UI.Shared` | `RenderController.RefreshGraphics(MRect?)` overload with `RefreshRequest` accumulation; `MapControl` wiring |
| `Mapsui.Rendering.Skia` | No changes |
| `Mapsui.Experimental.Rendering.Skia` | Persistent `SKSurface`; `dirtyRegion` parameter used for dirty-rect clip + `queryExtent` render path |

No existing method signatures, interface members, or event types change. The standard renderer and all non-experimental UI code behave identically to today.
