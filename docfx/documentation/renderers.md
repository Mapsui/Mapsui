# Renderers

Mapsui has two renderers:
- **Xaml** in the Mapsui.Rendering.Xaml assembly. Used only in the WPF MapControl. Older and stable. 
- **Skia** in the Mapsui.Rendering.Skia assembly. Used in all supported platforms. Newer.

## Differences Between Xaml and Skia
Skia is nearly equivalent to Xaml but there are still some differences (This list has grown shorter over last year):
1. There is a difference in the way halo symbolization is implemented. We will match Xaml to Skia (not the other way around). We already use the correct technique for Xaml halos for labels styles.
1. Skia does not have the option to displayed symbols in world units. The means the icon will grow bigger if you zoom in. This is unlike regular icon but like regular polgons. This is an exotic feature, you will probably not need this. 
1. There are probably some more differences I missed.

## Xaml and Skia side by side in Mapsui.Samples.Wpf
The project Mapsui.Samples.Wpf has a dropdown on the top left to select between Skia and Wpf rendering. This is allows you to compare them.

## Renderer abstraction layer 

There is the question how much of the renderer should be abstracted away. An abstraction layer add extra complexity. Since it looks more and more like skiasharp will be the only renderer for as far as we can see we could just add that dependency to all our code and work directly with SkiaSharp classes, that may simplify some things. However, in the past we had to switch many times to different renderers, so I am not so sure if this won't happen again. So let's not settle for one renderer just yet. 

For context, these are the renderers Mapsui had in the past:
- System.Drawing
- System.Drawing for PocketPC
- Silverlight XAML
- WPF XAML
- UWP XAML (could later be merged with WPF XAML)
- iOS native rendering
- Android native rendering (this is actually internally using skia)
- OpenTK (this was not mature enough at that point)
- SkiaSharp
