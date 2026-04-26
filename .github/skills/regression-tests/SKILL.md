---
name: regression-tests
description: >
  Authoritative guide for working with Mapsui's rendering regression tests — running, interpreting,
  updating reference images, diagnosing failures, and adding new samples that require the
  experimental renderer. Load this skill whenever a task involves regression tests or rendering changes.
---

# Rendering Regression Tests

Tests live in `Tests/Mapsui.Rendering.Skia.Tests`. They render every sample to a PNG and compare it pixel-by-pixel against a stored reference image.

---

## Running

```powershell
# All regression tests
dotnet test Tests/Mapsui.Rendering.Skia.Tests --filter "TestSampleAsync"

# Single sample (fastest for targeted changes)
dotnet test Tests/Mapsui.Rendering.Skia.Tests --filter "FullyQualifiedName~CalloutSample"
```

---

## Results

| Result | Meaning |
|---|---|
| **Passed** | Generated image matches the reference. |
| **Inconclusive** | No reference image exists yet — the test generated one. **Visually inspect it** before promoting it to a reference. |
| **Failed** | Pixel difference exceeded the threshold. Compare generated vs. reference (paths printed in the failure message). |

Image paths:
- Generated: `Tests/Mapsui.Rendering.Skia.Tests/bin/Debug/net9.0/Resources/Images/GeneratedRegression/`
- Reference:  `Tests/Mapsui.Rendering.Skia.Tests/Resources/Images/OriginalRegression/`

---

## Updating reference images after intentional changes

```powershell
.\Scripts\CopyGeneratedImagesOverOriginalImages.ps1
```

Then run `git diff --name-only` and **revert every image that was not affected by your change** — avoid committing unrelated binary diffs.

Alternatively, copy a single image manually:
```powershell
Copy-Item "Tests\Mapsui.Rendering.Skia.Tests\bin\Debug\net9.0\Resources\Images\GeneratedRegression\MySample.Regression.png" `
          "Tests\Mapsui.Rendering.Skia.Tests\Resources\Images\OriginalRegression\MySample.Regression.png" -Force
```

---

## Standard vs. experimental renderer

The test suite can run with **either** renderer. The active renderer is determined by config files, searched in priority order:

1. `config.local.json` in the test binary output dir (created by copying the project-level file)
2. `config.json` in the test binary output dir (committed — default `experimentalRenderer: false`)
3. `config.local.json` at the repository root (git-ignored, per-machine)
4. `config.json` at the repository root

**CI always runs with the standard renderer** (`experimentalRenderer: false`). Do not assume CI uses the experimental renderer.

To run locally with the experimental renderer, create `Tests/Mapsui.Rendering.Skia.Tests/config.local.json`:
```json
{ "experimentalRenderer": true }
```
This file is git-ignored and automatically copied to the binary output dir at build time.

---

## Samples that require the experimental renderer

The standard renderer does **not** support `Font.FontSource`, RichTextKit BiDi, or emoji rendering. Samples that require any of these must be added to `ExperimentalOnlySamples` in `MapRegressionTests.cs`:

```csharp
public static ISampleBase[] ExperimentalOnlySamples =>
[
    new CalloutWrapAroundSample(),   // FontSource + Chinese text
    new CustomFontWidgetSample(),    // FontSource for Arabic and Chinese widgets
    new RightToLeftSample(),         // FontSource for Arabic
    new EmojiSample(),               // RTK emoji/font-fallback
];
```

When `IsExperimentalRenderer = false` (CI), these samples are excluded from the test run. When `IsExperimentalRenderer = true`, they are included and their reference images must have been generated with the experimental renderer.

**If you add a sample that uses `FontSource`, RTK, or emoji — always add it to `ExperimentalOnlySamples`.**

---

## FontSource — common pitfalls

`FontSource` allows embedding custom fonts. Several things can silently fail:

### TTF vs OTF
`SKTypeface.FromStream` only works with **TTF** (`0x00 0x01 0x00 0x00` magic bytes). OTF/CFF files (magic `OTTO` = `0x4F 0x54 0x54 0x4F`) return `null` silently. Variable fonts in TTF format work fine.

Always verify font format before embedding:
```powershell
$bytes = [System.IO.File]::ReadAllBytes("MyFont.ttf")
"Magic: 0x{0:X2} 0x{1:X2} 0x{2:X2} 0x{3:X2}" -f $bytes[0], $bytes[1], $bytes[2], $bytes[3]
# Must be: 0x00 0x01 0x00 0x00 (TTF) or 0x74 0x72 0x75 0x65 (truetype)
# NOT:     0x4F 0x54 0x54 0x4F (OTF/CFF — won't work with SKTypeface.FromStream)
```

### FetchAllFontDataAsync in tests
Regression tests call `await map.RenderService.FontSourceCache.FetchAllFontDataAsync()` **before** rendering (see `MapRegressionTests.cs`). This populates the font cache synchronously. In production the cache is populated by `DataFetcher` on viewport change — there is no need to call it manually outside tests.

### Renderer must support FontSource
Only `Mapsui.Experimental.Rendering.Skia` honours `Font.FontSource`. The standard renderer (`Mapsui.Rendering.Skia`) ignores it and falls back to `FontFamily` / system font. If glyphs render as boxes with the standard renderer, that is expected — add the sample to `ExperimentalOnlySamples`.

**Why Arabic may appear correct while Chinese shows boxes (standard renderer):** Windows ships with system Arabic fonts (e.g. Segoe UI, Arial Unicode MS) so the standard renderer's `FontFamily` fallback accidentally finds a matching glyph. It does *not* ship with a CJK font by default, so Chinese characters render as boxes. This asymmetry can mask the fact that `FontSource` is being ignored — the Arabic "works" for the wrong reason. If a sample uses `FontSource` for any script, add it to `ExperimentalOnlySamples` regardless of whether individual scripts appear to render correctly on the standard renderer.

### Embedded resource path
The URI must exactly match the fully-qualified assembly resource name:
```
embedded://Mapsui.Samples.Common.Resources.Fonts.NotoSansArabic-Regular.ttf
```
The project `.csproj` must have a matching `<EmbeddedResource>` entry. Mismatch produces null bytes with no error.

---

## Diagnosing a broken rendering test

1. **Check which renderer is active**: read `Tests/Mapsui.Rendering.Skia.Tests/bin/Debug/net9.0/config.local.json` and `config.json`.
2. **Visually compare** the generated image against the reference image — the failure message prints both paths.
3. **Font issues**: if text is boxes, the typeface is null. Check: (a) TTF magic bytes, (b) `FetchAllFontDataAsync` was awaited, (c) `Font.FontSource` URI matches the embedded resource name, (d) renderer is experimental.
4. **Pre-existing failures**: when running with the experimental renderer, some non-related samples may fail because their reference images were generated with the standard renderer. Only fix failures in samples you changed.

---

## Adding a new sample with a regression test

1. Implement the sample (see copilot-instructions for auto-registration via source generator).
2. Run `dotnet test Tests/Mapsui.Rendering.Skia.Tests --filter "FullyQualifiedName~MySample"`.
3. Result should be **Inconclusive** (no reference yet).
4. **Visually inspect** the generated image in `GeneratedRegression/`.
5. Copy it to `OriginalRegression/` (manually or via `CopyGeneratedImagesOverOriginalImages.ps1`).
6. Re-run — should now be **Passed**.
7. If the sample uses `FontSource` / RTK / emoji, add it to `ExperimentalOnlySamples` in `MapRegressionTests.cs` and generate the reference with the experimental renderer.
