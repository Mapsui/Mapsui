# Global Experimental Renderer Switching — Design

**Date:** 2026-03-23  
**Status:** Implemented  
**Branch:** `chore/allow-experimental-switching`

## Problem

The experimental renderer (`Mapsui.Experimental.Rendering.Skia.MapRenderer`) could only be activated per-sample by calling `mapControl.SetMapRenderer(...)` at the call site, or per-project by uncommenting a line. There was no single switch that affected both the rendering regression tests and all sample apps at once.

## Goal

One file at the repository root that switches the renderer for:
- All rendering regression tests
- All sample apps (WPF, WinForms, Avalonia, Blazor, WinUI, Uno, Eto, Maui)

Setting `"experimentalRenderer": true` in `config.json` (or a gitignored `config.local.json`) at the repo root is enough to run everything through the experimental renderer.

## Implemented approach

### 1. `DefaultRendererFactory` — `IsConfigured` flag

`DefaultRendererFactory.Create` now has a proper backing field and setter that sets an `IsConfigured` flag when assigned. Both `MapRenderer` static constructors (standard and experimental) check this flag before registering themselves:

```csharp
if (!DefaultRendererFactory.IsConfigured)
    DefaultRendererFactory.Create = () => new MapRenderer();
```

This ensures an explicit `ApplyRendererConfig()` call made before any renderer type is first touched is never overwritten by a static constructor firing later.

### 2. `SampleConfiguration` — shared config reader

`Mapsui.Samples.Common/SampleConfiguration.cs` is the single source of truth for all sample apps. It:
- Walks up from `AppContext.BaseDirectory` to find the repo root (identified by `Mapsui.slnx`).
- Reads `config.local.json` then `config.json` at that root.
- `ApplyRendererConfig()` sets `DefaultRendererFactory.Create` accordingly.
- Uses `JsonSerializerContext` source generation for AOT/trimming compatibility.
- Falls back to the standard renderer silently on devices (Android/iOS) where no root config file is reachable.

### 3. `RenderController` — uses factory when configured

`_mapRenderer` is now initialised as:

```csharp
private IMapRenderer _mapRenderer = DefaultRendererFactory.IsConfigured
    ? DefaultRendererFactory.Create()
    : new MapRenderer();
```

Apps that never call `ApplyRendererConfig()` get `new MapRenderer()` as before. Apps that do call it get the configured renderer on every new `MapControl`.

### 4. Config files

- `config.json` at the repo root — committed, default `experimentalRenderer: false`.
- `config.local.json` at the repo root — gitignored, overrides the committed file for local developer use.

The `.gitignore` pattern `config.local.json` (no leading `/`) already covered all levels.

### 5. `TestConfiguration` — falls back to repo root

`TestConfiguration` still checks the test binary directory first (so in-tree overrides still work), then falls back to the repo-root config using the same `FindRepoRoot()` walk:

```
Priority order:
1. config.local.json  — test binary directory
2. config.json        — test binary directory
3. config.local.json  — repository root
4. config.json        — repository root
```

### 6. Sample app startup wiring

`SampleConfiguration.ApplyRendererConfig()` is called once, before any `MapControl` is created, in a static constructor (or the earliest startup point) of each sample host:

- `Samples/Mapsui.Samples.Wpf/Window1.xaml.cs` — static ctor; removed hardcoded `SetMapRenderer` call.
- `Samples/Mapsui.Samples.WinUI/Mapsui.Samples.WinUI/MainWindow.xaml.cs` — static ctor; removed commented-out `SetMapRenderer`.
- `Samples/Mapsui.Samples.Uno.WinUI/Mapsui.Samples.Uno.WinUI/MainPage.xaml.cs` — static ctor; removed commented-out `SetMapRenderer`.
- `Samples/Mapsui.Samples.WindowsForms/SampleWindow.cs` — new static ctor added.
- `Samples/Mapsui.Samples.Eto/MainForm.cs` — static ctor.
- `Samples/Avalonia/Mapsui.Samples.Avalonia/Views/MainView.axaml.cs` — static ctor.
- `Samples/Mapsui.Samples.Blazor/Program.cs` — before `builder.Build().RunAsync()`.
- `Samples/Mapsui.Samples.Maui/MauiProgram.cs` — top of `CreateMauiApp()`.

## Backward compatibility

Apps using the old `MapControl.SetMapRenderer(new Experimental.Rendering.Skia.MapRenderer())` call are unaffected. `SetMapRenderer` writes directly to the instance's `_mapRenderer` field after construction, completely bypassing `DefaultRendererFactory`. No migration is required.

---

## Promoting the experimental renderer (future cleanup)

When the experimental renderer is ready to replace the standard one, the migration is:

### 1. Copy experimental code over the standard renderer

Move / copy the contents of `Mapsui.Experimental.Rendering.Skia/` into `Mapsui.Rendering.Skia/`, replacing the existing implementations. Then delete the `Mapsui.Experimental.Rendering.Skia/` project entirely and remove it from all solution/filter files.

### 2. Remove `SampleConfiguration` and the root config files

The switching mechanism is no longer needed. Delete:
- `Samples/Mapsui.Samples.Common/SampleConfiguration.cs`
- `config.json` (repo root)
- `config.local.json` (if present locally; it is already gitignored)

### 3. Remove `ApplyRendererConfig()` calls from all sample apps

Remove the `SampleConfiguration.ApplyRendererConfig()` line (and the `using`) from the static constructor / startup of:
- `Samples/Mapsui.Samples.Wpf/Window1.xaml.cs`
- `Samples/Mapsui.Samples.WinUI/Mapsui.Samples.WinUI/MainWindow.xaml.cs`
- `Samples/Mapsui.Samples.Uno.WinUI/Mapsui.Samples.Uno.WinUI/MainPage.xaml.cs`
- `Samples/Mapsui.Samples.Blazor/Program.cs`
- `Samples/Mapsui.Samples.WindowsForms/SampleWindow.cs` (and remove the now-empty static ctor)
- `Samples/Mapsui.Samples.Eto/MainForm.cs`
- `Samples/Avalonia/Mapsui.Samples.Avalonia/Views/MainView.axaml.cs`
- `Samples/Mapsui.Samples.Maui/MauiProgram.cs`

### 4. Simplify `DefaultRendererFactory`

Remove the `IsConfigured` flag and backing-field indirection — they only exist to let `ApplyRendererConfig()` win the race against renderer static constructors. Revert to the original simple shape:

```csharp
public static class DefaultRendererFactory
{
    private static IMapRenderer? _renderer;
    static DefaultRendererFactory() => Create = () => throw new Exception("No renderer registered");
    public static Func<IMapRenderer> Create { get; set; }
    public static IMapRenderer GetRenderer() => _renderer ??= Create();
}
```

### 5. Revert the `MapRenderer` static ctor guard

Remove the `if (!DefaultRendererFactory.IsConfigured)` guard added to `Mapsui.Rendering.Skia/MapRenderer.cs`. After promotion there is only one renderer so the guard is meaningless:

```csharp
static MapRenderer()
{
    InitRenderer();
    DefaultRendererFactory.Create = () => new MapRenderer();
}
```

### 6. Revert `RenderController` initialiser

Revert the field back to the simple form:

```csharp
private IMapRenderer _mapRenderer = new MapRenderer();
```

### 7. Simplify `TestConfiguration`

Remove `FindRepoRoot()` and the repo-root config fallback. The test project's own `config.json` / `config.local.json` is all that is needed, and with only one renderer the `experimentalRenderer` property itself can be removed entirely, taking `TestConfiguration.cs` back to reading nothing (or deleted if `RegressionMapControl.CreateRenderer` no longer needs overriding).

### 8. Simplify `MapRegressionTests`

Remove `ExperimentalOnlySamples` and the conditional exclusion:

```csharp
// Remove this property:
public static ISampleBase[] ExperimentalOnlySamples => [ ... ];

// Simplify ExcludedSamples to only AlwaysExcludedSamples:
public static ISampleBase[] ExcludedSamples => AlwaysExcludedSamples;
```

Also update the reference images: run all regression tests once after promotion and copy the generated images over the originals with `Scripts/CopyGeneratedImagesOverOriginalImages.ps1`.
