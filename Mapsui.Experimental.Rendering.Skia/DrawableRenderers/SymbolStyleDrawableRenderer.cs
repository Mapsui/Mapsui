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
/// Creates and draws SymbolStyle drawables using the two-step architecture.
/// Step 1 (CreateDrawables): Creates SKPath and SKPaint objects on a background thread.
/// Step 2 (DrawDrawable): Applies viewport transform and draws to the canvas on the UI thread.
/// Reuses the static helper methods from <see cref="SymbolStyleRenderer"/> for creating Skia objects.
/// </summary>
public class SymbolStyleDrawableRenderer : IDrawableStyleRenderer
{
    public IReadOnlyList<IDrawable> CreateDrawables(Viewport viewport, ILayer layer, IFeature feature,
        IStyle style, RenderService renderService)
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

        return drawables;
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

    public void DrawDrawable(object canvas, Viewport viewport, IDrawable drawable, ILayer layer)
    {
        if (canvas is not SKCanvas skCanvas)
            throw new ArgumentException($"Expected {nameof(SKCanvas)} but got {canvas?.GetType().Name}");

        if (drawable is not SymbolStyleDrawable symbolDrawable)
            throw new ArgumentException($"Expected {nameof(SymbolStyleDrawable)} but got {drawable?.GetType().Name}");

        DrawSymbolDrawable(skCanvas, viewport, symbolDrawable);
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
