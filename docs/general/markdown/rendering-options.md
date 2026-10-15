---
title: Rendering Options
description: Choose a drawing backend, compare Android GPU host views in MAUI, and understand how these choices differ from the map renderer.
---

# Rendering options

Mapsui has three distinct rendering choices:

| Choice | What it controls | Example |
|---|---|---|
| Renderer | How map content is drawn through `IMapRenderer` | Standard or experimental Skia renderer |
| Backend | CPU rasterization or GPU drawing | Software or GPU |
| Host view | The native view hosting GPU drawing | TextureView or SurfaceView on Android |

## Backend: software or GPU

Most applications should start with their framework's default. GPU drawing can help some workloads, but it is not always faster. Compare the same map and interactions on the devices your application supports.

| UI framework | Default and available selection |
|---|---|
| MAUI | `MapControl.Backend = RenderBackend.Gpu`; `Software` is available on every platform. Android also offers `AndroidSurfaceView`. |
| Native Android | Instance `RenderMode = SkiaRenderMode.Hardware`, using `SKGLSurfaceView`; choose `SkiaRenderMode.Software` for CPU drawing. |
| Native iOS | `MapControl.UseGPU = true`, using Metal; `false` uses a canvas view. |
| Blazor | `MapControl.UseGPU = true`; `false` selects software drawing. |
| Windows Forms | `MapControl.UseGPU = false`; `true` selects its GPU control. |
| WPF | Software drawing through `SKElement`; no Mapsui backend selector. |
| WinUI on Windows | Software drawing through `SKXamlCanvas`; no Mapsui backend selector. |
| Avalonia | Uses Avalonia's Skia drawing context, including its GPU context when available. Backend configuration belongs to Avalonia. |
| Eto | Uses Eto's `SkiaDrawable` host; no Mapsui backend selector. |
| Uno | Uses `SKCanvasElement` where supported; otherwise selects a GPU host on Android/browser and a software host elsewhere. No Mapsui backend selector. |

### MAUI configuration

Configure the default in shared startup code before constructing controls, including controls created from XAML:

```csharp
using Mapsui.UI.Maui;

MapControl.Backend = RenderBackend.Gpu; // Initial default
```

Or select a backend for one control:

```csharp
var mapControl = new MapControl(RenderBackend.Software);
var activeBackend = mapControl.ActiveBackend;
```

Each control keeps the backend selected at construction. Changing `MapControl.Backend` affects future parameterless controls only. Recreate a control to change its backend; dispose the old control and create a fresh map with fresh data sources, because disposing a control also disposes its map.

Existing `MapControl.UseGPU` code continues to work and is not obsolete. Setting it to `false` selects `Software`; setting it to `true` selects `Gpu`. Its getter returns `true` for either GPU option. In particular, setting `UseGPU = true` after selecting `AndroidSurfaceView` changes the default back to `Gpu`.

Keep the application's existing SkiaSharp setup (`builder.UseSkiaSharp()`). The SurfaceView host installs its own dedicated handler locally; no additional builder registration is required.

## Platform-specific GPU options

`Gpu` uses a TextureView on Android. To try a SurfaceView instead:

```csharp
var mapControl = new MapControl(RenderBackend.AndroidSurfaceView);
```

You can also set `MapControl.Backend = RenderBackend.AndroidSurfaceView` in shared startup code. On non-Android platforms it maps to `Gpu`, and `ActiveBackend` reports `Gpu`. Platform-specific enum values carry their platform in their name. The enum belongs to MAUI; it does not change the native UI projects' APIs.

On Android the SurfaceView host draws on demand, handles pointer coordinates in native pixels, and recreates its Skia context when Android recreates the GL context. `ActiveBackend` identifies the selected host implementation; native initialization errors are not silently replaced with a TextureView. The Android `FixInvisible()` workaround applies only to the TextureView backend and does nothing for Software or SurfaceView.

SurfaceView participates in Android composition differently from TextureView. Clipping, stacking, overlays, page transitions, background/resume and rotation still require physical-device verification for this implementation. No tested-device composition findings or performance advantage are claimed yet. Keep the default `Gpu` option unless your own tests support changing it.

### Compare backends in the MAUI sample

1. Run `Samples/Mapsui.Samples.Maui` on the target device. For a local vector-tile comparison, choose **ExperimentalVectorTiles / VexTileAsSKImage** from the category and sample lists. It uses the bundled Zurich MBTiles dataset and its fixed default style, without an online basemap.
2. Pan, zoom and rotate the map, then use the chevron to expand the **Rendering** card. Its header shows the backend used by the current control. Change the **Backend** picker inside the card; Android offers all three enum values, and other platforms offer Software and Gpu.
3. The expanded card shows the constructed backend, the MAUI and native view types, and Skia's graphics API once a GPU paint creates a context. The sample does not identify the physical GPU. Switching recreates the selected sample, map, sources and control, and restores the camera position, resolution and rotation. Rapid selections cancel older pending switches.
4. Keep renderer, tile source, style, dependencies and viewport size constant. Use Release builds without a debugger for performance observations. Align resolved SkiaSharp versions when comparing against the separate native Android sample.

This is a manual comparison. It does not automate gestures, reset operating-system caches, measure display FPS, or produce a benchmark report. The [performance widget](performance-widget.md) can show renderer-call timing; paint callback cadence and display presentation timing are different measurements. SurfaceView may perform similarly to, better than, or worse than TextureView.

## Renderer

The renderer is a separate choice from the backend and host view. These backend options do not change `IMapRenderer` or `DefaultRendererFactory`. See the [experimental renderer](experimental-renderer.md), [custom style renderers](custom-style-renders.md), and [custom layer renderers](custom-layer-renderer.md) for renderer configuration and extensibility.
