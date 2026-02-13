using Mapsui.Experimental.Rendering.Skia.Drawables;
using Mapsui.Experimental.Rendering.Skia.Extensions;
using Mapsui.Experimental.Rendering.Skia.SkiaStyles;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Rendering;
using Mapsui.Styles;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Mapsui.Experimental.Rendering.Skia.DrawableRenderers;

/// <summary>
/// Two-step renderer for SymbolStyle. No cache interaction inside the renderer.
/// <list type="bullet">
///   <item><description><see cref="CreateDrawable"/>: Creates SKPath and SKPaint objects
///         (expensive, runs on background thread).</description></item>
///   <item><description><see cref="DrawDrawable"/>: Applies viewport transform and draws
///         (fast, render thread).</description></item>
/// </list>
/// Does NOT implement <c>ISkiaStyleRenderer</c> — features that haven't been prepared yet
/// simply won't render until the background thread catches up.
/// </summary>
public class TwoStepSymbolStyleRenderer : ITwoStepStyleRenderer
{
    /// <inheritdoc />
    public IDrawableCache CreateCache() => new DrawableCache();

    /// <inheritdoc />
#pragma warning disable IDISP015 // Member should not return created and cached instance - ownership transfers to cache
    public IDrawable? CreateDrawable(Viewport viewport, ILayer layer, IFeature feature,
        IStyle style, RenderService renderService)
#pragma warning restore IDISP015
    {
        if (style is not SymbolStyle symbolStyle)
            throw new ArgumentException($"Expected {nameof(SymbolStyle)} but got {style?.GetType().Name}");

        var drawables = new List<IDrawable>();
        var opacity = (float)(layer.Opacity * symbolStyle.Opacity);

        feature.CoordinateVisitor((x, y, setter) =>
        {
            var drawable = CreateSymbolDrawable(x, y, symbolStyle, opacity);
            drawables.Add(drawable);
        });

        return drawables.Count switch
        {
            0 => null,
            1 => drawables[0],
            _ => new CompositeDrawable(drawables)
        };
    }

    /// <inheritdoc />
    public void DrawDrawable(object canvas, Viewport viewport, IDrawable drawable, ILayer layer)
    {
        if (canvas is not SKCanvas skCanvas)
            return;

        switch (drawable)
        {
            case SymbolStyleDrawable symbolDrawable:
                DrawSymbolDrawable(skCanvas, viewport, symbolDrawable);
                break;
            case CompositeDrawable composite:
                foreach (var child in composite.Children)
                    DrawDrawable(canvas, viewport, child, layer);
                break;
        }
    }

    internal static SymbolStyleDrawable CreateSymbolDrawable(double worldX, double worldY,
        SymbolStyle symbolStyle, float opacity)
    {
        // Create the path (heavy work - done on background thread)
        var path = SymbolStyleRenderer.CreatePath(symbolStyle.SymbolType);

        // Create fill paint if visible
        SKPaint? fillPaint = null;
        if (symbolStyle.Fill.IsVisible())
        {
            fillPaint = SymbolStyleRenderer.CreateFillPaint((symbolStyle.Fill!, opacity));
        }

        // Create outline paint if visible
        SKPaint? outlinePaint = null;
        if (symbolStyle.Outline.IsVisible())
        {
            outlinePaint = SymbolStyleRenderer.CreateLinePaint((symbolStyle.Outline!, opacity));
        }

        // Resolve relative offset
        var relativeOffset = symbolStyle.RelativeOffset.GetAbsoluteOffset(
            SymbolStyle.DefaultWidth, SymbolStyle.DefaultWidth);

        return new SymbolStyleDrawable(
            worldX: worldX,
            worldY: worldY,
            path: path,
            fillPaint: fillPaint,
            outlinePaint: outlinePaint,
            symbolScale: (float)symbolStyle.SymbolScale,
            symbolRotation: symbolStyle.SymbolRotation,
            rotateWithMap: symbolStyle.RotateWithMap,
            offsetX: (float)symbolStyle.Offset.X,
            offsetY: (float)-symbolStyle.Offset.Y,
            relativeOffsetX: (float)relativeOffset.X,
            relativeOffsetY: (float)-relativeOffset.Y,
            opacity: opacity
        );
    }

    internal static void DrawSymbolDrawable(SKCanvas canvas, Viewport viewport, SymbolStyleDrawable drawable)
    {
        canvas.Save();
        try
        {
            // 1. Transform world coordinates to screen coordinates
            var (screenX, screenY) = viewport.WorldToScreenXY(drawable.WorldX, drawable.WorldY);
            canvas.Translate((float)screenX, (float)screenY);

            // 2. Apply symbol scale
            canvas.Scale(drawable.SymbolScale, drawable.SymbolScale);

            // 3. Apply rotation
            var rotation = drawable.SymbolRotation;
            if (drawable.RotateWithMap)
                rotation += viewport.Rotation;
            if (rotation != 0)
                canvas.RotateDegrees((float)rotation);

            // 4. Apply absolute offset
            canvas.Translate(drawable.OffsetX, drawable.OffsetY);

            // 5. Apply relative offset
            canvas.Translate(drawable.RelativeOffsetX, drawable.RelativeOffsetY);

            // 6. Draw - this is the fast part
            if (drawable.FillPaint is not null)
                canvas.DrawPath(drawable.Path, drawable.FillPaint);

            if (drawable.OutlinePaint is not null)
                canvas.DrawPath(drawable.Path, drawable.OutlinePaint);
        }
        finally
        {
            canvas.Restore();
        }
    }
}
