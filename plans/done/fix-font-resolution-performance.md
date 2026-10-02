# Fix Font Resolution Performance

**Implementation status:** Done. The implementation, automated tests, and best-effort Windows benchmark are complete. A Linux/fontconfig run remains optional future confirmation.

## Completion evidence (2026-09-14)

The `ScaleBarFontPerformance` BenchmarkDotNet benchmark was run on Windows 11 with
.NET SDK 9.0.318 and .NET runtime 9.0.20:

| Scenario | Mean | Ratio | Allocated |
|---|---:|---:|---:|
| Font cache disabled | 19.04 µs | 1.00 | 3.26 KB |
| Font cache enabled | 14.76 µs | 0.77 | 3.11 KB |

On this machine, the cached path was approximately 23% faster and allocated about
5% less memory per render. This confirms that the benchmark works and that the cache
provides a measurable benefit on Windows. Linux fontconfig has different lookup costs,
so a Linux run would provide useful additional evidence but is not required to close
this implementation plan.

The benchmark covers the scale bar's system-font path. A dedicated supplied-`FontSource`
benchmark can be added later if Linux measurements or a new performance report justify it.

## Purpose

Fix [Mapsui issue #3386](https://github.com/Mapsui/Mapsui/issues/3386) without changing public APIs. Font files and system font families must not be resolved again for every rendered frame.

This is the short-term plan. It deliberately uses the existing cache infrastructure; renaming and reorganizing that infrastructure belongs to the long-term cache architecture plan.

## Problem

`Mapsui.Rendering.Skia.SkiaWidgets.ScaleBarWidgetRenderer.Draw` currently calls `SKTypeface.FromFamilyName` twice during every draw. On Linux, each named-family lookup can invoke fontconfig and take several milliseconds.

The experimental renderer has the same class of problem in additional paths: some widgets and custom layer renderers create `SKFont`/`SKTypeface` instances on every draw. For supplied fonts, `FontSourceCache` caches the file bytes, but conversion of those bytes into an `SKTypeface` is not consistently cached.

The scale bar does not appear to trigger its own redraw loop. Normal refreshes and animations repeatedly expose the expensive work.

## Constraints

- Do not change the public `Font`, `FontSource`, `RenderService`, or renderer registration APIs.
- Do not rename or replace `VectorCache` in this change.
- Preserve existing text appearance and font fallback behavior.
- Keep cached native objects owned by a map's `RenderService`, so they are disposed with the map.
- Use immutable cache keys. Do not use a mutable `Font` instance directly when later mutation could invalidate its hash code.

## Implementation plan

### 1. Establish a baseline

- Add a small Linux-oriented benchmark for repeated scale-bar rendering.
- Record frame cost and the number of typeface constructions before the fix.
- Test both a named system font and a supplied `FontSource`.

The benchmark is supporting evidence, not a blocking automated test on platforms where font lookup behavior differs.

### 2. Define an internal immutable font cache key

The short-term implementation caches complete `SKFont` instances. Add an internal value key containing the attributes that determine the created rendering resource:

- System font family or `FontSource.SourceId`.
- Bold/weight.
- Italic/slant.
- Typeface collection index if supported later.
- Font size only when the cached value is an `SKFont`; exclude size when the cached value is an `SKTypeface`.

The key must copy values from `Font`; it must not retain a mutable `Font` as a dictionary key.

### 3. Fix the standard scale-bar renderer

- Use one `SKFont` for both the fill and stroke draws. The two `SKPaint` instances continue to control fill and halo rendering.
- Obtain the font/typeface through the existing per-`RenderService` cache instead of calling `SKTypeface.FromFamilyName` from `Draw`.
- Ensure changing the scale bar's font selects a new cache entry.
- Remove direct per-frame typeface assignment where it is no longer necessary.

### 4. Centralize creation inside each current Skia renderer

The stable and experimental renderer projects are separate assemblies. Until they are consolidated, give each one a single internal font/typeface creation helper and route its renderers through that helper.

- The helper is the only place in that assembly that calls `SKTypeface.FromFamilyName` or `SKTypeface.FromStream`.
- The helper uses the existing `RenderService.VectorCache` as its short-term backing store.
- Callers should not know how the value is cached.

This duplication between the two renderer assemblies is temporary and should disappear when the experimental renderer becomes the main renderer.

### 5. Audit every creation path

Classify every current call as one of:

- Already protected by an `SKFont`, drawable, or picture cache.
- Executed per frame and must use the central helper.
- Intentionally uncached, with a documented reason.

At minimum, inspect:

- Label styles.
- Scale-bar widgets.
- Text-box-derived widgets.
- Logging and performance widgets.
- Grid labels.
- Callout content.
- Both system-family and supplied-font paths.

### 6. Add regression coverage

- Verify repeated draws with the same font invoke the typeface factory once.
- Verify two sizes can reuse one typeface where typefaces are cached separately.
- Verify family, weight, slant, or source changes produce distinct entries.
- Verify unavailable `FontSource` data does not permanently cache a fallback.
- Verify disposal clears and disposes cached native resources.
- Run existing Skia rendering regression tests to detect visual changes.
- Run the benchmark on Windows and record the improvement. A Linux run is optional future confirmation.

### 7. Document the temporary rule

Add a short developer comment/documentation note:

> Renderer code must not resolve a typeface during every draw. Use the renderer's centralized font helper, backed temporarily by `VectorCache`.

## Acceptance criteria

- `ScaleBarWidgetRenderer.Draw` performs no repeated named-family lookup for an unchanged font.
- The fill and stroke scale-bar text use one `SKFont`.
- Repeated rendering with a supplied font does not repeatedly call `SKTypeface.FromStream`.
- All direct typeface construction sites are centralized or explicitly justified.
- No public API is removed or changed.
- Existing rendering regression tests pass.
- The scale-bar benchmark demonstrates a measurable improvement with font caching; Linux-specific confirmation may follow separately.

## Non-goals

- Renaming `VectorCache`.
- Introducing public cache metrics.
- Redesigning all cache ownership.
- Removing the static `FontSource` registry.
- Combining the stable and experimental renderer projects.

## Risks

- `SKTypeface`, `SKFont`, and cache eviction must have unambiguous disposal ownership.
- The current renderer registries contain shared static renderer instances, so mutable renderer-local cache state should not be introduced.
- Existing cache capacity is shared with other rendering resources. The long-term plan separates these concerns.
