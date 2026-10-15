---
title: MAUI Android Surface Selection and Comparison Plan
description: Add an optional Android GPU SurfaceView host to the MAUI MapControl behind a single backend enum, and let users compare backends in the sample app.
---

# MAUI Android surface selection and comparison

Date: 2026-10-12
Status: Feature implementation and documentation complete. Automated builds, tests, style and changed-file formatting passed; repository-wide whitespace still reports pre-existing line-ending failures. No Android manual test will be done before the merge request, at the user’s direction. The automated comparison/report harness remains a separate deliverable.

## Objective

Allow users to compare the existing MAUI GPU TextureView host, an Android GPU SurfaceView host, and software rendering in one application. Keep the existing behavior by default and contain platform-specific code behind a small internal drawing-view abstraction.

The motivating report is that Mapsui.Android performs better than Mapsui.Maui with vector tiles. Different native surfaces are a plausible cause, not an established diagnosis. The comparison must allow equal performance or a slower SurfaceView result; do not automatically describe either GPU option as faster. The original sequence required measurements before adding public API. On 2026-10-12 the user explicitly directed: “Finish the feature now; test on a phone later.” On 2026-10-14 the user decided to proceed to a merge request without Android manual testing. Steps 3–5 therefore proceed without a performance conclusion; the physical-device checks remain outstanding.

## Vocabulary: three independent layers

Mapsui already has three independent choices. This plan, its code, its sample UI and its documentation must use these terms consistently and must not call all of them "rendering modes" or "renderers".

| Layer | What it picks | Where it lives today | Scope |
|---|---|---|---|
| Renderer | How map content is drawn (the `IMapRenderer`, Skia today) | `DefaultRendererFactory`, `RenderController.SetMapRenderer` | all platforms |
| Backend | Software (CPU raster) vs GPU | `MapControl.UseGPU` in each UI project | per UI platform |
| Host view | Which native view hosts the GPU backend: TextureView or SurfaceView | MAUI `RenderBackend.AndroidSurfaceView` | Android GPU only |

- *Renderer* is reserved for `IMapRenderer`. This plan does not change it.
- *Backend* is the CPU/GPU choice. This plan extends it.
- *Host view* (or *surface*) is TextureView vs SurfaceView. This plan adds it as one extra backend value rather than as a separate knob.

## Scope and decisions

### Public API: one shared enum, `UseGPU` kept as a shim

Replace the bool-plus-Android-enum design with a single enum defined in the shared target of `Mapsui.UI.Maui`, so shared MAUI startup code can set it without platform guards. The enum belongs to `Mapsui.UI.Maui` only. Names are proposed API names.

```csharp
namespace Mapsui.UI.Maui;

public enum RenderBackend
{
    /// <summary>SKCanvasView on every platform. CPU rasterization.</summary>
    Software,
    /// <summary>The platform default GPU view. TextureView on Android. This is the default.</summary>
    Gpu,
    /// <summary>
    /// Android only: host the GPU backend in a SurfaceView instead of a TextureView.
    /// On other platforms this behaves as Gpu and the control reports Gpu as its active backend.
    /// </summary>
    AndroidSurfaceView,
}

public partial class MapControl
{
    /// <summary>Default backend used by the parameterless constructor. Read once during construction.</summary>
    public static RenderBackend Backend { get; set; } = RenderBackend.Gpu;

    /// <summary>Compatibility shim over <see cref="Backend"/>. Existing applications need no changes.</summary>
    public static bool UseGPU
    {
        get => Backend != RenderBackend.Software;
        set => Backend = value ? RenderBackend.Gpu : RenderBackend.Software;
    }

    /// <summary>The backend this instance actually constructed. Never reports AndroidSurfaceView unless a SurfaceView was created.</summary>
    public RenderBackend ActiveBackend { get; }

    public MapControl() : this(Backend) { }
    public MapControl(RenderBackend backend) { /* snapshot into a readonly field, create host view */ }
}
```

Decisions:

- Keep the existing public static `MapControl.UseGPU` property and parameterless constructor working unchanged. Do not mark `UseGPU` obsolete in this change; it is widely used and the shim is cheap.
- `Gpu` remains the default. Most users never touch the new API.
- The advanced choice is one extra value in the same list, not a second property with a precedence rule.
- Add the `MapControl(RenderBackend)` constructor overload now. It removes the need for the sample to temporarily change static defaults and restore them, and it gives explicit per-instance selection without a wider options object. The static remains the default for XAML users.
- Snapshot the selected backend during construction into a readonly field. Existing controls retain their selected implementation when the static default changes. Lifecycle callbacks must use the instance selection, never the current static default.
- Expose `ActiveBackend` so the sample UI, logs and reports share one vocabulary with the configuration API. Report initialization failures clearly; never report `AndroidSurfaceView` while a TextureView is in use.
- On non-Android platforms `AndroidSurfaceView` maps to `Gpu` at the factory boundary. This is explicit, documented behavior, and `ActiveBackend` reports `Gpu`. It is not a silent fallback.
- The enum is MAUI-only and is not shared with the native UI projects. Software vs GPU is the only choice common to all platforms, and each native project's existing `UseGPU` bool already expresses it. Everything beyond that is platform-specific, so a shared enum would collect values that each apply to one platform. Mapsui.UI.iOS, Mapsui.UI.Blazor and Mapsui.UI.WindowsForms are unchanged; if one ever needs more choices, it defines its own enum.

### Rules for keeping the enum understandable

MAUI has one control for all platforms, so its enum necessarily holds some platform-specific values. Two rules keep the list short and readable:

1. **`Software` and `Gpu` are universal.** `Gpu` means the best GPU option for the current platform. Most users only ever choose between these two.
2. **Every platform-specific value carries its platform as a name prefix**, for example `AndroidSurfaceView`. On other platforms it behaves as `Gpu`, and `ActiveBackend` reports `Gpu`.

A realistic long-term list is `Software`, `Gpu`, `AndroidSurfaceView` and perhaps one iOS-specific value. If the list grows beyond a handful of platform-specific values, reconsider the design, for example `Software`/`Gpu` plus a separate per-platform options object.

### Implementation containment

- Place the SurfaceView host implementation under `Mapsui.UI.Maui/Platforms/Android/`. It must not leak Android types into the generic target or other platform assets. The enum and properties live in the shared target.
- Use a small internal drawing-view interface, provisionally `IMapDrawingView`. Its name must not contain "renderer", to avoid confusion with `IMapRenderer`. Allow one conditional Android selection at the factory boundary rather than scattering platform checks through shared control behavior.
- Switch backends in the sample by recreating the control and map using the constructor overload. Do not add live surface replacement to the library in this change.
- Keep renderer selection, tile source, style and dependency versions constant during a comparison. Do not change the default Mapsui renderer or add SQLite-provider selection.
- No new dependencies or package upgrades are planned.

Configuration from shared MAUI startup code would look like:

```csharp
MapControl.Backend = RenderBackend.AndroidSurfaceView; // Android uses SurfaceView, other platforms behave as Gpu
```

or per instance:

```csharp
var mapControl = new MapControl(RenderBackend.Software);
```

## Repository observations before implementation

- [MAUI MapControl](../../Mapsui.UI.Maui/MapControl.cs) creates `SKGLView` or `SKCanvasView` in its constructor based on `UseGPU`. Painting, pixel-density calculation, invalidation and disposal currently refer directly to those fields.
- [Android partial MapControl](../../Mapsui.UI.Maui/Platforms/Android/MapControl.cs) contains a TextureView-specific navigation workaround involving `SKGLViewHandler` and `GLTextureView`, gated on `UseGPU`. It must be gated on the TextureView backend specifically and must not run for SurfaceView or Software.
- [Native Android MapControl](../../Mapsui.UI.Android/MapControl.cs) provides a reference for `SKGLSurfaceView` integration. Reusing the whole native MapControl would also change input and control behavior, making it a poor substitute for changing just the surface.
- [Shared RenderController](../../Mapsui.UI.Shared/RenderController.cs) coordinates rendering and records renderer-call timing. It should remain the common drawing path.
- Mapsui.UI.iOS, Mapsui.UI.Blazor and Mapsui.UI.WindowsForms each have their own static `UseGPU` bool; Mapsui.UI.WinUI has no GPU path. This plan does not change them.
- [MAUI sample page](../../Samples/Mapsui.Samples.Maui/View/MainPage.cs) owns a readonly control, while its view model holds another reference and binds the map. Switching requires updating ownership, bindings and references together.
- [MAUI tests](../../Tests/Mapsui.UI.Maui.Tests/Mapsui.UI.Maui.Tests.csproj) run on Windows. They cannot validate Android surface creation or lifecycle. Some existing MapControl lifetime tests are ignored.
- The MAUI sample currently overrides SkiaSharp to 4.151.2 while Mapsui libraries target 3.119.4. A single-app comparison holds the resolved version constant. Any separate native Android comparison must align resolved versions explicitly.

## Implementation sequence

Originally, steps 1–2 had to establish a measurable benefit before steps 3–5. The user explicitly changed that sequence to finish implementation now and test on a phone later. Measurements and runtime checks remain outstanding; step 6 remains a separate deliverable.

### 1. Verify the Android hosting boundary (spike, local only)

- [x] Inspect the resolved SkiaSharp 3 and 4 view/handler APIs and existing compatibility helpers before choosing concrete adapter types.
- [x] Establish a minimal MAUI-hosted SurfaceView with a dedicated internal view/handler (implemented with `GLSurfaceView` and `SKGLSurfaceViewRenderer` for context recreation, as explained below). Do not assume the existing TextureView handler accepts a SurfaceView or replace the handler globally for all `SKGLView` instances.
- [x] Establish explicit handler registration/creation using supported MAUI APIs. Prefer local setup for the dedicated view; if builder registration is required, document and expose it for the opt-in path. Existing applications must need no startup changes for existing backends.
- [ ] Verify actual paint callback threading, invalidation, native dimensions, GPU-context access and basic touch delivery.
- [x] Confirm project target selection excludes the Android files from the generic build. The MAUI `_MauiRemovePlatformCompileItems` target removes Android platform files; the new native host and adapter are excluded from non-Android targets without a feature symbol.

Exit criterion: a clear hosting implementation that works with the supported dependency combinations, without changing default behavior.

### 2. Measure before adding public API

- [ ] On one physical device, Release, no debugger, same resolved SkiaSharp version and the same local vector-tile scenario, compare: MAUI TextureView, MAUI SurfaceView (step 1 spike), and the native Mapsui.Android sample.
- [ ] In the same session rule out the cheaper suspects: the sample's SkiaSharp 4 override vs the library's SkiaSharp 3 baseline, debug vs release, and MAUI touch/layout overhead unrelated to the surface.
- [ ] Record renderer-call durations and paint callback cadence for each. Label paint cadence as paint cadence, not display FPS.
- [ ] Write the result into this plan under a "Spike result" heading, including device, versions and the numbers.

Original exit criterion: measurements supporting a public option. The user has explicitly deferred this criterion until physical-device testing; record any eventual result, including equal or worse performance.

### Implementation result — 2026-10-12

The public enum, default and per-instance selection, compatibility shim, internal adapters, Android SurfaceView host, manual sample controls and user documentation are implemented. `Gpu` remains the default, and no dependency versions or core renderer code changed. The temporary `MapsuiAndroidSurfaceSpike` build property has been removed; select `RenderBackend.AndroidSurfaceView` through the supported API.

Handler setup is local to the dedicated view, before the parent maps its Content. No global handler registration or additional application startup call is required. The Android host uses `GLSurfaceView` and the public SkiaSharp `SKGLSurfaceViewRenderer`, with `WhenDirty` rendering, native pixel coordinates and the current `GRContext` forwarded to MapControl. The wrapper abandons/disposes the previous Skia context and creates a new renderer on `OnSurfaceCreated`; plain `SKGLSurfaceView` does not reset that context in the supported SkiaSharp implementation. This is a focused adjustment to the initially proposed native host, necessary to avoid retaining a lost EGL context.

API inspection covered the [SkiaSharp 3.119.4 handler](https://github.com/mono/SkiaSharp/blob/v3.119.4/source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Android.cs), [4.151.2 handler](https://github.com/mono/SkiaSharp/blob/v4.151.2/source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Android.cs), and [native renderer lifecycle](https://github.com/mono/SkiaSharp/blob/v3.119.4/source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceViewRenderer.cs). Native drawing pauses on detach, visibility loss and window stop, and resumes with an explicit redraw. Disconnect stops callbacks and waits for the GL frame before map disposal. A changed GPU context requests a complete repaint through existing RenderService ownership. The TextureView reflection workaround remains exclusive to `Gpu` instances.

The sample keeps its generic first-category startup. For a local comparison, select `ExperimentalVectorTiles / VexTileAsSKImage`, which uses Zurich MBTiles without an online basemap. Each switch constructs a fresh sample, map, sources and control; preserves the camera for the same scenario; and cancels/serializes superseded requests. See [Rendering options](../general/markdown/rendering-options.md) for API examples and the manual comparison workflow.

**Device and numbers:** no Android device is attached (`adb devices -l` returned an empty list). Renderer-call durations and paint cadence have **not been measured**. Paint threading, first paint, touch, navigation, background/resume, context recreation, rotation, clipping and composition still require device verification. No performance advantage or tested-device composition behavior is claimed.

**Decision:** proceed to review without Android manual testing. Steps 1–2 retain their unchecked runtime/measurement tasks below. Step 6 is tracked in the [separate comparison harness plan](maui-backend-comparison-harness-plan.md).

### Automated validation

Full checklist selected because shared MAUI lifecycle behavior and public APIs change. Windows unit tests and offscreen rendering regressions do not establish Android runtime correctness.

- All six required Android Debug builds passed, plus generic MAUI and both MAUI/MapView Windows sample builds. The Android library compiles against SkiaSharp 3.119.4 and the MAUI sample resolves 4.151.2. The MAUI 8 project also links this shared source; its enum/adapters/native hosts were added to its explicit compile items. Both generic and Android MAUI 8 builds passed against SkiaSharp 3.119.2.
- 451 tests passed: core 269, NTS 3, vector tiles 5, MAUI 18 and Skia 156. Three pre-existing MAUI lifetime tests remain ignored. The 15 new MAUI cases cover selection, static snapshots, non-Android mapping, disposal, coordinate scaling, cancelled touch and serialized/cancelled sample switches. Standard-renderer regressions passed; no reference images changed.
- Changed-file whitespace and style checks passed for library (including Android files), sample and tests. Full whitespace verification fails on existing ENDOFLINE issues in unchanged files, with zero diagnostics in changed C# files. Full style verification passed after the MAUI 8 source-link fix (workspace-load warnings only).
- The final MkDocs site build passed; every pre-existing navigation page remains present. Its existing projections source-link warning is unrelated to this change. `git diff --check` passed.
- Existing duplicate SQLite native-library warnings occur in the Android samples; the Windows sample reports existing MVVM AOT warnings. No dependency versions were changed.

The full checklist cannot be marked fully passing because repository-wide whitespace and physical-device verification remain outstanding. The user chose to proceed without Android manual testing; the merge request must not claim Android runtime or performance validation. Other non-Android platforms still need runtime smoke tests; Windows compilation/unit coverage is not a substitute.

### 3. Introduce the enum, shim, constructor overload and internal drawing-view abstraction

- [x] Add `RenderBackend`, `MapControl.Backend`, the `UseGPU` shim, `ActiveBackend` and the `MapControl(RenderBackend)` constructor with XML documentation covering defaults, non-Android mapping of `AndroidSurfaceView` and construction-time snapshot behavior.
- [x] Add the internal `IMapDrawingView` interface and adapters for the existing GPU and software views.
- [x] Keep its contract limited to the hosted MAUI view, canvas pixel dimensions, invalidation, paint/touch/size notifications, and view lifecycle cleanup.
- [x] Supply canvas and optional GPU context through paint notifications. Use borrowed callback resources; do not retain or dispose event-owned canvases or surfaces.
- [x] Normalize input at the adapter boundary so all backends reach the same MapControl gesture logic with consistent pointer identities, cancellation and coordinate units.
- [x] Replace direct `_glView`/`_canvasView` branching in MapControl with the adapter selected from the instance snapshot. Keep map ownership, navigation, shared rendering coordination and gesture policy in MapControl.
- [x] Preserve pixel-density scaling exactly once, on-demand rendering, initialization behavior and skipped-paint invalidation completion. Avoid new per-frame allocations in the adapter.
- [x] Preserve the public `FixInvisible()` entrypoint. It delegates to the TextureView backend only and safely does nothing for Software and SurfaceView.
- [x] Gate the Android page-navigation workaround on the TextureView backend instead of `UseGPU`.

Exit criterion: the two existing backends behave as before, including on non-Android platforms, `UseGPU` round-trips through `Backend`, and `new MapControl()` still honors the static default.

### 4. Add the Android SurfaceView implementation

- [x] Select the SurfaceView adapter at the factory boundary from the instance snapshot; map `AndroidSurfaceView` to the TextureView adapter on non-Android targets and set `ActiveBackend` accordingly.
- [x] Implement the SurfaceView adapter and its MAUI host, including measurement/layout, native touch translation and redraw requests.
- [x] Set the current GPU context during painting through the supported compatibility path. Do not assume that requesting a redraw happens on the paint thread.
- [x] Handle appearing/disappearing, background/resume, size changes, rotation, handler disconnect and surface/context recreation. Stop callbacks after disposal and recreate resources tied to a lost context as required by existing RenderService ownership.
- [x] Do not apply the TextureView reflection workaround to SurfaceView.
- [ ] Verify clipping, stacking, overlays and page transitions. SurfaceView may have observable composition differences; document those found on tested devices.
- [x] Report initialization failures clearly. If SurfaceView creation fails, fail or fall back visibly and make `ActiveBackend` reflect the real view.

Exit criterion: all three Android backends are usable and `ActiveBackend` matches the actual native view.

### 5. Add manual comparison controls to the MAUI sample

- [x] Expose Software, Gpu and AndroidSurfaceView on Android. Show only supported choices elsewhere. Use the `RenderBackend` names in the UI.
- [x] Keep the sample's generic startup selection. Offer a fixed local vector-tile dataset/style as a manually selected comparison scenario, independent of network timing. Avoid the online basemap used by some existing vector-tile samples.
- [x] Preserve the selected scenario and viewport when changing backends. Cancel work and detach bindings/events before disposing the old control.
- [x] Recreate the map, data sources and render resources from a scenario factory. MapControl disposal currently disposes its map: never attach the disposed map, a disposed sample source, or old-context GPU resources to the new control.
- [x] Construct the new control with `new MapControl(backend)`. Do not change or restore static defaults in the sample.
- [x] Update the page and view-model control references and map binding together. Serialize switches so rapid taps cannot overlap teardown and creation.
- [x] Display `ActiveBackend` and allow the user to pan and zoom freely.

Exit criterion: a user can compare the same scene in each backend without rebuilding or editing application code.

### 6. Repeatable comparison and copyable report (separate deliverable)

This step is valuable but large. Deliver it as its own plan or pull request so steps 3 to 5 can land without it. Requirements carried over:

- [ ] Add Run comparison, Cancel and Copy results actions. Manual comparison remains available separately.
- [ ] Drive the same timed Navigator sequence for each backend with identical viewport size, resolution, style and tile source. Disable manual gestures and unrelated animations during measurement.
- [ ] Separate loading/preparation measurement from drawing after required tiles are ready. Track tile completion/failure explicitly and use a timeout; do not infer readiness from an arbitrary delay.
- [ ] Define cache conditions precisely: recreate/reset application caches for the loading phase, then warm the scene for the drawing phase. Do not claim to clear SQLite or operating-system disk caches.
- [ ] Repeat runs in alternating backend order to reduce order and warm-up bias. Record run order and warm-up policy; this is a practical diagnostic, not a controlled hardware benchmark.
- [ ] Record renderer-call duration distributions and paint callback intervals/counts over the test window. Measure source reads and tile preparation separately where possible using sample-side wrappers.
- [ ] Label paint cadence as paint cadence, not display FPS. If presentation timing is needed to explain a gap, add an Android trace as a follow-up rather than claiming it was measured.
- [ ] Include device/Android version, app/build identity, debugger state, resolved Mapsui/SkiaSharp versions, `ActiveBackend`, renderer, viewport pixels/density, scenario, cache policy, errors and results in a human-readable report.
- [ ] Copy locally only; no automatic submission. Avoid absolute file paths and credentials. Show incomplete or failed runs explicitly and do not declare an automatic winner.

## Validation

For implementation, select the **full checklist** before editing code because the abstraction changes shared MAUI lifecycle behavior and introduces a public API. Use the repository checklist skill and reassess after editing. Its required Android builds, applicable unit tests, formatting, compatibility and review checks remain mandatory; this plan does not replace them.

Additional coverage for this feature:

- Meaningful tests for: `UseGPU` round-tripping through `Backend` in both directions; the parameterless constructor honoring the static default; the overload ignoring the static default; construction-time snapshots surviving later static changes; `AndroidSurfaceView` mapping to `Gpu` on non-Android with `ActiveBackend` reporting `Gpu`; disposal/callback shutdown; coordinate scaling; and sample switch cancellation. Restore static settings between tests and avoid parallel interference.
- Build generic and Android MAUI targets, affected sample apps and available non-Android consumers. Validate both the library's SkiaSharp 3 baseline and the sample's SkiaSharp 4 consumer combination.
- Run applicable MAUI tests, noting their Windows-only scope and ignored lifetime tests rather than counting those as Android validation.
- Load the rendering regression skill and run the required rendering coverage. Offscreen regression tests do not validate Android composition or surface lifecycle.
- On physical Android hardware, exercise each backend through repeated switching, pan/pinch/rotate, resize, background/resume, page navigation, overlays, clipping and disposal. Check for blank frames, stalled drawing, exceptions, ghost touches and sustained memory/resource growth.
- Run Release comparisons without a debugger on at least two devices/GPU families before making any broader performance claim. If only one device is available, report conclusions as device-specific.
- Smoke-test the existing GPU/software paths on supported non-Android platforms. Record any unavailable platform verification as outstanding rather than passed.
- Ensure reports distinguish renderer time, paint cadence and loading time. A performance improvement is not an acceptance requirement; a valid comparison is.

## Documentation

### State before implementation

- No user documentation page mentions `UseGPU` or the Software/GPU choice today, so even the existing option is undocumented.
- Renderer topics exist but are scattered: [Mapsui components](../general/markdown/mapsui-components.md) has a short MapRenderer section, and there are separate pages for the experimental renderer and three custom renderer pages.
- The navigation in [mkdocs.yml](../general/mkdocs.yml) is one flat list of about 24 pages that mixes getting started, concepts, how-to guides, extensibility and project information.

### New page: Rendering options

Add one page, provisionally `docs/general/markdown/rendering-options.md`, covering all rendering choices. Do not add a standalone Android SurfaceView page; users asking "should I use the GPU" or "why is my map slow on Android" would not find it.

Order the content by how often users need it:

- [x] **Introduction:** the three-layer table (renderer, backend, host view) from the vocabulary section of this plan, in user-facing wording.
- [x] **Backend: Software or GPU.** Document `UseGPU`, and for MAUI `RenderBackend`, `Backend`, the constructor overload and `ActiveBackend`. Include a table of defaults per UI framework, verified against the code at implementation time. Known today: MAUI, iOS and Blazor default to GPU; Windows Forms defaults to software; WinUI has no GPU path; native Android uses its own instance `RenderMode` property (`SkiaRenderMode.Hardware` by default, hosted in a SurfaceView). Check the remaining frameworks.
- [x] **Platform-specific GPU options.** A short section on `AndroidSurfaceView` in MAUI: when to try it, that it behaves as `Gpu` on other platforms, the naming rule for platform-specific values, startup/registration requirements if any, known composition limitations found on tested devices, and how to confirm the result with `ActiveBackend`. Link to the sample comparison workflow.
- [x] **Renderer.** One short paragraph explaining that the renderer is a separate choice, linking to the experimental renderer and custom renderer pages instead of repeating them.

Cross-links:

- [x] Add one sentence to the MapRenderer section of the components page linking to the new page.
- [x] Link from the performance widget page to the new page, and note there that the widget measures draw time, not display frame rate.
- [x] Add an FAQ entry along the lines of "the map is slower in MAUI on Android than in native Android" that points to the new page.

### Navigation regrouping (deferred separate change)

Regroup the flat navigation into sections in a separate documentation change. This feature only adds the new page to the existing flat navigation, keeping the backend pull request focused.

| Section | Pages |
|---|---|
| Getting started | introduction, samples, FAQ, NuGet of latest build |
| Concepts | components, resolution, projections, async fetching |
| Data and styling | working with layers, WMS, vector style, image source, font source, map info, static image generation |
| Rendering | rendering options (new), custom style renderers, custom point style renderer, custom layer renderer, experimental renderer |
| Diagnostics | logging, performance widget |
| About the project | roadmap, projects that use Mapsui, experimental packages, documentation of v4 |
| Upgrade guides | unchanged |
| Contributing to Mapsui | unchanged |

- [ ] For the separate navigation change, verify the site builds and that no page is dropped from it.

### Other documentation tasks

- [ ] Document the comparison's cache and measurement limits when step 6 lands. Update the upgrade guide if implementation introduces observable compatibility concerns.

## Completion

- [x] Review the final diff for unnecessary public abstractions, platform branches outside the factory/Android files, dependency changes and resource ownership errors.
- [ ] Close validation after the repository-wide whitespace check and outstanding device/platform checks pass. Feature code is implemented; the user chose to proceed to review without Android manual testing.

Expected implementation areas: `Mapsui.UI.Maui/MapControl.cs`, a new `RenderBackend` enum, internal drawing-view adapters, `Mapsui.UI.Maui/Platforms/Android/`, the MAUI sample page/view model and optional handler registration, focused tests, and user-facing documentation. Changes to Mapsui's core renderer or tile-loading implementation are not expected; reassess scope if the integration requires them.

## Deferred work

- Marking `UseGPU` obsolete once the enum has been available for a release.
- Live surface swapping, automatic backend selection, renderer/provider comparison matrices, changes to SQLite behavior, and changing the default host view.
- The repeatable comparison harness (step 6), tracked in [its own plan](maui-backend-comparison-harness-plan.md).

Comparing SurfaceView and TextureView within MAUI tests the surface hypothesis; it does not measure all overhead differences between a complete native Android application and a MAUI application.
