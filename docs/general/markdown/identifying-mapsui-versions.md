# Identifying Mapsui Versions in Projects

This guide explains how to determine which version of Mapsui a project is using. This can be helpful when:
- Investigating compatibility issues
- Understanding version adoption across the ecosystem
- Contributing to projects that use Mapsui
- Learning from real-world Mapsui implementations

## Method Overview

.NET projects typically reference NuGet packages in one of several ways. Here's how to identify the Mapsui version for each approach:

## 1. Traditional PackageReference (In .csproj files)

For projects that specify versions directly in their `.csproj` files, look for entries like:

```xml
<PackageReference Include="Mapsui.Avalonia" Version="5.0.0" />
```

The version number is directly visible in the Version attribute.

## 2. Central Package Management (Recommended for larger projects)

Many modern .NET projects use Central Package Management (CPM), which separates package versions from package references. This is indicated by:

### In the .csproj file:
```xml
<PackageReference Include="Mapsui.Avalonia" />
```
Note: No Version attribute is present.

### In Directory.Packages.props (root of repository):
```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Mapsui.Avalonia" Version="5.0.0-rc.2" />
  </ItemGroup>
</Project>
```

The actual version is defined in the `Directory.Packages.props` file.

## 3. packages.config (Legacy .NET Framework)

For older .NET Framework projects, check for a `packages.config` file:

```xml
<packages>
  <package id="Mapsui.Wpf" version="4.1.0" targetFramework="net48" />
</packages>
```

## Real-World Example: kusto-loco Project

Let's examine [kusto-loco (LokqlDx)](https://github.com/NeilMacMullen/kusto-loco), a KQL data explorer that uses Mapsui.

### Step 1: Locate the Project File

First, we find the `.csproj` file that references Mapsui:
- File: `applications/lokqlDx/lokqlDx.csproj`

### Step 2: Check for PackageReference

In `lokqlDx.csproj`, we find:
```xml
<PackageReference Include="Mapsui.Avalonia" />
```

Note: No version is specified, indicating Central Package Management is in use.

### Step 3: Locate Directory.Packages.props

Since CPM is being used, we look in the repository root for `Directory.Packages.props`.

### Step 4: Find the Version

In `Directory.Packages.props`, we find:
```xml
<PackageVersion Include="Mapsui.Avalonia" Version="5.0.0-rc.2" />
```

**Result**: The kusto-loco project uses **Mapsui.Avalonia version 5.0.0-rc.2**.

## Quick Reference Guide

| File Location | What to Look For | What It Means |
|--------------|------------------|---------------|
| `*.csproj` | `<PackageReference Include="Mapsui.*" Version="X.X.X" />` | Direct version specification |
| `*.csproj` | `<PackageReference Include="Mapsui.*" />` (no Version) | Using Central Package Management |
| `Directory.Packages.props` | `<PackageVersion Include="Mapsui.*" Version="X.X.X" />` | Centrally managed version |
| `packages.config` | `<package id="Mapsui.*" version="X.X.X" />` | Legacy .NET Framework approach |

## Using GitHub Search

If you're investigating a GitHub repository:

1. **Search for .csproj files**: Use GitHub's code search for `Mapsui extension:csproj`
2. **Look for Directory.Packages.props**: Search for `Directory.Packages.props` or `Mapsui Version`
3. **Check the file contents**: Use GitHub's file viewer to examine the relevant files

## Tools and Automation

You can also use command-line tools to investigate local repositories:

```bash
# Find all .csproj files that reference Mapsui
find . -name "*.csproj" -exec grep -l "Mapsui" {} \;

# Search for version specifications
grep -r "Mapsui" Directory.Packages.props *.csproj

# Find Central Package Management files
find . -name "Directory.Packages.props"
```

## Understanding Version Numbers

Mapsui follows semantic versioning (SemVer):
- Format: `Major.Minor.Patch[-suffix]`
- Example: `5.0.0-rc.2`
  - Major: 5
  - Minor: 0
  - Patch: 0
  - Suffix: `-rc.2` (release candidate 2)

## See Also

- [Projects That Use Mapsui](projects-that-use-mapsui.md)
- [NuGet Package Versions](https://www.nuget.org/packages?q=mapsui)
- [Mapsui Releases](https://github.com/Mapsui/Mapsui/releases)
