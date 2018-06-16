# Mapsui Components

If you install the NuGet package into your app these assemblies are added:

- Mapsui.UI.{platform} - Platform specific UI. Contains the MapControl
- Mapsui.Rendering.Skia \*
- Mapsui \*
- Mapsui.Geometries \*
- On Windows desktop Mapsui.Rendering.Xaml is added.
- A number of nuget dependencies

\* A PCL with Profile111. Targets: .Net Framework 4.5, ASP.NET Core 5.0, Windows 8, Windows Phone 8.1, Xamarin.Android, Xamarin.iOS, Xamarin.iOS (Classic)

## Mapsui Parts

Mapsui consists of 3 major object.

### Map 

Holds all information about the map like projection, layers, widgets and so on.

### Viewport

Holds all information about the part of the map, that is visible on the screen like center, extent, rotation, resolution and so on. It is created automatically when Map is created. You could access it Map.Viewport.

#### Properties
##### Center
Center of the visible map as Mapsui.Geometries.Point
##### Resolution
Resolution of the visible part of the map. With this, you could zoom in and out.
##### Rotation
Rotation of the visible part of the map in degrees
##### IsRotated
Flag, if the map visible map is oriented to north
##### Extent
Extent of the visible map as Mapsui.Geometries.BoundingBox

#### Methods
##### ScreenToWorld
Convert viewport coordinates to world coordinates
##### WorldToScreen
Convert world coordinates to viewport coordinates

#### Events
##### ViewportChanged
Event, we is risen, if anything changes in the viewport

### MapControl

Holds all information to display the viewport on the screen and to react on user actions like touches and taps. It holds a instance of the renderer too.
