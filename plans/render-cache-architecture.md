# Render Cache Architecture

## Purpose

Create a clear, measurable, and maintainable cache architecture for the next major Mapsui release. Developers should be able to determine where a resource is loaded, decoded, cached, measured, evicted, and disposed without tracing individual renderers.

This plan follows the short-term font performance fix. It may change public cache APIs and therefore belongs to a major-release workstream.

## Current problems

- `VectorCache` is publicly named as though it caches vector geometry, but it stores arbitrary disposable rendering objects such as `SKFont` and `SKPaint`.
- Unrelated resource types compete for one capacity and eviction policy.
- Per-resource counts, hit rates, misses, and evictions are not readily visible.
- Font resolution is distributed across renderers.
- `FontSourceCache` caches raw bytes, while conversion to native typefaces is inconsistently cached.
- `FontSource.SourceToSourceId` is static and can only grow.
- Static renderer instances complicate mutable resource ownership and concurrency.

## Architectural principles

1. Source caches store source data; renderer caches store decoded native resources.
2. Every expensive resource type has one creation/resolution gateway.
3. Every cache has one explicit owner and a deterministic disposal point.
4. Cache names describe what they contain.
5. Cache keys are immutable value objects.
6. Cache capacity and policy are selected per resource category.
7. Every cache exposes the same basic metrics.
8. Renderers consume resources; they do not own hidden static caches.

## Target resource flow

```text
Font/FontSource
      |
      +--> FontSourceCache --------> raw font bytes
      |                                  |
      +--> TypefaceResolver <------------+
                   |
                   v
             TypefaceCache --------> SKTypeface
                   |
                   v
          SKFont or cached drawable --------> renderer
```

The equivalent source-versus-decoded distinction should also remain visible for images and other externally loaded resources.

## Proposed cache categories

### Source caches

- `ImageSourceCache`: raw bytes obtained from embedded, file, or remote sources.
- `FontSourceCache`: raw font bytes obtained from embedded, file, or remote sources.

These caches participate in fetching and should not contain renderer-specific native objects.

### Renderer resource caches

- `TypefaceCache`: decoded `SKTypeface` resources, keyed by family/source and face attributes.
- `StyleRenderCache`: reusable style-derived resources such as `SKPaint`, sized `SKFont`, and paths where caching is beneficial.
- `DrawableImageCache`: decoded image or picture resources.
- Per-layer drawable caches: feature/layer render artifacts.
- `TileCache`: tile data/resources, with its existing specialist policy.

Do not create a separate cache merely because a type is disposable. Create one when the resource category needs distinct ownership, capacity, eviction, or metrics.

## Implementation plan

### 1. Inventory and measure the existing caches

For every cache, document:

- Key and value types.
- Owner and lifetime.
- Capacity and eviction policy.
- Threading assumptions.
- Disposal behavior.
- Producers and consumers.
- Expected and worst-case cardinality.

Add temporary diagnostics if necessary to measure real applications before selecting defaults.

### 2. Define a common metrics contract

Introduce a small cache diagnostics contract shared by all caches:

- Name/category.
- Current entry count.
- Capacity, or an explicit unbounded marker.
- Hits.
- Misses.
- Additions.
- Evictions/removals.
- Optional estimated byte size when the value can be calculated credibly.

Metrics must be cheap when enabled and avoid retaining cache entries. Decide whether counters are always active or enabled through diagnostics configuration.

### 3. Decide renderer-resource ownership

Write and approve an architecture decision before changing public APIs. Compare at least:

- Explicit Skia resource caches owned by a Skia rendering context associated with each `RenderService`.
- A renderer-independent resource-cache registry owned and disposed by `RenderService`, into which the Skia renderer registers typed caches.
- Renderer-owned caches with an explicit lifecycle added to `IMapRenderer`.

The selected design must:

- Keep SkiaSharp types out of the core Mapsui assembly.
- Dispose native resources deterministically with the map/render service.
- Support multiple maps and concurrent rendering.
- Expose individual cache metrics rather than hiding all resources in one undifferentiated cache.

### 4. Introduce the centralized typeface subsystem

- Add a Skia-specific `TypefaceResolver` and `TypefaceCache`.
- Make it the only path for `SKTypeface.FromFamilyName` and `SKTypeface.FromStream`.
- Key entries by immutable family/source, weight, width, slant, and collection face index.
- Exclude font size from the typeface key.
- Define fallback behavior for missing families, unavailable source bytes, and unsupported glyphs.
- Define whether failures are cached and, if so, for how long.

### 5. Separate style rendering resources

- Identify the parts of `VectorCache` that cache paints, fonts, paths, bitmaps, and other objects.
- Move each entry type to the cache matching its lifetime and eviction needs.
- Avoid caching cheap objects unless measurements justify it.
- Avoid mutable objects shared across concurrent render operations unless access is explicitly controlled.

### 6. Replace `VectorCache` through a compatibility migration

Because `VectorCache` and `RenderService.VectorCache` are public:

1. Add the new APIs and migrate Mapsui internals first.
2. Keep the old API as a forwarding compatibility layer where practical.
3. Mark the old API obsolete with migration guidance.
4. Remove it only at the chosen major-version boundary.

If forwarding cannot preserve correct ownership or semantics, document the break explicitly rather than maintaining a misleading compatibility layer.

### 7. Remove unbounded global font registration

- Replace `FontSource.SourceToSourceId` with source discovery/registration scoped to the map or rendering session.
- Prefer a deterministic source identity derived from the URI/value where possible instead of a global URI-to-GUID registry.
- Ensure releasing a map makes its source bytes and native typefaces collectible/disposable.
- Define explicit behavior for applications that intentionally share resources across maps.

### 8. Remove static mutable renderer resource state

- Review static widget and style renderer registries.
- Separate immutable renderer registration from mutable renderer instances/resources.
- Ensure renderer-local state is either per rendering session or demonstrably thread-safe.

### 9. Expose diagnostics

- Add an API that enumerates cache metrics from a `RenderService`.
- Integrate those metrics into logging and, optionally, the performance widget or a diagnostics sample.
- Make it easy to answer: "Which cache is growing, and why?"

### 10. Enforce and document the architecture

- Document the resource lifecycle and cache-selection rules for contributors.
- Add tests or an architectural check preventing direct typeface construction outside `TypefaceResolver`.
- Add lifecycle, concurrency, capacity, eviction, and metrics tests for each cache.
- Add migration notes for users who accessed `VectorCache` directly.

## Acceptance criteria

- Every expensive rendering resource has a documented owner, key, lifetime, and disposal path.
- Font family and font stream resolution occur only through `TypefaceResolver`.
- Cache statistics can be inspected individually through one consistent diagnostics API.
- No renderer-specific native type is exposed from the core Mapsui assembly.
- Multiple maps do not share mutable renderer resources accidentally.
- Cache capacities and unbounded caches are explicit and documented.
- `FontSource` registration no longer creates unavoidable process-lifetime growth.
- Existing users receive clear migration guidance for changes to `VectorCache`.

## Rollout

1. Complete and release the short-term performance fix.
2. Collect cache metrics from representative samples and applications.
3. Approve the cache ownership architecture decision.
4. Introduce new caches and migrate internal consumers.
5. Add compatibility shims and obsolescence notices.
6. Validate memory, rendering performance, concurrency, and visual output.
7. Remove obsolete APIs at the major release boundary.

## Non-goals

- A single universal cache policy for all resources.
- Precise native-memory measurement where the underlying library cannot provide it reliably.
- Preserving misleading APIs indefinitely when doing so prevents correct ownership or disposal.

