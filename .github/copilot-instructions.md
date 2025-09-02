# Copilot Instructions for Mapsui

## Repository Overview

Mapsui (pronounced "map-su-wii") is a cross-platform mapping library for .NET applications. It provides map components for apps built with MAUI, WPF, Avalonia, Uno Platform, Blazor, WinUI, Windows Forms, Eto, .NET Android, and .NET iOS. The library uses SkiaSharp for high-performance 2D graphics rendering and supports various map data sources including tile-based services and vector data.

## High-Level Repository Information

- **Project Type**: Cross-platform .NET mapping library
- **Languages**: C# 13.0 with .NET 9/.NET 8 multi-targeting
- **Size**: ~75 projects across multiple UI frameworks
- **Main Dependencies**: SkiaSharp (rendering), NetTopologySuite (geometries), BruTile (tiles)
- **Architecture**: Core library with platform-specific UI implementations
- **Package Management**: Central Package Management enabled via Directory.Packages.props

## Build and Validation Instructions

### Prerequisites

**Required:**
- .NET 9.0.301 SDK (exact version specified in global.json)
- Android workload: `dotnet workload install android`
- MAUI Android workload: `dotnet workload install maui-android`

**Installation Command:**
```bash
# Install .NET 9.0.301 if not available
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 9.0.301

# Install required workloads
dotnet workload install android maui-android
```

### Build Process

**Always restore before building.** Restore takes ~70 seconds, build takes 5+ minutes.

**Linux/Cross-platform build:**
```bash
dotnet restore Mapsui.Linux.slnf     # ~70 seconds
dotnet build --no-restore --configuration Debug Mapsui.Linux.slnf  # ~5 minutes
```

**Platform-specific builds use solution filter files (.slnf):**
- `Mapsui.Linux.slnf` - Linux-compatible projects (recommended for cross-platform development)
- `Mapsui.slnx` - Full solution (Windows, used in CI)
- `Mapsui.Tests.slnf` - Test projects only
- UI-specific filters: `Mapsui.Maui.slnf`, `Mapsui.Blazor.slnf`, `Mapsui.Wpf.slnf`, etc.

**Note**: Use `Mapsui.Linux.slnf` for development on non-Windows systems, `Mapsui.slnx` for full Windows builds.

**Release builds:**
```bash
dotnet pack --configuration Release Mapsui/Mapsui.csproj -o Artifacts -p:PackageVersion=$(git describe --tags)
```

### Testing

**Core test projects:**
```bash
# Run after successful build
dotnet test Tests/Mapsui.Tests/bin/Debug/net9.0/Mapsui.Tests.dll --blame-hang-timeout:60s
dotnet test Tests/Mapsui.Nts.Tests/bin/Debug/net9.0/Mapsui.Nts.Tests.dll --blame-hang-timeout:60s
dotnet test Tests/Mapsui.Rendering.Skia.Tests/bin/Debug/net9.0/Mapsui.Rendering.Skia.Tests.dll --blame-hang-timeout:60s
```

**Rendering tests are critical** - they compare generated images pixel-by-pixel against reference images. Failed rendering tests require manual inspection to determine if changes are intentional.

### Code Formatting

**Always run formatting before submitting:**
```bash
# Format whitespace and style (use full solution for consistency with CI)
dotnet format whitespace Mapsui.slnx --verbosity normal --verify-no-changes
dotnet format style Mapsui.slnx --verbosity normal --verify-no-changes
# Note: Analyzer formatting is not enforced yet (work in progress)
```

Code must comply with .editorconfig rules. Use `--verify-no-changes` in CI/CD to prevent formatting violations. **Note**: Existing codebase has formatting issues; new code should follow standards.

### Common Build Issues

1. **Missing workloads**: Error NETSDK1147 indicates missing Android/MAUI workloads
2. **SDK version mismatch**: Must use exact .NET 9.0.301 version (see global.json)
3. **Long restore times**: First restore downloads many packages (~10GB), use caching
4. **Memory issues**: Large solution may require increased memory limits

## Project Layout and Architecture

### Core Architecture

**Layered architecture:**
- `Mapsui/` - Core mapping engine (platform-agnostic)
- `Mapsui.Rendering.Skia/` - SkiaSharp-based rendering
- `Mapsui.Tiling/` - Tile management and caching
- `Mapsui.Nts/` - NetTopologySuite integration for geometries
- `Mapsui.ArcGIS/` - ArcGIS service support
- `Mapsui.Extensions/` - Additional utilities

**UI Framework Implementations:**
- `Mapsui.UI.*/` - Platform-specific map controls
- `Samples/Mapsui.Samples.*/` - Example applications per platform

### Configuration Files

- `global.json` - .NET SDK version pinning
- `Directory.Build.props` - MSBuild properties for all projects
- `Directory.Packages.props` - Central package version management
- `.editorconfig` - Code style rules
- `Nuget.config` - NuGet source configuration with local artifacts support

### Continuous Integration

**GitHub Actions workflows:**
- `.github/workflows/dotnet.yml` - Main build (Linux, Windows, Mac)
- `.github/workflows/dotnet-docs.yml` - Documentation build
- `.github/workflows/dotnet-release-nugets.yml` - Release automation

**Build validation includes:**
- Multi-platform compilation
- Unit test execution
- Code formatting verification
- Package creation and validation

### Key Development Guidelines

1. **All checks must be green**: Compilation, tests, and samples must work
2. **Code formatting**: Use dotnet format and comply with .editorconfig
3. **Small PRs**: Keep changes focused on single topics
4. **Rendering tests**: Manually verify image changes when rendering tests fail
5. **Extension methods**: Place in `Extensions/` folders with specific naming conventions

### Rendering Test Workflow

When rendering tests fail:
1. **Visual inspection required** - Compare generated vs reference images
2. **Generated images location**: `Tests/Mapsui.Rendering.Skia.Tests/bin/Debug/net9.0/Resources/Images/GeneratedTest/`
3. **Reference images location**: `Tests/Mapsui.Rendering.Skia.Tests/Resources/Images/OriginalTest/`
4. **Update references**: Use `Scripts/CopyGeneratedImagesOverOriginalImages.ps1` if changes are intentional
5. **Commit image updates**: Only commit reference images that correspond to failed tests

**Key commands for rendering tests:**
```powershell
# Copy generated images to replace originals (run from solution root)
.\Scripts\CopyGeneratedImagesOverOriginalImages.ps1
```

### Performance Considerations

- **Restore caching**: Cache ~/.nuget/packages (exclude Mapsui.* packages)
- **Build parallelization**: Solution supports parallel builds
- **Memory usage**: Large solution may require >8GB RAM for full builds
- **Android emulator**: Not required for basic builds, skip with --skip androidemulator

### Trust These Instructions

These instructions are comprehensive and tested. Only search for additional information if:
- Instructions are incomplete for your specific scenario
- You encounter errors not covered in common issues
- Working with untested platforms or configurations

The repository is actively maintained with a strong CI/CD pipeline, so following these instructions should result in successful builds and tests.