---
title: Rendering engine plan: chunked, prepared, immutable
status: proposed
date: 2026-11-22
---

# Rendering engine plan: chunked, prepared, immutable

Status: proposed, 2026-11-22; revised 2026-11-24 (chunk artifact, scheduling, simplification). Scope: the experimental renderer (`Mapsui.Experimental.Rendering.Skia`), which may break at any point. Target release: Mapsui 6.

This plan turns the two-step rendering idea into a conceptually simple engine. Read this file first, then the stage you are working on. Decisions below are made; do not reopen them without a measured reason. Open questions are listed at the end.

## The model in one paragraph

The render thread only replays prepared, immutable work; it never prepares anything. The unit of preparation is a **chunk** (a tile in a grid derived from the map's coordinate system), not a feature. The prepared artifact for a chunk is immutable geometry that is cheap to draw under the current transform (pre-tessellated `SKVertices`, an `SKPicture` or a bitmap, chosen by measurement) plus a list of screen-space symbols that must not scale. Chunks are produced on background threads by a bounded, cancellable work queue that yields while the user interacts, published through a hand-over (see the threading section in [mapsui-architecture-principles.md](../../docs/general/markdown/mapsui-architecture-principles.md)), cached by key and invalidated by dropping the key. Layers whose content changes every frame bypass preparation and draw directly from their applied state. Labels are placed in their own low-rate pass.

## Decisions

| Decision | Choice | Why |
|---|---|---|
| Unit of preparation | Chunk: `(level, column, row)` in a grid over the map CRS | Bounded work, trivial invalidation, LRU caching, overzoom fallback, parallelism. Per-feature caches keyed by feature, style and resolution are what makes two-step rendering complicated today. |
| Grid | Generated from the map CRS extent and a resolution ladder, in map units; chunk edge = 256 screen pixels at each level | Works for any projection, not only Spherical Mercator. Use `Navigator.Resolutions` when tile layers define a ladder, else a power-of-two ladder from the extent. |
| Prepared artifact | One immutable artifact per chunk per layer, plus a screen-space overlay list (icons, label anchors). The artifact type is replaceable behind one interface: `SKVertices` (pre-tessellated triangles), `SKPicture` or a bitmap. `SKVertices` is the leading candidate for fill and line geometry; the default is chosen by the stage 3 measurements | An `SKPicture` stores drawing commands, not results: every replay fills and strokes each path again, so a picture of a heavy chunk costs about as much as drawing its features. Pre-tessellated triangles move tessellation to the background thread once; the draw is a few draw calls. This is how GPU vector map engines (MapLibre, Mapbox GL) work. Same key, same cache for all three, so switching type changes no other code. All three are immutable, so the hand-over pattern applies unchanged. See *Chunk artifact options* below. |
| Background scheduling | Chunk preparation runs on a dedicated, bounded work queue, not the thread pool: at most `cores − 1` workers (fewer on mobile), an interaction gate that pauses or throttles workers while the user pans or zooms, cancellation of chunks that left the viewport, and low-allocation preparation | User input and the draw thread must keep their full speed under any background load; background work waits until interaction stops. .NET thread priority cannot guarantee that: it does not apply to the thread pool, is largely ignored on Linux and macOS, needs platform APIs on Android and iOS, and does not stop blocking garbage collections, which suspend every managed thread including the draw thread. Bounded concurrency, the gate and low allocation do. Workers may additionally run at `BelowNormal` priority where the OS honours it. |
| Simplification | The chunk cutter simplifies geometry to the chunk's resolution (drop vertices closer than about one pixel) before tessellation | At small scales thousands of detailed polygons cover a few hundred pixels; most of their vertices cannot be seen. Reducing them is a bigger win than any drawing technique, and it shrinks every artifact type. |
| Static vs dynamic layers | Decided per layer type through an interface, `IImmediateLayer`; not a mutable property | The renderer builds different pipelines for the two kinds; flipping at runtime would mean tearing caches down. A type-level contract is simpler to reason about and to review. Layers are retained (prepared per chunk) unless they implement `IImmediateLayer`. |
| Labels | Separate placement pass on a background thread at a low rate (about 10 Hz) on a viewport snapshot; the render thread draws a placed-label list | Placement and collision are global and expensive; they do not need 60 Hz. |
| Shared components | Changes to core (`Mapsui`, `Mapsui.Tiling`) are additive or opt-in; a stage that needs a breaking change in shared code stops and asks | The experimental renderer may break; the components it shares with the default renderer may not. |
| Performance | Measured, not assumed. Every stage reports numbers from the tooling in stage 1, on a software and a GPU backend, including frame time while preparation is saturated | See [performance.md](../../docs/general/markdown/performance.md). Perceived lag is often whole-machine overload or GC pauses, not work on the draw thread; only measuring under load shows that. |

## Chunk artifact options

The render thread must draw the chunk artifact fast; everything expensive happens while preparing it.

| Artifact | Draw cost | Strengths | Limits and their fixes |
|---|---|---|---|
| `SKVertices` | Lowest on a GPU backend: triangles only, no path processing. On a software backend Skia still rasterizes the triangles on the CPU, so the gain shrinks | Fills (with holes, even-odd), lines, per-vertex colour, pattern and gradient fills through a paint shader, images and sprites through texture coordinates and an image shader | No anti-aliasing: use MSAA on GPU surfaces or semi-transparent fringe triangles along edges. Joins, caps and dashes are not done by Skia: the tessellator produces them. Overlapping triangles of a semi-transparent stroke double the alpha: tessellate without overlap or draw the stroke opaque into a layer. Indices are 16-bit, so at most 65,535 vertices per `SKVertices`: split heavy chunks into batches. Text is not practical, but text is in the overlay and the label pass anyway |
| `SKPicture` | Roughly the cost of drawing the recorded paths again: tessellation (GPU) or scan conversion (CPU) on every replay | Records the normal style renderers unchanged; anything Skia can draw | Not cheap for heavy chunks |
| Bitmap | A blit, independent of content | Constant cost however heavy the chunk | Blurs under overzoom and when zooming between levels; memory per chunk |

Mapsui ships both backend kinds: software (`SKCanvasView` on Android and MAUI, `SKXamlCanvas` on WinUI, `SKControl` on Windows Forms) and GPU (`SKGLSurfaceView`, `SKGLView`, `SKSwapChainPanel`, `SKGLControl`). The choice of default artifact must hold on both, or be made per backend.

## Stages

Each stage is one pull request, validated with the checklist skill, and ends with numbers from the performance tooling. The stages are ordered; a later stage must not start before the previous one has merged, except where noted.

### Stage 1: performance tooling

Goal: a repeatable frame profile that every later stage reports against.

- Add per-layer timing to the experimental renderer behind `Map.Performance` (time spent per layer per frame; prepare time separately from draw time).
- Define a fixed set of heavy scenarios in `Benchmarks/Mapsui.Rendering.Benchmarks` (large point layer, large line layer, vector tiles, labels, rasters, a mixed map). Reuse the existing headless `RegressionMapControl`.
- Produce one table: scenario x (prepare ms, draw ms, allocations) for both renderers. Make it runnable with one command and document it in `performance.md`.
- Add a load scenario: frame times (median and worst) while background preparation is saturated, plus the count and duration of blocking garbage collections. This is the number the scheduling decision is judged by.
- Record the backend (software or GPU) with every number; run the draw measurements on both kinds.
- Record the baseline numbers in `performance.md`.

Exit: the table runs on a developer machine in minutes and its numbers are in the docs.

### Stage 2: chunk schema and chunk cutter

Goal: any layer can be asked for its features per chunk, in any projection.

- `ChunkSchema`: built from the map CRS extent and the resolution ladder; `GetChunks(extent, resolution)` returns keys; `GetExtent(key)`. Design the key and the cutter so that a column can later be normalised modulo a world width; world wrap-around is a separate plan (see Out of scope) and must not be made impossible here.
- `ChunkCutter`: for a retained layer, `GetFeatures(chunkExtent, resolution)` through the existing `ILayer.GetFeatures`, then simplifies each geometry to the chunk's resolution. Tiled layers map their tiles to chunks directly.
- Unit tests for the schema in Spherical Mercator, EPSG:4326 and a projected national CRS, including overzoom parent lookup; tests that simplification keeps polygons valid (no collapsed rings left in, holes stay inside).

Exit: tests pass; no renderer change yet.

### Stage 3: prepared chunks

Goal: one static vector layer renders through chunks and is measurably faster, with the artifact type chosen by measurement.

- `ChunkDrawable`: immutable; holds the artifact (behind one interface with a `Draw(canvas, transform)` method) and the overlay list.
- `ChunkPreparer`: background work per `(layer, key, style version)`, publishes through a hand-over; LRU cache in `RenderService`; eviction by iteration like today's drawable caches.
- Work queue for `ChunkPreparer`, as in the *Background scheduling* decision: bounded worker count, an interaction gate fed by the manipulation state the map control already tracks, cancellation of queued and running chunks that left the viewport, nearest-to-centre first. Preparation reuses buffers (vertex and index arrays, feature lists) so it allocates little.
- Render thread: for each retained layer, for each visible key: draw the artifact under the viewport transform, or the nearest prepared parent (overzoom) while the chunk is pending; then draw the overlay list in screen space.
- Prototype first on one vector layer (polygons and lines) in the experimental renderer. Implement `SKVertices` and `SKPicture` artifacts (bitmap if both disappoint) and compare them on a software and a GPU backend against the stage 1 numbers, including visual quality of edges and strokes. Pick the default from that, or a default per backend.
- Tessellation for `SKVertices`: fills through a polygon tessellator (holes, even-odd); strokes through an own stroker producing joins, caps and dashes without overlapping triangles; batches split at 65,535 vertices. A third-party tessellator is a new dependency; check its licence and platforms before adding it.

Exit: the prototype layer is faster in the stage 1 table, including the load scenario; the draw thread keeps its frame time while chunks are being prepared during a pan; no regression-test differences beyond tolerance. Then extend to the remaining style renderers in the same stage or a follow-up.

### Stage 4: immediate layers

Goal: layers that change every frame draw directly, with no preparation and no cache.

- `IImmediateLayer` in `Mapsui`: implemented by `MyLocationLayer`, `AnimatedPointLayer`, editing and selection layers.
- The experimental renderer draws immediate layers from their applied state every frame, through the existing style renderers.
- Retained layers no longer need the per-feature drawable caches once stage 3 covers their styles; remove those caches here or in stage 6.

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
- Threading follows the architecture principles: background work publishes immutable artifacts, the render pipeline only swaps and replays. Background work never runs unbounded on the thread pool; it goes through the chunk work queue.

## Out of scope

- **World wrap-around (issue #518).** Only its drawing is a renderer concern; `Navigator` pan limits and centring, fetch extents that cross the world edge, hit-testing and CRS metadata are shared core. It has its own plan, [world-wrap-plan.md](world-wrap-plan.md), built as opt-in (`Map.WorldWrap`, off by default) so existing apps are unaffected. Stage 2 keeps the chunk key wrap-ready.

## Open questions

- Chunk edge of 256 screen pixels is a starting point; stage 3 should try 512 and record both.
- Whether overzoom should also draw children (when zooming out) or only parents; decide in stage 3 by what looks acceptable.
- Whether the label pass should run per chunk first (fast, local collisions) and globally second; decide in stage 5.
- Anti-aliasing for `SKVertices`: MSAA on GPU surfaces, fringe triangles, or both; and what to do on software backends where MSAA is not available. Decide in stage 3 by visual comparison.
- Line width during continuous zoom: with pre-tessellated strokes, width is fixed per chunk level and scales with the transform between levels, as it does with a replayed picture. Whether that looks acceptable, or widths need to change per frame without re-tessellating, is decided in stage 3. Candidate techniques to investigate: storing each stroke vertex with its extrusion direction and applying the width in a vertex shader (Skia's `SkMesh` has one; `SKRuntimeEffect` only shades pixels and cannot move vertices; check whether the SkiaSharp version in use exposes `SkMesh`), or keeping a few tessellations per chunk at different widths. Colour can change per frame without re-tessellating through the paint or a shader.
- Which tessellator: an existing library or an own implementation; decide in stage 3.
- Interaction gate behaviour: pause workers completely, or keep one running; decide in stage 3 from the load scenario.
