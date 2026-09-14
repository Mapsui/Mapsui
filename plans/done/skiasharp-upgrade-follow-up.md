# SkiaSharp Upgrade Follow-up

**Implementation status:** Done. The primary compatibility bridge is implemented and its pre-PR validation results are recorded below.

## Implementation progress (2026-09-14)

- Updated MAUI 9 to 9.0.120 so it can consume SkiaSharp 4.151.2.
- Updated Avalonia 12 and ReactiveUI.Avalonia to 12.1.2 while retaining Avalonia's supported SkiaSharp 3.119.4 and HarfBuzzSharp 8.3.1.5 versions.
- Added an explicit SkiaSharp 4.151.2/HarfBuzzSharp 14.2.1.102 dependency train to the MAUI sample and compatibility tests.
- Added runtime tests that assert the loaded managed/native SkiaSharp versions, render a bitmap, and exercise RichTextKit with wrapped bidirectional and emoji text.
- Made the MAUI compatibility test switchable from project references to freshly packed Mapsui packages, and added that test to the NuGet validation workflow.
- Updated the Avalonia 12 image-comparison tool for ReactiveUI 24's `RxVoid` API.
- Documented the supported SkiaSharp 3/4 bridge in the v6 upgrade guide.

The normal SkiaSharp 3 path, the SkiaSharp 4 project-reference path, and the SkiaSharp 4 packaged-consumer path pass. All mandatory Android builds pass. The standard renderer's complete 128-case regression suite passes. The experimental suite is not clean: five existing image comparisons failed before the run exhausted memory, and focused text coverage reproduced the existing `CalloutWrapAroundSample` reference mismatch while the RTL and emoji samples passed. No reference images were replaced.

### Retained target-framework matrix

| Integration | Target frameworks | Dependency strategy |
|---|---|---|
| Shared and experimental Skia renderers | `net8.0`, `net9.0` | Compile against SkiaSharp 3.119.4 |
| MAUI 9 | `net9.0`, `net9.0-android` | Package stays on SkiaSharp 3.119.4; sample/test consumers resolve SkiaSharp 4.151.2 |
| MAUI 8 | `net8.0`, `net8.0-android` | Isolated legacy SkiaSharp 3.119.2 pin |
| Avalonia 12 | `net8.0`, `net9.0` | Avalonia 12.1.2 with SkiaSharp 3.119.4 |
| Avalonia 11 and Eto | `net8.0`, `net9.0` | Existing coherent SkiaSharp 3 package graph |
| Android, iOS, WPF, Windows Forms, WinUI, Blazor, Uno | Existing net9 platform targets | Existing SkiaSharp 3 package graph |

## Purpose

Continue the SkiaSharp upgrade started by [pull request #3380](https://github.com/Mapsui/Mapsui/pull/3380) without waiting for future Avalonia releases.

The immediate goal is to publish a well-supported compatibility bridge:

- Mapsui is compiled against stable SkiaSharp 3.119.4 and uses APIs shared by SkiaSharp 3 and 4.
- Avalonia uses the newest stable package combination available when the work starts, while remaining on the SkiaSharp version Avalonia officially supports.
- MAUI consumers can resolve SkiaSharp 4 when another application dependency requires it.
- Packaged Mapsui artifacts are tested in that MAUI/SkiaSharp 4 configuration so compatibility is demonstrated through normal NuGet resolution, not only by replacing binaries in a test output folder.

The eventual direct move of Mapsui to SkiaSharp 4 is a later step, triggered when all required platform integrations—especially Avalonia—support it.

## Decisions

- Merge #3380 as the compatibility foundation instead of reopening or replacing Jonas's work.
- Use the best stable package versions available on the day each follow-up is implemented. Do not delay a current update in anticipation of a release expected the following week.
- Do not make a stable Mapsui package depend on a SkiaSharp preview. Preview and release-candidate packages may be exercised in a non-blocking CI lane.
- Keep the managed SkiaSharp packages, view packages, HarfBuzz packages, and native assets on a coherent release train for each target framework.
- Treat dropping a supported target framework as a Mapsui release-policy decision, not as incidental dependency cleanup.
- Keep unrelated CI versioning and VexTile warning-policy improvements out of the primary dependency follow-up PR.

## Version snapshot

Recheck all versions immediately before implementation. As of 2026-09-14:

- Mapsui main catalog before #3380: SkiaSharp 3.119.2.
- Compatibility bridge in #3380: SkiaSharp 3.119.4.
- Avalonia.Skia 12.1.2: depends on SkiaSharp 3.119.4.
- Avalonia's SkiaSharp 4 upgrade: open and assigned to the Avalonia 12.2 milestone, but not released.
- SkiaSharp's official support table: 4.151.2 is the newest supported stable release; use the support-channel classification rather than assuming that every suffix-free NuGet is production-supported.
- Svg.Skia 5.2.3: supports SkiaSharp 4, so it is no longer a future blocker.
- Topten.RichTextKit 0.4.167: still declares SkiaSharp 2.88.7 dependencies and requires explicit compatibility coverage.

## Primary follow-up pull request

### 1. Establish the post-merge baseline

- Update the working branch from the target branch after #3380 is merged.
- Review the final merge diff because the merged result may differ from the current PR head.
- Record the effective package graph and target frameworks for every Mapsui UI head.
- Restore the repository before editing so pre-existing warnings and failures are distinguishable from follow-up changes.

### 2. Refresh Avalonia to today's stable versions

- Update `Mapsui.UI.Avalonia12` and its samples to the newest stable, mutually compatible Avalonia 12 packages.
- Replace any remaining Avalonia 12 prerelease SkiaSharp override with stable SkiaSharp 3.119.4.
- Keep all packages belonging to one Avalonia line in lockstep unless Avalonia explicitly versions them independently.
- Keep the Avalonia 11 integration on the highest coherent 11.x package set supported by its ReactiveUI integration.
- If reaching the newest Avalonia 11 patch requires replacing `Avalonia.ReactiveUI` with `ReactiveUI.Avalonia`, treat that migration as a separate change unless it is demonstrably mechanical and behavior-neutral.
- Build and launch at least one Avalonia desktop sample; a clean restore alone is not sufficient for native Skia compatibility.

### 3. Add packaged-consumer compatibility coverage

- Pack the affected Mapsui packages with one unique local/CI prerelease version.
- Restore a clean consumer project from those packages rather than from project references.
- Give the consumer an explicit dependency on the newest officially supported stable SkiaSharp 4 release and the matching native assets.
- Exercise the Mapsui MAUI renderer through the consumer so NuGet selection, managed assembly loading, and native library loading are all covered.
- Assert or log the resolved SkiaSharp managed assembly version and native version to make accidental fallback to SkiaSharp 3 visible.
- Keep the normal SkiaSharp 3.119.4 test path as a required lane for Avalonia.
- Optionally test the latest SkiaSharp preview in a non-blocking scheduled lane; do not use it as the published dependency.

### 4. Audit dependencies around the renderer

- Confirm that `Svg.Skia`, `Topten.RichTextKit`, `Eto.SkiaDraw`, VexTile, and every `SkiaSharp.Views.*` package resolve without incompatible managed/native combinations.
- Do not upgrade Svg.Skia to its SkiaSharp 4-based release while the shared renderer remains declared on SkiaSharp 3 unless the resulting graph is intentional and tested on every affected head.
- Add focused text smoke coverage for RichTextKit: word wrapping, BiDi/RTL, emoji, font fallback, and supplied fonts.
- Record any dependency that merely works through assembly unification as a known compatibility dependency rather than claiming native SkiaSharp 4 support from that package.

### 5. Decide target frameworks explicitly

- Produce a table of current target frameworks and the SkiaSharp view package available for each one.
- Do not drop net8 merely to make the central version catalog uniform if a tested, isolated legacy pin can preserve the existing Mapsui contract.
- Account for .NET 8 and .NET 9 both reaching end of support on 2026-11-10.
- Prefer net10 as the next long-lived baseline when Mapsui next removes target frameworks.
- If this PR must remove public TFMs, stop and confirm the Mapsui major-version/release decision before proceeding.

### 6. Document the supported bridge

- Add a concise release note stating that Mapsui is compiled against SkiaSharp 3.119.4 but is tested for MAUI applications that resolve the supported SkiaSharp 4 stable line.
- Avoid describing Mapsui as directly targeting SkiaSharp 4 until its package dependency and source compilation actually do so.
- State that Avalonia remains on its officially supported SkiaSharp dependency.
- Link the remaining direct-upgrade work to AvaloniaUI/Avalonia#21696.

## Validation

This is a broad, cross-platform native dependency change. Select the **full checklist** before implementation and load the checklist skill at that time.

At minimum:

- Restore and build every affected UI project and sample for each retained target framework.
- Run the core and renderer unit tests.
- Run the full Windows rendering regression suite:

  ```powershell
  dotnet test Tests/Mapsui.Rendering.Skia.Tests --filter "TestSampleAsync"
  ```

- Run rendering regression tests with both the standard and experimental renderer configurations.
- Do not replace reference images merely because the Skia engine or encoder changed. Investigate every unexpected visual difference.
- Pack the Mapsui packages and build clean consumers against SkiaSharp 3.119.4 and the chosen SkiaSharp 4 stable release.
- Run platform smoke tests for MAUI/Android, Avalonia desktop, WPF, Windows Forms, WinUI, Blazor/WASM, Uno, iOS, and macOS wherever supported by the repository checklist and available CI agents.
- Confirm that packaged consumers load matching managed and native SkiaSharp versions at runtime.
- Run package validation and inspect generated NuGet dependency groups for every target framework.

## Acceptance criteria

- Avalonia 12 uses the newest stable compatible release available when the PR is prepared.
- No Avalonia project is forced to load a renderer compiled against SkiaSharp 4.
- A clean packaged MAUI consumer resolves and runs with the selected supported SkiaSharp 4 stable release.
- The existing SkiaSharp 3.119.4/Avalonia path remains functional.
- No stable Mapsui package depends on a SkiaSharp prerelease.
- Managed and native SkiaSharp assets are coherent in every tested application.
- Standard and experimental rendering regressions pass without unexplained reference-image changes.
- Any target-framework removal is deliberate, documented, and aligned with the Mapsui release version.

## Separate follow-up: CI package versioning

Jonas's `0.0.1-fork` fallback solves a real problem and can remain as the immediate fix. Harden it in a dedicated PR:

- Calculate the package version once per job and reuse it for pack and sample validation.
- For fork and PR builds, generate a unique SemVer prerelease such as `0.0.0-pr.<number>.<run>.g<sha>` rather than a constant version.
- Allow forks to produce downloadable NuGet artifacts without release tags.
- Require an exact valid version tag on the checked-out commit for an official Mapsui release; do not silently use the nearest older tag.
- Ensure fork workflows cannot publish with official Mapsui credentials.
- Prefer one shared script or reusable workflow step over separate Bash and PowerShell implementations of the version rules.

## Separate follow-up: VexTile warning policy

Do not expand the folder-scoped nullable suppression in the dependency PR.

- Decide whether `VexTileCopies` is immutable vendored source or Mapsui-maintained source. Its existing Mapsui-specific changes mean it currently behaves as maintained source.
- If Mapsui owns it, fix nullable annotations and behavior with focused tests before enabling warnings as errors.
- If it must remain synchronized with upstream, isolate it behind a clear project/source boundary, document the upstream revision and local patches, and apply a deliberate nullable policy at that boundary.
- Consider removing the project-only `TreatWarningsAsErrors` and its folder `.editorconfig` until that decision is made.
- Preserve the independent `$(NoWarn);NU5104` correction because it retains inherited warning settings.

## Later direct SkiaSharp 4 upgrade

Start the direct upgrade when a stable Avalonia package actually depends on SkiaSharp 4; a milestone assignment alone is not sufficient.

- Re-evaluate the supported SkiaSharp stable channel on that date.
- Compile `Mapsui.Rendering.Skia` and the experimental renderer directly against SkiaSharp 4.
- Upgrade the complete SkiaSharp/HarfBuzz/native-assets family together.
- Upgrade or replace dependent rendering packages where necessary.
- Migrate remaining obsolete APIs, including path construction APIs where appropriate.
- Establish the intended net10-era Mapsui target framework matrix.
- Release as a Mapsui major version if public target frameworks or compatibility guarantees are removed.

## Non-goals of the primary follow-up PR

- Compiling the shared Mapsui renderer directly against SkiaSharp 4 before Avalonia supports it.
- Shipping a stable Mapsui release on a SkiaSharp preview.
- Redesigning Mapsui's complete CI versioning system.
- Resolving all nullable debt in the experimental vector-tile renderer.
- Combining unrelated behavioral fixes with dependency maintenance.
