# Renderers

As of v4 Mapsui has only one renderer, Skia. We still use a renderer interface but there are no plans to add another renderer atm.

## Why use an IRenderer interface?

If you know there is only one renderer why work with an interface. Some things can be simplified if you directly work with the implementation. Since it looks more and more like skiasharp will be the only renderer for as far as we can see we could just add that dependency to all our code and work directly with SkiaSharp classes, that may simplify some things. However, in the past we had to switch many times to different renderers, so I am not so sure if this won't happen again. So let's not settle for one renderer just yet. 

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
