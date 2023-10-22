## Keep the renderer behind and interface

As of v4 Mapsui has only one renderer, SkiaSharp. Although we have only one renderer and have currently no plans to add others we will keep it behind an interface. There are costs to keeping the abstraction but we keep it because a change could happen again, even though it does not seem likely now. In the past we had to switch renderers many times, a list:
- System.Drawing
- System.Drawing for PocketPC
- Silverlight XAML
- WPF XAML
- UWP XAML (could later be merged with WPF XAML)
- iOS native rendering
- Android native rendering (this is actually internally using skia)
- OpenTK (this was not mature enough at that point)
- SkiaSharp
