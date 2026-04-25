# PerformanceWidget

## Summary

Mapsui supports a widget that shows the main performance values for drawing the map.

## How it works

The `PerformanceWidget`, `Performance` object, tapped-to-clear behavior, and widget renderer are all set up automatically as part of the `Map`. Nothing needs to be created or registered manually.

By default, the widget is only shown when the debugger is attached (`ActiveMode.OnlyInDebugMode`). To show it in a released app, activate it and optionally customize its appearance:

```csharp
// The PerformanceWidget is created as part of the map.
var performanceWidget = map.Widgets.OfType<PerformanceWidget>().First();
// The default is ActiveMode.OnlyInDebugMode, which is usually the best option.
performanceWidget.Performance.IsActive = ActiveMode.Yes;
performanceWidget.BackColor = Color.FromRgba(255, 255, 32, 32);
performanceWidget.Opacity = 1;
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
