---
title: Mapsui 5.4: compile against SkiaSharp 4, remove Avalonia 11 and MAUI 8
status: done
date: 2026-11-24
supersedes: skiasharp-upgrade-follow-up.md
---

# Mapsui 5.4: compile against SkiaSharp 4, remove Avalonia 11 and MAUI 8

Done: implemented in one pull request; the decisions are recorded in [0002-skiasharp-4-minimum-version.md](../../docs/decisions/0002-skiasharp-4-minimum-version.md). Before this change, Mapsui 5.3 was compiled against SkiaSharp 3.119.4 and only let applications resolve SkiaSharp 4 (pull requests #3380 and #3390, see [skiasharp-upgrade-follow-up.md](skiasharp-upgrade-follow-up.md)). Mapsui 5.4 moves to SkiaSharp 4 itself. The user-facing description is *Changes in Mapsui 5.4* in [v6.0-upgrade-guide.md](../../docs/general/markdown/v6.0-upgrade-guide.md).

The work fits in one pull request: package versions, project removal, and moving path construction to `SKPathBuilder`.

## Decisions

| Decision | Choice | Why |
|---|---|---|
| SkiaSharp version | **4.151.1** for every `SkiaSharp.*` package | A library should depend on the lowest version it needs; applications can always pick a newer one. 4.151.1 is what Uno.Sdk 7.0.5 pins, and 7.0.5 is the minimum Uno SDK that `Mapsui.Uno.WinUI` 5.3 requires. A higher floor would make Uno 7.0.5 apps fail restore with a package downgrade error (NU1605). Uno.Sdk 7.0.22 pins 4.156.0, so newer Uno apps are unaffected either way. |
| HarfBuzzSharp version | **14.2.1.1**, including `HarfBuzzSharp.NativeAssets.Linux` and `.WebAssembly` | It is what `SkiaSharp.HarfBuzz` 4.151.1 depends on. Pinning the Linux native asset centrally fixes the mismatch documented for 5.3, where Avalonia and RichTextKit pulled in an 8.x or 2.x native library under the 14.x managed one. |
| Svg.Skia | **5.2.3** (from 3.2.1) | Svg.Skia 3 is built against SkiaSharp 3; running it on 4 risks missing-method failures at runtime. 5.2.3 is built for SkiaSharp 4 and needs only SkiaSharp 4.148 and HarfBuzzSharp 14.2.0, below our floor. 5.2.4 would raise the HarfBuzzSharp floor to 14.4.0 for no gain. |
| RichTextKit | **Stay on 0.4.167** in this release | Every 0.6.x release requires SkiaSharp 4.153.1, above the floor that Uno 7.0.5 allows. 0.4.167 already runs on SkiaSharp 4: the 5.3 compatibility tests cover wrapped, bidirectional and emoji callout text. Moving to RichTextKit 0.6 is a follow-up (see below). |
| Avalonia 12 version | **12.1.2**, now the central Avalonia version | Unchanged floor; 12.1.2 runs on SkiaSharp 4. Avalonia 12.2.0 still declares SkiaSharp 3.119.4, so a newer Avalonia buys nothing for this change. |
| `Mapsui.Avalonia` (Avalonia 11) | Removed. The three source files that `Mapsui.UI.Avalonia12` links from its folder move into `Mapsui.UI.Avalonia12` | Avalonia 11 only works with SkiaSharp 3. The `Mapsui.Avalonia12` package name and the `Mapsui.UI.Avalonia` assembly and namespace stay, so Avalonia 12 users change nothing. |
| `Mapsui.Maui8` | Removed | .NET 8 MAUI is out of support, and its SkiaSharp 3.119.2 pin is the last thing keeping a SkiaSharp 3 build in the repository. |
| Compatibility overrides | All `VersionOverride`s that existed only to put samples, tests and Avalonia 12 on SkiaSharp 4 or 3 are removed, along with the `SkiaSharp4Compatibility*`, `Avalonia12SkiaSharp*`, `Maui8*` properties | With one SkiaSharp version in the central catalog they have nothing left to bridge. |
| Obsolete `SKPath` methods | Move to `SKPathBuilder` (build, then `Detach()`), about 120 call sites in both renderers and three samples. Vendored VexTile copies keep the upstream calls, with CS0618 relaxed in their folder's `.editorconfig` | SkiaSharp 4 marks `SKPath.MoveTo`, `LineTo`, `AddRect` and the like obsolete, and the renderers treat warnings as errors. The 5.3 bridge compiled against SkiaSharp 3, so these never surfaced. The vendored files are kept close to upstream so they can be re-synced. |
| Target frameworks | Unchanged (`net8.0;net9.0` for the renderers and Avalonia 12, `net9.0` platform heads) | SkiaSharp 4 ships `net6.0` and `netstandard2.0` assets, so `net8.0` libraries keep working. Dropping a target framework is a release-policy decision, not part of this change. |

## Steps

1. **Central versions.** In `Directory.Packages.props`: SkiaSharp packages to 4.151.1, HarfBuzzSharp packages to 14.2.1.1 (add `HarfBuzzSharp.NativeAssets.Linux`), Svg.Skia to 5.2.3, Avalonia packages to 12.1.2, drop `Avalonia.ReactiveUI` and the `Mapsui.Avalonia` package entry.
2. **Build properties.** In `Directory.Build.props` remove `Maui8Version`, `Maui8SkiaSharpVersion`, `SkiaSharp4CompatibilityVersion`, `HarfBuzzSharp4CompatibilityVersion`, `Avalonia12SkiaSharpVersion`, `Avalonia12HarfBuzzSharpVersion`. Keep `Avalonia12Version` only if a project still needs it; otherwise the central catalog is the single source.
3. **Avalonia 11 removal.** Move `MapControl.cs`, `GlobalSuppressions.cs` and `Extensions/PointExtensions.cs` from `Mapsui.UI.Avalonia` into `Mapsui.UI.Avalonia12`. Delete `Mapsui.UI.Avalonia` and `Samples/Avalonia`. Remove them from `Mapsui.slnx` and every `.slnf`; point the Avalonia, Linux, Mac and WithoutMaui filters at the Avalonia 12 projects that the slnx includes.
4. **MAUI 8 removal.** Delete `Mapsui.UI.Maui8`, its steps in `dotnet.yml`, `dotnet-validate-nugets.yml` and `dotnet-release-nugets.yml`.
5. **Overrides.** Remove the SkiaSharp/HarfBuzz/Avalonia `VersionOverride`s from `Mapsui.UI.Avalonia12`, the Avalonia 12 samples and `Samples/Avalonia12/Directory.Build.props`, `Tools/Mapsui.Tools.ImageComparison`, the MAUI sample and `Mapsui.UI.Maui.Tests`. Revisit the Uno project's comment on `CentralPackageTransitivePinningEnabled`: with the central catalog on SkiaSharp 4 the reason it gives no longer holds.
6. **Code.** Svg.Skia 5 needed no changes. SkiaSharp 4's obsolete `SKPath` methods did: see the decision above.
7. **Tests.** `SkiaSharpCompatibilityTests` asserts HarfBuzzSharp 14.2.1.102; make it assert the major versions (SkiaSharp 4, HarfBuzzSharp 14) instead of one patch release, so it keeps meaning "the renderer runs on SkiaSharp 4" rather than "on this exact build".
8. **Documentation.** In the upgrade guide, replace the 5.3 bridge text and *Coming in Mapsui 5.4* with a *Changes in Mapsui 5.4* section: SkiaSharp 4.151.1 minimum, Avalonia 11 and MAUI 8 packages removed, Uno 7.0.5 still supported. Remove Avalonia 11 from `README.md`, `docs/general/markdown/index.md`, the `Mapsui.Avalonia12` package README and the Android build list in the checklist skill.
9. **Finish.** Move the decisions to a decision record in `docs/decisions/`, set this plan to done and move it to `plans/done/` in the same pull request.

## Validation

Full checklist: this is a dependency change that touches every platform. In particular:

- Build every remaining head, including Android, iOS (where the machine allows), Blazor, Uno, WinUI, WPF, Windows Forms, Eto and Avalonia 12.
- `Mapsui.Tests`, `Mapsui.Nts.Tests`, `Mapsui.Experimental.VectorTiles.Tests`, `Mapsui.UI.Maui.Tests`.
- The full rendering regression suite for both renderers. SkiaSharp 4 may anti-alias or shape text slightly differently; differences are inspected, not blindly accepted.
- Pack the packages and check that `Mapsui.Rendering.Skia.nuspec` lists SkiaSharp 4.151.1, HarfBuzzSharp 14.2.1.1 and Svg.Skia 5.2.3.

## Risks

- **Eto.SkiaDraw** (latest 0.3.0) is built against SkiaSharp 2.88. It ran on SkiaSharp 3; SkiaSharp 4 removed more obsolete APIs. A clean build does not prove it works at runtime, so the Eto sample needs a manual run.
- **RichTextKit 0.4.167** is built against SkiaSharp 2.88 as well. The 5.3 compatibility tests show it working on SkiaSharp 4, and those tests stay.
- **Uno apps with the `Svg` feature** get Svg.Skia 3.0.6 from Uno.Sdk 7.0.5, below Mapsui's 5.2.3. Already the case with 5.3's 3.2.1; the upgrade guide tells them to set `SvgSkiaVersion`. Not covered by CI, because neither the Uno library nor its sample enables the feature.
- **Avalonia 12.1 Vulkan on Windows** crashes with SkiaSharp 4 (documented in the upgrade guide). This becomes the default for every Avalonia user instead of an opt-in, so the guidance moves into the 5.4 section.

## Follow-ups (not in this pull request)

- Move to RichTextKit 0.6 once raising the SkiaSharp floor to 4.153.1 is acceptable, for example when Uno.Sdk 7.0.22 or later becomes the Uno minimum. Needs its own regression run, since it changes the text layout library.
- Consider a non-blocking CI lane that builds against the newest stable SkiaSharp 4, so the floor and the latest release are both tested.
