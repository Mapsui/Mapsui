# Experimental Renderer

`Mapsui.Experimental.Rendering.Skia` is a rewrite of the Skia-based renderer. See [experimental-packages.md](experimental-packages.md) for how to switch to it.

## Partial Rendering

By default the map redraws the entire screen on every refresh. Partial rendering means only a small part of the screen is redrawn — the area that actually changed. This saves work, which matters especially for apps that update the map often, like a GPS tracking app where the location marker moves every second.

### How to trigger a partial refresh

Call `RefreshGraphics` with a rectangle in world coordinates (the same coordinate system your map uses):

```cs
map.RefreshGraphics(new MRect(minX, minY, maxX, maxY));
```

Without a rectangle, `RefreshGraphics()` redraws the full screen as normal.

### MyLocationLayer

`MyLocationLayer` uses partial rendering automatically. When the location or heading changes, it calculates the small area around the old and new position and only redraws that. You don't need to do anything extra to get this benefit.

### Custom widget renderers

If you write a custom widget renderer by implementing `ISkiaWidgetRenderer`, the `Draw` method receives a `SKRect? dirtyScreenRect` parameter. This tells you which part of the screen is being redrawn (in screen pixels). A `null` value means the full screen is being redrawn.

The canvas is already clipped to the dirty area, so drawing outside it has no effect. But if your widget does expensive work before drawing — like fetching data or building paths — you can use this parameter to skip that work when the widget is outside the refreshed area:

```cs
public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget,
                 RenderService renderService, float layerOpacity,
                 SKRect? dirtyScreenRect)
{
    // Skip expensive work if this widget is outside the area being redrawn.
    if (dirtyScreenRect.HasValue && widget.Envelope != null)
    {
        var e = widget.Envelope;
        var d = dirtyScreenRect.Value;
        if (e.MaxX < d.Left || e.MinX > d.Right || e.MaxY < d.Top || e.MinY > d.Bottom)
            return;
    }
    // ... draw normally
}
```

### Layer data fetching

When a partial refresh is triggered, the renderer also passes the dirty rectangle as the spatial query to each layer's `GetFeatures` call. This means layers that support spatial queries will only process features that fall inside the refreshed area, saving more work.

If your layer always returns all features regardless of the query extent, you still get the benefit of the canvas clip, but not the feature-query savings.
