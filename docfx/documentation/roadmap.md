# Roadmap 
*Updated November 2023*

## Mapsui 4

Branch: [develop/4.1](https://github.com/Mapsui/Mapsui/tree/develop/4.1). Milestone: [v4.1](https://github.com/Mapsui/Mapsui/milestone/9). Mapsui v4.1.x is the stable version you should use.

- [x] Use NTS for geometries
- [x] License to MIT
- [x] Improve Viewport logic
- [x] Improve MVVM support [#1731](https://github.com/Mapsui/Mapsui/issues/1731)
- [x] Improve samples
- [x] Add MAUI MapControl
- [x] Add Blazor MapControl
- [x] Add Uno Platform MapControl
- [x] Add Avalonia MapControl
- [x] Add Eto MapControl
- [x] Add 'getting started' tutorials for all platforms
- [ ] Keep fixing bugs

## Mapsui 5

Branch: [master](https://github.com/Mapsui/Mapsui/tree/master). Milestone: [v5.0](https://github.com/Mapsui/Mapsui/milestone/10). We are working on a series of preview releases.

- [ ] Everything to .NET 6
- [ ] Cleanup:
  - [ ] Use `<Nullable>enable</Nullable>` everywhere (Add to Directory.Build.props) and go through the current suppressions to see if there is a better way.
  - [ ] Fix the remaining warnings wrt IDispose.
  - [ ] Run dotnet format (whitespace, style and analyzers) on the entire solution.
  - [x] Always propagate async back to the caller.
  - [ ] Remove nuget packages that we previously needed for things now supported in .NET 6. We may also have some code copies for things now in .NET 6.
  - [ ] Remove the #if defines we do not need anymore.
  - [ ] Remove all scripts and configurations we do not use anymore.
  - [ ] Simplify the build scripts now that we do not need the workarounds.
- [ ] Remove older frameworks:
  - [ ] Remove Xamarin.Forms (but not Mapsui.MAUI, Mapsui.iOS and Mapsui.Android).
  - [x] Remove Uno UWP (but not Mapsui.Uno.WinUI)
  - [x] Remove Avalonia.V0 (but not Avalonia)
- [ ] Merge MapView functionality into MapControl. We need to work this out in more detail.

## Mapsui 6 and later

This is mostly speculation. You could browse through the ['design/roadmap' tags](https://github.com/Mapsui/Mapsui/labels/design%2Froadmap) to get an impression of the options.

- [ ] Two step rendering. In the draw loop only draw skia object, create skia object in an earlier step https://github.com/Mapsui/Mapsui/issues/1448
- [ ] World wrap https://github.com/Mapsui/Mapsui/issues/518
- [ ] Add vector tiles https://github.com/Mapsui/Mapsui/issues/1478
- [ ] All redering through a single pipeline https://github.com/Mapsui/Mapsui/issues/2269
- [ ] Add support for GeoParquet https://github.com/Mapsui/Mapsui/issues/2282
