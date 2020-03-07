# Custom Style Renderers

## Summary

As of 2.0.0-beta.37 Mapsui supports Custom Style Renders. This means a user can create a *custom style* and associate this with a *custom style renderer* to allow full freedom in rendering a feature the way the user would like.

## How it works
- Create a custom style by deriving a class from IStyle. 
- Assign that style to an ILayer.Style or IFeature.Styles.
- Create a custom renderer by deriving a class from ISkiaStyleRenderer and implement the Draw method.
- Register the association of the *custom style* to the *custom style renderer* at the MapRenderer like this:
  - ```mapControl.Renderer.StyleRenderers.Add(typeof(CustomStyle), new SkiaCustomStyleRenderer());```.

This the ISkiaStyleRenderer interface:
```csharp
    public interface ISkiaStyleRenderer : IStyleRenderer
    {
        bool Draw(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IFeature feature, IStyle style, ISymbolCache symbolCache);
    }
```
The IFeature has a Geometry field. The renderer is responsible to cast that geometry to the type it intends to render. The IStyle is the custom style the user defined. It could contain extra style information not present in the default style classes. 

## Code sample
Look in the Mapsui source code for CustomStyleSample.cs. 

## Remarks
Note, that the renderer depends on the rendering implementation. Currently we still support XAML as renderer but this may be removed in the future. We expect to support Skia for a long time but it may be replaced at some point. There are no such plans just yet.
