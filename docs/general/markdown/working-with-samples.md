---
title: Working with Samples
description: How the Mapsui sample system works: finding samples, running them locally, and adding new sample classes.
---

# Working with Samples

This page explains how the Mapsui sample system works, both for users who want to find a sample and for contributors who want to add one.

## Finding and running samples

See [Samples](samples.md) for the live Blazor demo, how to run the samples locally, and how to find the class behind a sample.

## How the sample system is structured

### The sample interfaces

All sample classes live in `Samples/Mapsui.Samples.Common/` and implement one of these interfaces:

| Interface | Use when |
|---|---|
| `ISample` | The common case — cross-platform map samples. Implement `Task<Map> CreateMapAsync()`. |
| `IMapControlSample` | You need direct access to the `IMapControl` during setup. Implement `void Setup(IMapControl)`. |
| `IMapViewSample` | MAUI MapView-specific samples (extends `IMapControlSample`). |
| `ISampleTest` | The sample needs extra initialization before a regression test runs. |
| `IPrepareSampleTest` | The sample needs synchronous pre-test preparation. |

Both `ISample` and `IMapControlSample` extend `ISampleBase`, which provides the `Name` and `Category` properties that identify the sample in the UI.

### Automatic registration

Samples **do not need to be manually registered**. The `Mapsui.Sample.SourceGenerator` (in `SourceGenerators/`) scans every class implementing any of the sample interfaces at build time and generates a `Samples.Register()` method for the assembly. Adding a class that implements `ISample` is sufficient — no call to `AllSamples.Register()` is needed.

### Categories and folder structure

The `Category` property on the sample class controls which group it appears in across all sample apps. The folder under `Maps/` is a mirror of the category name for navigation convenience, but the category the app uses comes from the property, not the folder.

Existing categories:

`Basic` · `CustomLayerRenderer` · `CustomPointStyle` · `DataFormats` · `Demo` · `Editing` · `ExperimentalVectorTiles` · `FeatureAnimations` · `Geometries` · `Labels` · `MapBuilders` · `MapInfo` · `MapView` · `Navigation` · `Performance` · `Projection` · `Special` · `Styles` · `Tests` · `ViewportAnimations` · `WFS` · `Widgets` · `WMS` · `WMTS`

## Adding a new sample

### 1. Create the class

Add a `.cs` file in `Samples/Mapsui.Samples.Common/Maps/<CategoryFolder>/`. Name the class `<DescriptiveName>Sample` and the file to match.

Implement `ISample` for the common case:

```csharp
namespace Mapsui.Samples.Common.Maps.Special;

public class MyFeatureSample : ISample
{
    public string Name => "My Feature";
    public string Category => "Special";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        // build the map...
        return Task.FromResult(map);
    }
}
```

The source generator picks it up automatically on the next build.

### Keep each sample file self-contained

A sample file should contain everything needed to understand and use it. If the sample relies on a helper class (a custom provider, a data generator, etc.), put that class in the **same `.cs` file** rather than a separate one. This is an intentional exception to the one-class-per-file convention used elsewhere in the codebase. The goal is that someone reading the sample on the website, or copy-pasting it into their own project, gets all the code they need without having to hunt for additional files.

### 2. Run the rendering regression test

Every sample is automatically picked up by the rendering regression tests in `Tests/Mapsui.Rendering.Skia.Tests`. Run the test for the new sample:

```powershell
dotnet test Tests/Mapsui.Rendering.Skia.Tests --filter "FullyQualifiedName~MyFeatureSample"
```

The first run will be **Inconclusive** because no reference image exists yet — the test generates one in `GeneratedRegression/`. Copy it to `OriginalRegression/` using the helper script:

```powershell
.\Scripts\CopyGeneratedImagesOverOriginalImages.ps1
```

Then revert any files that were not touched by your change to keep binary diffs minimal. Subsequent runs should report **Passed**.
