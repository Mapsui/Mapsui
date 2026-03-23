# v6 Post-Branch TODO

Things to do after branching for v6 but before the release. These are deferred because they introduce breaking changes.

## Samples: move label samples from Tests to Labels category

Some label-related samples currently live in the `Tests` category. They should be moved to the `Labels` category. The samples don't need to be realistic or map-based — simple demos are fine.

- Identify label samples in `Samples/Mapsui.Samples.Common/Maps/Tests/` and move them to `Samples/Mapsui.Samples.Common/Maps/Styles/` (or a dedicated `Labels/` subfolder).
- Update the `Category` property on each sample accordingly.
- Verify regression tests still pass after the move (the source generator will pick up the relocated samples automatically).

## FontDataLoaded event — consider replacing with a generation counter

`FontSourceCache.FontDataLoaded` and the subscription in `RenderService` (`FontDataLoaded += (_, _) => VectorCache.Dispose()`) are needed because:

- When a `LabelStyle` references a `FontSource` that hasn't loaded yet, the experimental renderer falls back to the system font and caches that `SKFont` in `VectorCache`, keyed by the style.
- When the real font bytes arrive asynchronously, the cached entry is stale (wrong typeface). Without any invalidation it would stay wrong forever.
- Images don't have this problem: a missing image simply produces no cache entry. When it loads, a correct entry is created fresh. No "wrong entry" to clear.

The event is correct and documented, but a **generation counter** (`FontSourceCache.FontGeneration`, incremented on each load) would be slightly cleaner: store `_lastFontGeneration` on `RenderService` and check before each render instead of subscribing to an event. This removes event subscription concerns and makes the relationship explicit in the render path. Not a breaking change, but worth revisiting when tidying the rendering pipeline.
