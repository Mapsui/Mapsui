# Roadmap 
*Updated Januari 2024*

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

Branch: [main](https://github.com/Mapsui/Mapsui/tree/main). Milestone: [v5.0](https://github.com/Mapsui/Mapsui/milestone/10). We are working on a series of preview releases.

The main objective of v5 is to reduce technical debt, remove everything that makes development slower. Support less and support it better. Release a version that is easy to maintain while working on v6.

- [x] Lowest supported version to .NET 6 (remove .netstandard)
- [ ] Cleanup:
    * [x] Rename master to main.
    * [x] Run dotnet format on the entire solution.
    * [x] Run dotnet style on the entire solution.
    * [x] Run dotnet analyze on the entire solution.
    * [x] Always propagate async back to the caller.
    * [x] Remove nuget packages that we previously needed for things now supported in .NET 6.
    * [ ] Remove code copies of things now in .NET 6 (in the past we copied some .NET Core things which were not in .NET standard).
    * [ ] Remove the #if defines we do not need anymore.
    * [ ] Remove all scripts and configurations we do not use anymore.
    * [ ] Simplify the build scripts now that we do not need the workarounds.
    * [ ] Use `<Nullable>enable</Nullable>` everywhere (Add to Directory.Build.props) and revisit all current suppressions.
    * [ ] Fix the remaining warnings wrt IDispose.
- [ ] Remove older frameworks:
    * [x] Remove Xamarin.Forms (but not Mapsui.MAUI, Mapsui.iOS and Mapsui.Android).
    * [x] Remove Uno UWP (but not Mapsui.Uno.WinUI)
    * [x] Remove Avalonia.V0 (but not Avalonia)
- [ ] Merge MapView functionality into MapControl. We need to work this out in more detail.
    * [ ] Add extension methods for Map to make it easy to add MapView functionality to the MapControl. A few:
        * [ ] AddMarkerLayer() extension which adds a layer with symbols and a callout style, which is toggled on click.
        * [ ] AddOpenStreetMapBackgroundLayer() which adds to a specific layer group and sets the Map CRS and perhaps more.
    * [ ] Make MyLocationLayer function property in Mapcontrol.
    * [ ] Add mechanism for layer grouping. [Here](https://github.com/Mapsui/Mapsui/issues/1491) is a proposal but perhaps we need something simpler.
- [ ] Make Map dispose a layer when create function is used https://github.com/Mapsui/Mapsui/issues/2284.
- [ ] Dispose the samples if needed.

## Mapsui 6 and later

There are many things we eagerly want to work on. You could browse through the ['design discussion' tags](https://github.com/Mapsui/Mapsui/labels/design%20discussion) to get an impression of the options.

- [ ] Two step rendering. In the draw loop only draw skia object, create skia object in an earlier step https://github.com/Mapsui/Mapsui/issues/1448
- [ ] World wrap https://github.com/Mapsui/Mapsui/issues/518
- [ ] Add vector tiles https://github.com/Mapsui/Mapsui/issues/1478
- [ ] All redering through a single pipeline https://github.com/Mapsui/Mapsui/issues/2269
- [ ] Add support for GeoParquet https://github.com/Mapsui/Mapsui/issues/2282
- [ ] Make the MapControl disposable not the Map https://github.com/Mapsui/Mapsui/issues/2372
- [ ] Only one type of layer. Defined in Mapsui and sealed. All DataSources can be mutable. All layers have a related DataSource, which is not a member of the layer so the layer does not need Dispose. There are different types of DataSource which require different kinds of fetching (continues fetching like in the tile layer, discrete fetching like in WMS)
- [ ] The render is responsible for performance optimizations like we use in the RasteringLayer, RasterizingTileLayer
- [ ] Centralized data projection. This should be taken into account when working on centralized data fetching and two step rendering.
