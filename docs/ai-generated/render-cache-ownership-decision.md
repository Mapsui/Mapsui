# Render cache ownership decision

Status: proposed for approval as part of implementing `plans/render-cache-architecture.md`.

## Decision

Use a renderer-independent, typed resource registry owned by each `RenderService`.
The service creates each registered cache once, exposes its diagnostics, and disposes
it after dependent style resources and layer drawables. Skia registers a
`TypefaceCache`; core APIs contain no SkiaSharp types. Keep `IMapRenderer`
non-disposable.

| Option | Benefits | Costs |
|---|---|---|
| Skia context attached to RenderService | Groups native resources naturally | Core cannot inspect individual caches without an additional diagnostics abstraction; duplicates context plumbing for other renderers |
| Typed cache registry in RenderService (selected) | Explicit owner, independent category metrics, supports other renderers without native dependencies | Requires a small registration and diagnostics contract |
| Disposable IMapRenderer | Direct renderer ownership | Conflicts with the existing architecture, complicates platform lifecycles and renderer sharing |

## Implementation constraints

- Always-on atomic counters: hits, misses, additions, removals; snapshots include
  category, name, current count, capacity (null means unbounded), and optional bytes.
- Preserve the existing 30,000-entry style LRU default until representative
  application measurements justify a change. Typefaces have their own capacity.
- Mutable style objects are exclusively borrowed with `CacheTracker`; eviction
  during a borrow disposes the resource when returned. Cache keys must include the
  value type to avoid cross-category collisions.
- Typeface keys contain source/family, weight, width, slant, collection face index,
  and source-data identity; font size is excluded. Source bytes remain in the core
  source cache. Missing bytes must not permanently cache a fallback. System family
  fallback uses Skia; RichTextKit continues providing glyph fallback and shaping.
- `VectorCache` becomes an obsolete forwarding adapter to `StyleRenderCache`.
  Retain the existing constructor argument for source compatibility.
- Font source identity is the source URI. Register sources in the owning map's
  `FontSourceCache`, discover built-in style/widget fonts before fetching, and
  register dynamically encountered sources during resolution. Remove automatic
  population of the global registry and document the migration.
- Renderer registration supports factories. Built-in mutable renderer instances
  belong to a renderer instance; native widget resources belong to RenderService.
  Legacy instance registration explicitly shares the supplied custom instance.
- Dispose after rendering/fetching has stopped. Concurrent cache acquisitions
  must not double-create owned resources or return resources disposed by eviction.

## Existing cache inventory

| Cache | Key / value | Owner and lifetime | Capacity / eviction | Threading and disposal | Cardinality / consumers |
|---|---|---|---|---|---|
| VectorCache | arbitrary object / CacheHolder of paint, font, path, bitmap | RenderService / map | 30,000; LRU | locked LRU, exclusive borrowing; currently leaks resources returned after eviction | up to capacity plus concurrent borrows; both Skia renderers |
| DrawableImageCache | source/style string / IDrawableImage | RenderService / map | unbounded; dispose only | concurrent lazy creation; native image disposed at map shutdown | distinct decoded image variants; image renderers |
| ImageSourceCache | source ID / byte[] | RenderService / map | unbounded; explicit clear/unregister | concurrent dictionary; managed bytes | distinct global image registrations; fetching and image decoding |
| FontSourceCache | source ID / byte[] | RenderService / map | unbounded; explicit clear | concurrent dictionary; managed bytes | currently all process font registrations; fetching and font decoding |
| DrawableCache | feature/style generation IDs / IDrawable | RenderService / layer | visible generation set; iteration eviction | background creation, render-thread deferred disposal | visible feature-style pairs, transient overlap; experimental renderer |
| TileDrawableCache | feature/style generation IDs / IDrawable | RenderService / layer | trim to 64 at cleanup | background creation, render-thread deferred disposal | fetched tile pairs between cleanups; experimental renderer |
| TileCache | MRaster identity / ITileCacheEntry | RenderService / map | max(256, 3 x active tiles); iteration eviction | concurrent dictionary, render-bound cleanup | active tiles plus retained history; raster renderer |
| FeatureIdTileCache | feature ID / ITileCacheEntry | caller-owned specialist cache | max(256, 3 x active tiles); iteration eviction | concurrent dictionary; explicit disposal | active tiles plus retained history |
| LruCache | generic key / value | caller-owned utility | explicit capacity; LRU | locked; disposes on replace/evict/clear | at most capacity; style cache and other clients |

Source and decoded-image caches deliberately retain their current map-lifetime,
unbounded policy initially. Diagnostics make growth visible without inventing a
safe eviction policy for resources still referenced by drawables. No exact native
byte-size estimate is claimed. Cross-map native cache sharing is unsupported;
applications may explicitly reuse source values, which each map loads separately.

## Validation

Full repository checklist: lifecycle, concurrent borrowing, capacity, metrics,
font identity/fallback, map isolation, architecture enforcement, all unit tests,
both rendering regression modes, platform builds including Android, and CI format
checks. Record observed sample metrics separately from capacity assumptions.
