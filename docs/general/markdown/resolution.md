# Resolution

In Mapsui the resolution is a value that indicates to what degree the map is zoomed in or zoomed out. A large value means you are zoomed out (see the whole world), a small value means you are zoomed in (looking at the details). The resolution of a viewport is its size in map coordinates (of the coordinate system used) devided by its size in pixels. If you use the openstreetmap background layer (or another layer in that coordinates system) the resolution is meters / pixel at the equator.

### The resolution of the Mapsui viewport
The Viewport is an important class in Mapsui. It has a Resolution field. Its value can be directly derived from the coordinate extent and the size in pixels. So Viewport.Resolution will always be equal to Viewport.Extent.Width / Viewport.Width. The Viewport.Extent is in the units of the coordinate system (different apps can use different coordinate systems) and the Viewport.Width/Height is in pixels.

### The resolution in openstreetmap
Mapsui's resolution concept is derived from the value for zoom levels used in openstreetmap [tile schema](https://wiki.openstreetmap.org/wiki/Zoom_levels). Openstreetmap uses the EPSG:3857 coordinate system (called SphericMercator within Mapsui). The full width of that coordinate system is 40075017 units. The top level tile in openstreetmap is 256x256 pixels. So the top level tile has a resolution of 40075017 / 256 = 156543 if is shown unscaled.

### Scale in meters / pixel in openstreetmap
The coordinates of EPSG:3857 happen to be based on the circumference of the earth in meters at the equator which is [40075017 meters](https://en.wikipedia.org/wiki/Earth%27s_circumference). This means that near the equator the units of EPSG:3875 are equal to meters. It starts to deviate when moving away from the equator. Also note that other coordinate systems can have completely different unit sizes. So the relation between the coordinate system used and meters is complicated.

### I am just interested in meters / pixel and don't care about the coordinate system
That makes sense but at the moment there is no good solution for that in Mapsui. What makes this complicated:

- The meters / pixel can be different in the x and y direction because of distortion of the map projection.
- The meters / pixel can be different for different locations within a single map view.
- To implement this for a coordinate system we need projection support for that coordinate system, which we may not have. 

### Density Independent Pixels

When we talk about pixels on this page we mean density independent pixels, or dip, or dp.

Modern devices have a very high resolution. If something is drawn onto the canvas using raw pixels as coordinates the fonts would become tiny and unreadable and lines would become very thin. To correct for this a scale factor is used. Those scaled-up coordinates are called density independent pixels. Most of the time users deal only with the density independent pixels.

### Density Independent Pixels in SkiaSharp

The scale in skia has caused some confusion in the past and bugs as a consequence. So here is some extra information on this topic. This is mainly targetted at contributors, users of the Mapsui nugets do not need to know about this.

Most (all?) views in SkiaSharp use pixels as coordinates by default but for our purposes we need to use density independent pixels, so we need to correct for this. We do this by setting the scale of skia's SKCanvas. This needs to be done in the render loop because it is the only place where we have access to the SKCanvas. Also the size of the map needs to be adjusted at that point, otherwise we would draw outsize the screen. In later versions of SkiaSharp it is possible to use IgnorePixelScaling=true instead of scaling.
