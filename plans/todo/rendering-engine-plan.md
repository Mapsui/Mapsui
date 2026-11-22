---
title: Rendering engine plan: chunked, prepared, immutable
status: proposed
date: 2026-11-22
---

# Rendering engine plan: chunked, prepared, immutable

Status: proposed, 2026-11-22. Scope: the experimental renderer (`Mapsui.Experimental.Rendering.Skia`), which may break at any point. Target release: Mapsui 6.

This plan turns the two-step rendering idea into a conceptually simple engine. Read this file first, then the stage you are working on. Decisions below are made; do not reopen them without a measured reason. Open questions are listed at the end.

## The model in one paragraph

The render thread only replays prepared, immutable work; it never prepares anything. The unit of preparation is a **chunk** (a tile in a grid derived from the map's coordinate system), not a feature. The prepared artifact for a chunk is an immutable `SKPicture` (vector, replayed under the current transform) plus a list of screen-space symbols that must not scale. Chunks are produced on background threads, published through a hand-over (see the threading section in [mapsui-architecture-principles.md](../../docs/general/markdown/mapsui-architecture-principles.md)), cached by key and invalidated by dropping the key. Layers whose content changes every frame bypass preparation and draw directly from their applied state. Labels are placed in their own low-rate pass.

## Decisions

| Decision | Choice | Why |
|---|---|---|
| Unit of preparation | Chunk: `(level, column, row)` in a grid over the map CRS | Bounded work, trivial invalidation, LRU caching, overzoom fallback, parallelism. Per-feature caches keyed by feature, style and resolution are what makes two-step rendering complicated today. |
| Grid | Generated from the map CRS extent and a resolution ladder, in map units; chunk edge = 256 screen pixels at each level | Works for any projection, not only Spherical Mercator. Use `Navigator.Resolutions` when tile layers define a ladder, else a power-of-two ladder from the extent. |
| Prepared artifact | `SKPicture` per chunk per layer, plus a screen-space overlay list (icons, labels anchors) | Vector: crisp under zoom and rotation, no raster upscaling. Replay cost is close to a bitmap blit. Immutable, so the hand-over pattern applies unchanged. |
| Raster fallback | Rasterize a chunk to a bitmap only where a measurement shows picture replay is too slow | Same key, same cache, different artifact. Not the default. |
| Static vs dynamic layers | Decided per layer type through an interface, `IImmediateLayer`; not a mutable property | The renderer builds different pipelines for the two kinds; flipping at runtime would mean tearing caches down. A type-level contract is simpler to reason about and to review. Layers are retained (prepared per chunk) unless they implement `IImmediateLayer`. |
| Labels | Separate placement pass on a background thread at a low rate (about 10 Hz) on a viewport snapshot; the render thread draws a placed-label list | Placement and collision are global and expensive; they do not need 60 Hz. |
| Shared components | Changes to core (`Mapsui`, `Mapsui.Tiling`) are additive or opt-in; a stage that needs a breaking change in shared code stops and asks | The experimental renderer may break; the components it shares with the default renderer may not. |
| Performance | Measured, not assumed. Every stage reports numbers from the tooling in stage 1 | See [performance.md](../../docs/general/markdown/performance.md). |

## Stages

Each stage is one pull request, validated with the checklist skill, and ends with numbers from the performance tooling. The stages are ordered; a later stage must not start before the previous one has merged, except where noted.

### Stage 1: performance tooling

Goal: a repeatable frame profile that every later stage reports against.

- Add per-layer timing to the experimental renderer behind `Map.Performance` (time spent per layer per frame; prepare time separately from draw time).
- Define a fixed set of heavy scenarios in `Benchmarks/Mapsui.Rendering.Benchmarks` (large point layer, large line layer, vector tiles, labels, rasters, a mixed map). Reuse the existing headless `RegressionMapControl`.
- Produce one table: scenario x (prepare ms, draw ms, allocations) for both renderers. Make it runnable with one command and document it in `performance.md`.
- Record the baseline numbers in `performance.md`.

Exit: the table runs on a developer machine in minutes and its numbers are in the docs.

### Stage 2: chunk schema and chunk cutter

Goal: any layer can be asked for its features per chunk, in any projection.

- `ChunkSchema`: built from the map CRS extent and the resolution ladder; `GetChunks(extent, resolution)` returns keys; `GetExtent(key)`. Design the key and the cutter so that a column can later be normalised modulo a world width; world wrap-around is a separate plan (see Out of scope) and must not be made impossible here.
- `ChunkCutter`: for a retained layer, `GetFeatures(chunkExtent, resolution)` through the existing `ILayer.GetFeatures`. Tiled layers map their tiles to chunks directly.
- Unit tests for the schema in Spherical Mercator, EPSG:4326 and a projected national CRS, including overzoom parent lookup.

Exit: tests pass; no renderer change yet.

### Stage 3: picture per chunk

Goal: one static vector layer renders through chunks and is measurably faster.

- `ChunkDrawable`: immutable; holds the `SKPicture` and the overlay list.
- `ChunkPreparer`: background work per `(layer, key, style version)`, publishes through a hand-over; LRU cache in `RenderService`; eviction by iteration like today's drawable caches.
- Render thread: for each retained layer, for each visible key: replay the picture under the viewport transform, or the nearest prepared parent (overzoom) while the chunk is pending; then draw the overlay list in screen space.
- Prototype first on one vector layer (polygons and lines) in the experimental renderer; compare with stage 1 numbers before extending to symbols, rasters and vector tiles.

Exit: the prototype layer is faster in the stage 1 table, with no regression-test differences beyond tolerance. Then extend to the remaining style renderers in the same stage or a follow-up.

### Stage 4: immediate layers

Goal: layers that change every frame draw directly, with no preparation and no cache.

- `IImmediateLayer` in `Mapsui`: implemented by `MyLocationLayer`, `AnimatedPointLayer`, editing and selection layers.
- The experimental renderer draws immediate layers from their applied state every frame, through the existing style renderers.
- Retained layers no longer need the per-feature drawable caches once stage 3 covers their styles; remove those caches here or in stage 7.

Exit: `MyLocationLayer` and the animation samples render through the immediate path; stage 1 numbers unchanged or better.

### Stage 5: label pass

Goal: label placement off the render thread.

- A placement job runs on a background thread at a low rate on a viewport snapshot and publishes a placed-label list.
- The render thread draws the list; between placements, labels move with the viewport transform.
- Collision across layers happens in this pass, which also removes today's per-layer collision state.

Exit: label-heavy scenario in the stage 1 table is faster; placement visually stable in the samples.

### Stage 6: retire the per-feature path

Goal: one way to render.

- Remove per-feature drawable caches, `DrawableCacheKey` combinations and `FeatureIdTileCache` from the experimental renderer once every style renderer goes through chunks.
- Update `experimental-renderer.md` and the architecture principles.

Exit: the experimental renderer has two paths only: retained-by-chunk and immediate.

## How to work

- One branch and one pull request per stage; stack on the previous stage only when it has not merged yet.
- Use the checklist skill; rendering stages run the rendering regression tests and report the stage 1 numbers in the PR body.
- Keep the default renderer untouched; the experimental renderer is where breaking changes go. Shared components get additive or opt-in changes only; a stage that needs more stops and asks the maintainer.
- Threading follows the architecture principles: background work publishes immutable artifacts, the render pipeline only swaps and replays.

## Out of scope

- **World wrap-around (issue #518).** Only its drawing is a renderer concern; `Navigator` pan limits and centring, fetch extents that cross the world edge, hit-testing and CRS metadata are shared core. It has its own plan, [world-wrap-plan.md](world-wrap-plan.md), built as opt-in (`Map.WorldWrap`, off by default) so existing apps are unaffected. Stage 2 keeps the chunk key wrap-ready.

## Open questions

- Chunk edge of 256 screen pixels is a starting point; stage 3 should try 512 and record both.
- Whether overzoom should also draw children (when zooming out) or only parents; decide in stage 3 by what looks acceptable.
- Whether the label pass should run per chunk first (fast, local collisions) and globally second; decide in stage 5.
