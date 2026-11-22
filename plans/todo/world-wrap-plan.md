---
title: World wrap-around plan
status: proposed
date: 2026-11-22
---

# World wrap-around plan

Status: proposed, 2026-11-22. Issue: #518. Target release: Mapsui 6, opt-in. Companion to the [rendering engine plan](rendering-engine-plan.md); independent of it, but stage 4 below lands in the experimental renderer first.

## The model in one paragraph

Wrapping is a property of the map's coordinate system, not of layers. When a map wraps, the world is a strip of width `W` (2πR in Spherical Mercator, 360 in EPSG:4326) that repeats to the left and right. Three things change and nothing else: the **navigator** keeps the viewport centre normalised inside the strip, **fetching** splits a viewport extent that crosses the edge into pieces that lie inside the strip, and the **renderer** draws the map once per world copy that touches the screen, each time with the viewport shifted by a multiple of `W` and the canvas clipped to that strip. Layers, providers, styles and style renderers never see a coordinate outside the world and are not changed.

## Decisions

| Decision | Choice | Why |
|---|---|---|
| Opt-in | `Map.WorldWrap` (type `WorldWrap?`, default `null`). `WorldWrap.ForCrs("EPSG:3857")` and `ForCrs("EPSG:4326")` give the known widths; any width can be given for other CRSs | Off by default, so no existing app changes behaviour. Additive to shared components, which may not break. |
| Where wrapping is handled | `Navigator` (normalisation), `Map.ToFetchInfo` (splitting), the renderer (copies). Not in layers or providers | Keeps every data path unaware of wrap; the surface of change is small and reviewable. |
| Rendering copies | Render the full layer stack once per offset `k` with `viewport with { CenterX = CenterX - k·W }` and a clip to the strip; `k` covers the screen | Reuses all renderers unchanged. Costs up to one extra pass at the edge; measured in stage 4. |
| Hit-testing | `ScreenToWorld`, then normalise X into the strip, then the existing `GetMapInfo` | Features live in the strip; a tap on a copy is the same feature. |
| Animations across the edge | `CenterOn`/`FlyTo` take the shortest way round | Expected behaviour; without it the map spins the long way. |
| Minimum zoom | Allowed to show more than one world; offsets then cover several copies | Simpler than forbidding it; measure the cost. |

## Stages

Each stage is one pull request with the checklist skill. Shared-component changes are additive or opt-in; anything more stops and asks the maintainer.

### Stage 1: `WorldWrap` and viewport helpers

- `WorldWrap` record in `Mapsui`: `Width`, `MinX`; `ForCrs(string)` for EPSG:3857 and EPSG:4326, `null` for others.
- `Map.WorldWrap` property, default `null`.
- Viewport extension helpers: `NormalizeX(x)`, `Normalize(viewport)`, `GetWorldOffsets(viewport)` (the list of `k` whose copy intersects the viewport extent, rotation included).
- Unit tests for both CRSs, including a viewport wider than the world and a rotated viewport.

Exit: tests pass; nothing else changes.

### Stage 2: `Navigator`

- After every viewport change, normalise `CenterX` when the map wraps. The jump by `W` is invisible because rendering is modular.
- Pan limiting: `ViewportLimiterKeepWithinExtent` and `OverridePanBounds` limit Y only when wrapping; X is free.
- `CenterOn`, `NavigateTo`, `FlyTo`: compute the X delta modulo `W` and take the shorter direction.
- `Navigator` needs to know the wrap: pass `Map.WorldWrap` the same way it gets pan and zoom bounds today.
- Tests: normalisation after pan, shortest-way animation across the edge, limiter ignores X.

Exit: tests pass; with `WorldWrap == null` every existing navigator test is unchanged.

### Stage 3: fetching

- `FetchInfo.Extents` (additive): the pieces of the section inside the strip, one piece when nothing crosses the edge. `Extent` keeps returning the first piece so existing layers keep working.
- `Map.ToFetchInfo` fills `Extents` by splitting the normalised section at the strip edge.
- `Layer.FetchAsync` and `TileLayer` iterate `Extents` and merge results. Other layers that read `Extent` fetch the first piece only; list them and convert the ones that ship with Mapsui.
- Tests with a section that crosses the edge: two pieces, both inside the strip; features from both pieces are present.

Exit: a tile layer and a feature layer fetch both sides of the edge; non-wrapping maps unchanged.

### Stage 4: rendering (experimental renderer)

- `MapRenderer.Render`: for each `k` from `GetWorldOffsets`, clip to the strip shifted by `k·W` and render all layers with the shifted viewport. `k = 0` only when not wrapping, so the non-wrapping path is identical to today.
- Partial rendering: a dirty rectangle in world coordinates is replicated per offset before clipping.
- `GetMapInfo`: normalise the world position of the screen point before hit-testing.
- Widgets are drawn once, after the copies, as today.
- Sample "World wrap" (OpenStreetMap, `WorldWrap.ForCrs("EPSG:3857")`), with a `MyLocationLayer` to show an immediate layer across the edge.
- Rendering regression tests: a viewport straddling the antimeridian, zoomed out past one world, and rotated.
- Measure the cost of the extra pass with the performance tooling and record it.

Exit: the sample pans around the world continuously; regression and performance numbers recorded.

### Stage 5: default renderer and documentation

- Apply the same `Render` loop and `GetMapInfo` change to `Mapsui.Rendering.Skia` once stage 4 has settled; the code is the same shape.
- Documentation page on world wrap; upgrade guide entry (opt-in, how to enable, what changes in `Viewport.CenterX`).

Exit: wrap works in both renderers; documented.

## Risks and open questions

- **Features that cross the edge themselves** (a polygon spanning the antimeridian) are a data problem; providers deliver them as they are and they render in whichever copy their coordinates fall. Document; do not solve here.
- **`VisibilityMargin` and `Grow`** near the edge produce extents outside the strip; the fetch split clamps them. Check `Layer.FetchAsync` after stage 3.
- **Callers that store `CenterX`** see it jump by `W` when the centre is normalised. Only when wrapping is on; stated in the upgrade guide.
- **Zoom-to-extent across the edge** (`ZoomToBox` with a box that crosses it): decide in stage 2 whether to support it or document the limitation.
- **Engine plan interaction:** when the rendering engine plan's chunk path exists, chunk columns are normalised modulo `W` and the per-offset render loop still applies unchanged. Nothing in either plan blocks the other.
