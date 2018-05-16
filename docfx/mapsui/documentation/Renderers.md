# Renderers

Mapsui has two renderers:
- **Xaml** in the Mapsui.Rendering.Xaml assembly. Used only in WPF. Older and stable. 
- **Skia** in the Mapsui.Rendering.Skia assembly. Used in all supported platforms. Newer.

## Differences Between Xaml and Skia
Skia is nearly equivalent to WPF but there are still some differences (This list has grown shorter over last year):
1. There is a difference in the way halo symbolization is implemented. We will match WPF to Skia (not the other way around). We have used the right technique for labels.
1. Skia does not have the option to displayed symbols in world units. The means the icon will grow bigger if you zoom in. This is unlike regular icon but like regular polgons. This is an exotic feature, you will probably not need this. 
1. There are probably some more differences I missed.

## Xaml and Skia side by side in Mapsui.Samples.Wpf
The project Mapsui.Samples.Wpf has a dropdown on the top left to select between skia and WPF rendering. This is allows you to compare them.
