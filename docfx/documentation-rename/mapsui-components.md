# Mapsui Components

If you install the NuGet package into your app these assemblies are added:

- Mapsui.UI.{platform} - Platform specific UI. Contains the MapControl
- Mapsui.Rendering.Skia
- Mapsui
- Mapsui.Geometries
- On Windows desktop Mapsui.Rendering.Xaml is added.
- A number of nuget dependencies

## Mapsui Parts

There are three important classes that you will deal with

### MapControl

This is the UI component that you add to you project. 

### Map 

Holds all information about the map like layers and widgets.

### Viewport

Holds all information about the part of the map that is visible on the screen like center, extent, rotation, resolution and so on. It is created automatically when Map is created. You can access it with MapControl.Map.Viewport.
