# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Common Commands

### Build
```bash
# Restore packages and build the entire solution in Debug configuration
# (the Release configuration is identical except for the --configuration flag)
#
# Usage:
#   dotnet restore && dotnet build Mapsui.slnx --configuration Debug
#   dotnet restore && dotnet build Mapsui.slnx --configuration Release
```

### Lint / Format
The repository enforces a compact style via `dotnet format` and `.editorconfig`.
```bash
# Check for style violations (exits non‑zero if any exist)
#
#   dotnet format --check
#
# Apply automatic fixes (may modify the repository – run after a successful test run)
#
#   dotnet format
```

### Run Tests
All unit and regression tests live under `Tests/`.
```bash
# Run the entire test suite (all projects in the solution)
#
#   dotnet test
```

#### Run a single test project
```bash
# Example – run only the rendering regression tests
#
#   dotnet test Tests/Mapsui.Rendering.Skia.Tests
```

#### Run a single test method or class
```bash
# Use the fully‑qualified name (including namespace)
# Example: run the CalloutSample test class in the rendering regression suite
#
#   dotnet test Tests/Mapsui.Rendering.Skia.Tests --filter "FullyQualifiedName~CalloutSample"
#
# To target a specific method:
#   dotnet test Tests/Mapsui.Rendering.Skia.Tests --filter "FullyQualifiedName~CalloutSample.TestMethod"
```

#### Rendering regression tests
These tests render every sample and compare the output pixel‑by‑pixel against stored reference images.

- **Run a single sample** (fastest for targeted changes):
  ```bash
  dotnet test Tests/Mapsui.Rendering.Skia.Tests --filter "FullyQualifiedName~CalloutSample"
  ```
- **Run all regression tests**:
  ```bash
  dotnet test Tests/Mapsui.Rendering.Skia.Tests --filter "TestSampleAsync"
  ```
- **Interpret results**:
  - `Passed` – output matches the reference image.
  - `Inconclusive` – no reference image yet; the test generated one in `GeneratedRegression/`.
  - `Failed` – pixel difference exceeded the threshold. Compare
    - Generated: `Tests/Mapsui.Rendering.Skia.Tests/bin/Debug/net9.0/Resources/Images/GeneratedRegression/`
    - Reference: `Tests/Mapsui.Rendering.Skia.Tests/bin/Debug/net9.0/Resources/Images/OriginalRegression/`
- **Update reference images** after intentional rendering changes:
  ```bash
  .\Scripts\CopyGeneratedImagesOverOriginalImages.ps1
  ```
  (Revert any unrelated binary files afterward.)

## High‑Level Architecture

- **Core library (`Mapsui`)** – Implements map rendering, layers, providers, styles, utilities, and the `Map` abstraction. Targets .NET 9 and is platform‑agnostic.
- **UI wrappers** – Separate projects for each UI framework (e.g., `Mapsui.UI.Maui`, `Mapsui.UI.Wpf`, `Mapsui.UI.Blazor`). Each exposes a `MapControl` that hosts a `Map` instance.
- **Sample projects** – Under `Samples`. They reference the appropriate UI wrapper and demonstrate usage. They are built only for demonstration and are not required for library consumption.
- **Source generator (`Mapsui.Sample.SourceGenerator`)** – Scans all classes that implement `ISample`, `ISampleBase`, `ISampleTest`, or `IMapViewSample` at build time and generates a `Samples.Register()` method. Adding a new sample class that implements one of these interfaces is sufficient – no manual registration is needed.
- **Tests** – Unit tests in `Tests/`. Rendering regression tests are in `Tests/Mapsui.Rendering.Skia.Tests`.
- **CI** – GitHub Actions defined in `.github/workflows/dotnet.yml` perform lint, build, and test steps for each push/PR.

## Build Configuration

- Solution file: `Mapsui.slnx` (used by Visual Studio and the `dotnet` CLI).
- Projects target .NET 9 (`net9.0`).
- Packages are published via `dotnet pack` and `dotnet nuget push` (not part of the daily dev workflow).

## Style and Quality

- `.editorconfig` and `dotnet format` enforce a compact style (use `var`, optional braces, expression‑bodied members, etc.).
- Copilot instructions are captured in `.github/copilot‑instructions.md` – follow those guidelines when writing or refactoring code.
- Tests should be deterministic, fast, and avoid external dependencies unless mocked.

## Documentation

- Docs are under `docs/` and built with DocFX.
- Public APIs are documented with XML comments; keep them up‑to‑date when changing surface types.

---

This CLAUDE.md is generated automatically; future instances of Claude Code should refer to it for consistent workflow guidance.
