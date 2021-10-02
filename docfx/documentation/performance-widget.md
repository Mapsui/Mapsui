# PerformanceWidget

## Summary

Mapsui 3.0 supports a widget, that could show the main performance values for drawing the map.

## How it works

1) Create a new Performance object for the MapControl, where the values could be stored

```csharp
if (mapControl.Performance == null)
    mapControl.Performance = new Utilities.Performance(10);
```

2) Create the PerformanceWidget. As parameter you have to provide the Performance object, that the widget should be use

```csharp
var widget = new Widgets.Performance.PerformanceWidget(mapControl.Performance);
```

3) If you want to clear all values of the Performance object, then add the following event handler for the touch event of the widget

```csharp
widget.WidgetTouched += (sender, args) =>
{
    mapControl?.Performance.Clear();
    mapControl?.RefreshGraphics();

    args.Handled = true;
};
```

4) Add the widget to the list of known widgets

```csharp
mapControl.Map.Widgets.Add(widget);
```

5) To draw the widget on the screen, we need a widget renderer. To use the default widget renderer, use the following lines

```csharp
mapControl.Renderer.WidgetRenders[typeof(Widgets.Performance.PerformanceWidget)] = new Rendering.Skia.SkiaWidgets.PerformanceWidgetRenderer(10, 10, 12, SkiaSharp.SKColors.Black, SkiaSharp.SKColors.White);
```

The first two parameters are the X and Y coordiantes for the widget. Third parameter is the text size. Fourth is the text color and fifth is the background color.
## Code copy

```csharp
if (mapControl.Performance == null)
    mapControl. Performance = new Utilities.Performance();

var widget = new Widgets.Performance.PerformanceWidget(mapControl.Performance);

widget.WidgetTouched += (sender, args) =>
{
    mapControl?.Performance.Clear();
    mapControl?.RefreshGraphics();

    args.Handled = true;
};

mapControl.Map.Widgets.Add(widget);
mapControl.Renderer.WidgetRenders[typeof(Widgets.Performance.PerformanceWidget)] = new Rendering.Skia.SkiaWidgets.PerformanceWidgetRenderer(10, 10, 12, SkiaSharp.SKColors.Black, SkiaSharp.SKColors.White);
```

## Values

### Last

Time for drawing of the last screen. Be careful: because the widget is drawn together with the screen, this time is the time for the screen drawn before the screen you see.

### Mean

The mean value is the mean of the last x draws. x is the number, you provide when creating the Performance object.

### Frames per second

This is the number of frames that could be drawn with the actual mean drawing time.

### Minimum

Fastest draw of the screen.

### Maximum

Slowest draw of the screen.

### Count

How often the screen is drawn.

### Dropped

How often the screen isn't invalidated, because a drawing is still in progress.

## Remarks

The Performance object contains the times between start and end of a drawing process. This must not be the real drawing time. It could be, that other tasks running in between the drawing process.
