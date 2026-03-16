# Custom Font Support — Design Analysis

**Date:** 2026-03-12 (updated 2026-03-16)  
**Status:** Phase 1 complete ✅ — Phase 2 mostly complete ✅ (widgets with no Font property still pending)  
**Related issues:** #3159, #3233, #3280, #3058, #3074

## Decisions Made

- **Option B selected**: Create a separate `FontSource` class (mirrors `Image`), do NOT add `Source` directly to `Font` class.
- **Scope**: `Mapsui.Experimental.Rendering.Skia` only. `Mapsui.Rendering.Skia` (non-experimental, stable renderer) is **out of scope** and will be deleted before v6 release.
- **Reuse `ImageFetcher`**: `FontSourceCache` calls `ImageFetcher.FetchBytesFromImageSourceAsync()` — same URI→bytes logic, no duplication.
- **LabelStyle first**: CalloutStyle and ScaleBarWidget FontSource support also completed in the same pass. Only widgets with no `Font` property at all remain as future work.
- **All four URI schemes supported** (`embedded://`, `file://`, `http://`, `https://`): implemented in `FontSource.ValidateUriScheme()` from the start (reuses `ImageFetcher` which already handles all four).
- **RTK FontMapper completed**: `MapsuiFontMapper` nested class in `SkiaTextLayoutHelper` overrides `FontMapper.TypefaceFromStyle()` to return the typeface already on the `SKFont`, ensuring RTK line-break measurements match the custom typeface metrics.

## Completed Work

- ✅ **RTK line-breaking in experimental renderer** (`chore/remove-richtextkit` branch): `SkiaTextLayoutHelper.SplitLines()` now uses `TextBlock.Layout()` (UAX#14) instead of ASCII-space-only splits. Added `Topten.RichTextKit` dependency back to `Mapsui.Experimental.Rendering.Skia.csproj`.
- ✅ **Regression test**: `CalloutWrapAroundSample` added to Tests category with reference image.
- ✅ **RTK `LineBreaker` confirmed internal**: Uses public `TextBlock` API instead; `FontMapper.TypefaceFromStyle()` is `virtual` and overridden by `MapsuiFontMapper`.
- ✅ **`MapsuiFontMapper`** added to `SkiaTextLayoutHelper`: 3-line nested class overriding `FontMapper` to return the `SKFont`'s existing typeface for RTK line-break measurements — ensures UAX#14 breaks use custom typeface metrics.
- ✅ **`FontSource` class** (`Mapsui/Styles/FontSource.cs`): URI validation, `SourceToSourceId` GUID registry, implicit `string` conversion. Supports `embedded://`, `file://`, `http://`, `https://` (not `svg-content://` or `base64-content://`).
- ✅ **`FontSourceCache`** (`Mapsui/Styles/FontSourceCache.cs`): Implements `IFetchableSource`, keyed by `SourceId` (GUID), reuses `ImageFetcher.FetchBytesFromImageSourceAsync()`.
- ✅ **`Font.FontSource`** property added, copy ctor fixed (Bold/Italic were missing), `Equals`/`GetHashCode` updated.
- ✅ **`RenderService.FontSourceCache`** property added; `Map.GetFetchableSources()` includes it.
- ✅ **`LabelStyleRenderer`** (experimental): `CreateFont()` checks `font.FontSource`, loads `SKTypeface.FromStream()`.
- ✅ **`CalloutStyleRenderer`** (experimental): `CreateSkFont()` checks `font.FontSource` for both `TitleFont` and `SubtitleFont`.
- ✅ **`ScaleBarWidgetRenderer`** (experimental): `CreateTypeface()` helper checks `font.FontSource`.
- ✅ **`CustomFontSample`**: Demonstrates embedded font (`OpenSans-Regular.ttf`) in `LabelStyle`.
- ✅ **`FontSourceCacheTests`** (10 tests) and **`FontTests`** (5 tests).
- ✅ **`fontsource.md`** documentation page added to MkDocs.


## Problem Statement

Mapsui currently resolves fonts exclusively by system font family name (`SKTypeface.FromFamilyName()`). Users cannot use fonts bundled with their application. This affects every platform differently — e.g. Blazor/WASM has only one font available by default, and mobile apps cannot install system fonts. Users need a way to embed font files and reference them in styles.

---

## Current Architecture

### The `Font` class ([Mapsui/Styles/Font.cs](../../../Mapsui/Styles/Font.cs))

Simple data class with four properties:
- `FontFamily` (string?) — resolved as a system font family name
- `Size` (double)
- `Bold` (bool)
- `Italic` (bool)

Implements `Equals`/`GetHashCode` on all four fields (used as VectorCache key).

### Where fonts are used

| Consumer | Font mechanism | Renderer | Notes |
|----------|---------------|----------|-------|
| **LabelStyle** | `Font` property | `LabelStyleRenderer` → `SKTypeface.FromFamilyName()` | Main feature style. Cached via `VectorCache.GetOrCreate(style.Font, CreateFont)` |
| **CalloutStyle** | `TitleFont` + `SubtitleFont` | `CalloutStyleRenderer` → Topten.RichTextKit `Style.FontFamily` | RichTextKit resolves font internally, also by system name. **The experimental renderer (`Mapsui.Experimental.Rendering.Skia`) bypasses RichTextKit entirely and uses `SkiaTextLayoutHelper` instead — see below.** |
| **ScaleBarWidget** | `Font` property | `ScaleBarWidgetRenderer` → `SKTypeface.FromFamilyName()` | Recreated every frame, no VectorCache |
| **TextBoxWidget** | `TextSize` only (no family) | `TextBoxWidgetRenderer` → `new SKFont { Size = ... }` | Uses SkiaSharp default typeface. No font customization at all |
| **LoggingWidget** | Hardcoded size 12 | `LoggingWidgetRenderer` | No font customization |
| **PerformanceWidget** | Custom size | `PerformanceWidgetRenderer` | No font customization |
| **GradientTheme** | Interpolates `Font.Size` between min/max | N/A | Takes `FontFamily` from min style; only size changes |
| **VectorTiles (experimental)** | `VectorStyle.TextFont` (string[]) | `SkiaCanvas` → `SKTypeface.FromStream()` | **Already loads fonts from embedded streams** — useful precedent |

### How `Image.Source` solves the analogous problem for images

The image system follows this pipeline:

1. **Style declaration**: `Image` class has `required string Source` with URI validation (`embedded://`, `file://`, `http://`, etc.)
2. **Static registration**: `Image.SourceToSourceId` (ConcurrentDictionary) maps source URI → GUID
3. **Fetching**: `ImageSourceCache` implements `IFetchableSource`. Its `GetFetchJobs()` checks for un-fetched sources and creates `FetchJob` delegates that call `ImageFetcher.FetchBytesFromImageSourceAsync()`
4. **ImageFetcher**: Static utility that dispatches on URI scheme:
   - `embedded://` → loads from assembly manifest resources
   - `file://` → reads from filesystem
   - `http://` / `https://` → HTTP client
   - `svg-content://` / `base64-content://` → inline data
5. **Integration into Map**: `Map.GetFetchableSources()` includes `ImageSourceCache` alongside layer fetchers. The `DataFetcher` calls `GetFetchJobs()` and executes them on a background thread.
6. **Rendering**: The renderer looks up the cached bytes via `ImageSourceCache.Get(image)`, converts to `IDrawableImage` (SKImage/SKPicture), and draws.

### Key differences between images and fonts

| Aspect | Images | Fonts |
|--------|--------|-------|
| Data volume | One file per image, many different images | Few font files, reused across many labels |
| Async needed? | Yes (HTTP images can be large) | Debatable — embedded fonts are available synchronously, but `file://` and `http://` would need async |
| Rendering artifact | `SKImage` / `SKPicture` (drawable) | `SKTypeface` (then wrapped in `SKFont` per style) |
| Platform specifics | Same across platforms | RichTextKit (Callout) has its own font resolution |
| Multiple consumers | `ImageStyle` only | LabelStyle, CalloutStyle, ScaleBarWidget, TextBoxWidget, etc. |
| Cache invalidation | Rarely needed | Never (fonts don't change at runtime) |

---

## Design Options

### Option A: Add a `Source` property to the `Font` class

Add an optional `string? Source` property to `Font`, following the `Image.Source` URI pattern:

```csharp
// New property on Font
public string? Source { get; init; }  // e.g. "embedded://MyApp.Fonts.CustomFont.ttf"
```

**How it works:**
- When `Source` is null (default), behavior is unchanged — `FontFamily` is resolved as a system font
- When `Source` is set, the rendering pipeline loads an `SKTypeface` from the source URI
- `FontFamily` becomes optional (can be extracted from the typeface metadata or used as a friendly name for caching)

**Pros:**
- No breaking changes — `Source` is optional, all existing code works
- Single unified `Font` class for all consumers
- Familiar pattern (mirrors Image.Source)

**Cons:**
- Adds complexity to a simple data class
- Font loading is renderer-level (Skia-specific), but `Font` lives in the core — there's a layer concern

### Option B: Create a `FontSource` class analogous to `Image`

Create a new `FontSource` class with the same pattern as `Image`:

```csharp
public class FontSource
{
    public required string Source { get; init; }  // "embedded://...", "file://..."
    public string SourceId { get; }               // Auto-generated GUID
    public static ConcurrentDictionary<string, string> SourceToSourceId { get; }
}
```

Then add it as an optional property on `Font`:

```csharp
public FontSource? FontSource { get; set; }
```

**Pros:**
- Clean separation of concerns
- Could add a `FontSourceCache` exactly like `ImageSourceCache`
- Future-proof for font-specific options (e.g. font collection index for TTC files)

**Cons:**
- More types to learn
- Still requires modifying `Font` to add the reference

### Option C: Centralized font registry on `Map` or `RenderService`

Instead of per-style font sources, provide a registration API:

```csharp
map.FontRegistry.Register("my-custom-font", "embedded://MyApp.Fonts.CustomFont.ttf");
// Then in styles:
new Font { FontFamily = "my-custom-font" }
```

**How it works:**
- Users register font names → sources at startup
- The render pipeline checks the registry before falling back to system fonts
- `FontFamily` stays a string, no Font class changes needed

**Pros:**
- Zero changes to existing style classes
- Clean API — register once, use everywhere
- Natural place for font lifecycle management

**Cons:**
- Global state that needs to be accessible from renderers
- Doesn't follow the Image.Source pattern (less consistency)
- Registration step is easy to forget

### Option D: Hybrid (Source on Font + Registry for shared fonts)

Combine A/B with C:

- Add `Font.Source` for per-style custom fonts (one-off usage)
- Add `Map.FontRegistry` for pre-registered shared fonts
- Resolution order: `Font.Source` > FontRegistry > system fonts

---

## Key Technical Challenges

### 1. SKTypeface lifecycle
`SKTypeface.FromStream()` creates a native Skia object. These must be cached and reused — creating one per frame would be a performance disaster. The `VectorCache` already caches `SKFont` objects keyed by `Font`; this just needs to include the source-loaded typeface.

### 2. RichTextKit font resolution (Callout)
RichTextKit resolves fonts internally using `FontFamily` as a string. To use custom fonts in Callouts with the **main renderer**, we would need to either:
- Use RichTextKit's `FontMapper` API (it does have one: `Topten.RichTextKit.FontMapper`) to redirect family name lookups to our loaded typefaces
- Or bypass RichTextKit and render callout text directly with Skia

Note: the **experimental renderer** (`Mapsui.Experimental.Rendering.Skia`) already takes the second path — it uses `SkiaTextLayoutHelper` (raw Skia `SKFont`/`SKPaint`) instead of RichTextKit for callout text. Custom font support for callouts in that renderer only requires that `SkiaTextLayoutHelper` uses `Font.Source` when creating its `SKFont`, without any RichTextKit involvement.

**Important:** when measuring line height in `SkiaTextLayoutHelper` (or any raw-Skia text path), always use `font.Spacing` (= ascent + descent + leading) per line, **not** tight glyph bounds (`rect.Bottom - rect.Top`). Using tight bounds omits the font's built-in leading, causing text blocks to be shorter than expected and making subtitle text appear too close to the title. `font.Spacing` matches RichTextKit's `TextBlock.MeasuredHeight` behavior for a single line.

**This needs further investigation (main renderer only)** — we should check:
- Does `Topten.RichTextKit.FontMapper.Default` allow overriding?
- Can we set a custom `FontMapper` that wraps the default and adds our loaded typefaces?

### 3. Font fetching: sync vs. async
Embedded fonts are available synchronously (assembly resources). File and HTTP fonts would need async loading. Options:
- Follow the `ImageSourceCache` pattern fully (async `IFetchableSource`, integrated with `DataFetcher`)
- Start with embedded-only support (synchronous, no fetcher needed)
- The ImageSourceCache approach has the advantage that labels/callouts would gracefully appear once the font is loaded, rather than blocking

### 4. Font data flow: bytes → SKTypeface
For images: bytes → `SKImage`/`SKPicture` (stored in `DrawableImageCache`).  
For fonts: bytes → `SKTypeface` (needs to be stored somewhere). Options:
- Add a `FontSourceCache` (like `ImageSourceCache`) that stores the raw bytes
- Add a `TypefaceCache` in the renderer layer that converts bytes → `SKTypeface`
- Or combine: store `SKTypeface` directly (since `SKTypeface` is already the minimal useful form)

### 5. Equality/hashing of Font with Source
Currently `Font.Equals` uses `FontFamily + Size + Bold + Italic`. If we add `Source`, it must be included in equality. If `Source` is set, the `FontFamily` string becomes secondary — two fonts with the same `Source` but different `FontFamily` strings should use the same typeface.

### 6. Widgets: missing font customization
Several widgets (TextBoxWidget, LoggingWidget, PerformanceWidget) have either only `TextSize` or hardcoded fonts. These would need `Font` properties added to benefit from custom fonts. This could be done incrementally.

---

## Phased Strategy

### Phase 1: `FontSource` for `LabelStyle` in experimental renderer ✅ COMPLETE

**Goal:** Enable custom fonts for `LabelStyle` in `Mapsui.Experimental.Rendering.Skia` without breaking changes.

**Decision:** Option B — separate `FontSource` class, optional property on `Font`.

All steps complete — see Completed Work above.

**Breaking changes:** None — all additions are optional.

### Phase 2: Extend to Callout and Widgets — partially complete

1. ✅ **`MapsuiFontMapper` for RichTextKit**: Implemented as nested class in `SkiaTextLayoutHelper`. Overrides `TypefaceFromStyle()` to return the typeface already on the `SKFont`. Set on `textBlock.FontMapper` in `SplitByWordUnicode()` when typeface is non-null.
2. ❌ Add `Font` property to `TextBoxWidget`/`BoxWidget` (replacing bare `TextSize`) — still pending
3. ✅ **`ScaleBarWidgetRenderer`** uses `FontSourceCache` via `CreateTypeface()` helper
4. ✅ **`CalloutStyleRenderer`** checks `Font.FontSource` for title and subtitle fonts

### Phase 3: Full implementation (next major version, breaking changes allowed)

1. Potentially make `Font.FontSource` required (or create a new `FontRef` type)
2. Potentially introduce `FontRegistry` on `Map` for shared font management
3. Deprecate system-font-name-only resolution
4. Add `file://` and `http://` scheme support
5. Unify all text rendering to go through the same font resolution pipeline

---

## Open Questions for Further Investigation


1. ~~**RichTextKit FontMapper**~~ ✅ RESOLVED: `MapsuiFontMapper` (nested in `SkiaTextLayoutHelper`) overrides `FontMapper.TypefaceFromStyle()` to return the custom typeface for RTK line-break measurements. The experimental renderer uses this; the main (non-experimental) renderer does not support `FontSource` and is out of scope.

2. **Font collection files (.ttc)**: Should we support TrueType Collection files where one file contains multiple typefaces? `SKTypeface.FromStream()` has an `index` parameter for this.

3. **Font fallback**: When a typeface doesn't contain a glyph (e.g. CJK characters), SkiaSharp falls back to the system font manager. If we load a custom font, do we need to handle fallback explicitly? The VectorTiles code uses `SKFontManager.Default.MatchCharacter()` for this.

4. **Blazor/WASM specifics**: On WASM, `SKTypeface.FromFamilyName()` has very limited support. Does `SKTypeface.FromStream()` work on WASM? If so, custom font support would particularly benefit Blazor users.

5. **Thread safety**: `SKTypeface` objects are thread-safe for reads. But `SKTypeface.FromStream()` consumes the stream. We need to ensure the bytes are cached separately from the typeface if we want to support re-creation.

6. **Font metrics consistency**: When switching from a system font to a custom font (same visual design), label sizes might change. Should we worry about layout stability?

7. **Sample app**: Issue #3159 specifically asks for a sample demonstrating custom font usage. Once the basic mechanism is in place, we should create a sample that demonstrates it end-to-end.
