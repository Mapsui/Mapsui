# Renderers

Mapsui has two renderers:
- **Xaml** in the Mapsui.Rendering.Xaml assembly. Used only in WPF. Stable and most complete. 
- **WPF** in the Mapsui.Rendering.Skia assembly. Used in all supported platforms. Newer.

## Differences
Skia is nearly equivalent to WPF but there are some differences:
1. There is a difference in the way halo symbolization is implemented. 
1. Skia vector rendering is somewhat faster
1. Skia raster rendering is somewhat blockier (higher quality is available in skia but slower in the software renderer currently used). 
1. Skia does not have the option to displayed symbols in world units (scale with the map). This is an exotic feature.
1. Wpf supports multiline labels. Skia does not.
1. There are probably some more differences I missed.

## Check it out
The project Mapsui.Samples.Wpf has a dropdown on the top left to select between skia and WPF rendering.