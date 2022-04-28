# Resolution

In Mapsui the resolution is a value that indicates to what degree the map is zoomed in or zoomed out. A large value means you are zoomed out (see the whole world), a small value means you are zoomed in (looking at the details). The resolution of a viewport is its size in map coordinates (of the coordinate system used) devided by its size in pixels. If you use the openstreetmap background layer (or another layer in that coordinates system) the resolution is meters / pixel at the equator.

### The resolution in openstreetmap
Mapsui's resolution concept is derived from the value for zoom levels used in openstreetmaps [tile schema](https://wiki.openstreetmap.org/wiki/Zoom_levels). The tiles have a size in pixels and those correspond to the number of units in openstreetmaps coordinate system ([EPSG:3857](https://epsg.io/3857)).  A top level tile in openstreet map is 256 pixels. The coordinates of EPSG:3857 are based on the circumference of the earth in meters which is [40075017 meters](https://en.wikipedia.org/wiki/Earth%27s_circumference) at the equator. So the width of the world spans 40075017 map units. So the resolution of the top level tile is 40075017 / 256 = 156543.

It happens to be so that the units in EPSG:3875 are equal to meters at the equator, so at the equator the resolution can be interpreted as meters / pixel, but note that other coordinate systems can have completely different unit sizes. 

### The resolution of the Mapsui viewport
The Viewport is an important class in Mapsui. It has a Resolution field. Its value can be directly derived from the coordinate extent and the size in pixels. So Viewport.Resolution should will always be equal to:
```csharp
var resolution = viewport.Extent.Width / viewport.Width;
```

### I am just interested in meters / pixel and don't care about the coordinate system
That makes sense but at the moment there is no good solution for that in Mapsui. What makes this complicated:
- The meters / pixel can be different in the x and y direction because of distortion of the map projection.
- The meters / pixel can be different for different locations within a single map view.
- To implement this for a coordinate system we need projection support for that coordinate system, which we may not have. 

### What is a pixel?
When we talk about pixels on this page we mean device independent pixels or [device independent units](device-independent-units.md) 
