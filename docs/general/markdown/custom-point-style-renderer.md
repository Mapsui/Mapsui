# Custom Point Style Renderer

Renders individual point features with your own drawing code. The framework handles positioning, scaling, and rotation — you draw at (0, 0) and the symbol appears at the correct map location.

## Quick start

**1. Assign a `CustomPointStyle` to a layer:**
```csharp
layer.Style = new CustomPointStyle { RendererName = "my-renderer" };
```

**2. Register your drawing function:**
```csharp
MapRenderer.RegisterPointStyleRenderer("my-renderer", Draw);
```

**3. Implement the drawing function — draw at (0, 0):**
```csharp
void Draw(SKCanvas canvas, IPointStyle style, RenderService renderService, float opacity)
{
    using var paint = new SKPaint { Color = SKColors.Blue, IsAntialias = true };
    canvas.DrawCircle(0f, 0f, 10f, paint);
}
```

By the time `Draw` is called the canvas has already been translated to the screen position and transformed for `SymbolScale`, `SymbolRotation`, and `Offset`.

## Sample

See the [CustomPointStyle samples](https://mapsui.com/v5/samples/#/CustomPointStyle/Basic) for working examples (Basic, Advanced, Shader).

## Additional notes

- `RelativeOffset` is **not** applied automatically because the symbol's pixel size is not known by the framework. If you use it, apply it yourself: `style.RelativeOffset.GetAbsoluteOffset(width, height)`.

## Experimental renderer differences

When using the experimental renderer, also register with `Mapsui.Experimental.Rendering.Skia.MapRenderer.RegisterPointStyleRenderer(...)`.

The experimental `PointStyleRenderer.DrawPointStyle` has an additional `IFeature feature` parameter compared to the regular renderer:

```csharp
// Regular renderer
PointStyleRenderer.DrawPointStyle(canvas, viewport, x, y, style, renderService, opacity, renderHandler);

// Experimental renderer
PointStyleRenderer.DrawPointStyle(canvas, viewport, x, y, style, feature, renderService, opacity, renderHandler);
```

The `RenderHandler` delegate signature is the same in both — this only matters if you call `DrawPointStyle` directly (e.g. from a custom layer renderer).
