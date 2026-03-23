# Custom Layer Renderer

Gives you complete control over how an entire layer is drawn. You receive the canvas, viewport, and layer — draw whatever you want. All coordinate transforms are your responsibility.

## Quick start

**1. Set `CustomLayerRendererName` on the layer:**
```csharp
var layer = new MemoryLayer("My Layer")
{
    Features = ...,
    CustomLayerRendererName = "my-layer-renderer"
};
```

**2. Register your rendering function:**
```csharp
MapRenderer.RegisterLayerRenderer("my-layer-renderer", Render);
```

**3. Implement the function:**
```csharp
void Render(SKCanvas canvas, Viewport viewport, ILayer layer, RenderService renderService)
{
    foreach (var feature in layer.GetFeatures(viewport.ToExtent(), viewport.Resolution))
    {
        var point = ((PointFeature)feature).Point;
        var (sx, sy) = viewport.WorldToScreenXY(point.X, point.Y);
        using var paint = new SKPaint { Color = SKColors.Purple, IsAntialias = true };
        canvas.DrawCircle((float)sx, (float)sy, 8f, paint);
    }
}
```

## Sample

See the [CustomLayerRenderer sample](https://mapsui.com/v5/samples/#/CustomLayerRenderer/CustomLayerRenderer) for a working example.

## Additional notes

- You can reuse `PointStyleRenderer.DrawPointStyle(...)` inside your layer renderer to get the same transform behaviour as the [Custom Point Style Renderer](custom-point-style-renderer.md).
- Any layer `Style` set on the layer is ignored by the custom renderer — your function is called instead. Set it to `null` or a placeholder.
- If you use the experimental renderer, also register with `Mapsui.Experimental.Rendering.Skia.MapRenderer.RegisterLayerRenderer(...)`.
