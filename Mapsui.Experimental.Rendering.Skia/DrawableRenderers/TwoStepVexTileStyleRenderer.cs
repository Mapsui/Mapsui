using Mapsui.Experimental.Rendering.Skia.SkiaStyles;
using Mapsui.Experimental.VectorTiles.Rendering;
using Mapsui.Experimental.VectorTiles.Tiling;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering;
using Mapsui.Styles;
using SkiaSharp;
using System;
using System.Collections.Generic;
using VexTile.Renderer.Mvt.AliFlux;
using VexVectorStyle = VexTile.Renderer.Mvt.AliFlux.VectorStyle;

namespace Mapsui.Experimental.Rendering.Skia.DrawableRenderers;

/// <summary>
/// Two-step renderer for VexTileStyle. The renderer itself has no cache interaction —
/// it just creates drawables and draws them. Caching is managed externally.
/// <list type="bullet">
///   <item><description><see cref="CreateDrawables"/>: Renders vector tile data to an <see cref="SKImage"/>
///         (expensive, runs on a background thread).</description></item>
///   <item><description><see cref="DrawDrawable"/>: Blits a cached tile image to the canvas (fast, render thread).</description></item>
/// </list>
/// Does NOT implement <c>ISkiaStyleRenderer</c> — tiles that haven't been prepared yet
/// simply won't render until the background thread catches up.
/// </summary>
public class TwoStepVexTileStyleRenderer : ITwoStepStyleRenderer
{
    /// <inheritdoc />
    public IDrawableCache CreateCache() => new TileDrawableCache();

    /// <inheritdoc />
    public IReadOnlyList<IDrawable> CreateDrawables(Viewport viewport, ILayer layer, IFeature feature,
        IStyle style, RenderService renderService)
    {
        if (feature is not VexTileFeature vexTileFeature)
            return [];

        if (style is not VexTileStyle vexTileStyle)
            return [];

        var extent = feature.Extent;
        if (extent is null)
            return [];

        try
        {
#pragma warning disable IDISP001 // Dispose created - ownership transferred to caller via drawable
            var image = RenderToImage(vexTileFeature, vexTileStyle.VexStyle);
#pragma warning restore IDISP001
            var opacity = (float)(layer.Opacity * style.Opacity);
            return [new VexTileStyleDrawable(image, extent, opacity)];
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, $"Error creating VexTile drawable: {ex.Message}", ex);
            return [];
        }
    }

    /// <inheritdoc />
    public void DrawDrawable(object canvas, Viewport viewport, IDrawable drawable, ILayer layer)
    {
        if (canvas is not SKCanvas skCanvas || drawable is not VexTileStyleDrawable vexDrawable)
            return;

        if (viewport.IsRotated())
        {
            skCanvas.SetMatrix(CreateRotationMatrix(viewport, vexDrawable.Extent, skCanvas.TotalMatrix));
            var destination = new SKRect(0.0f, 0.0f, (float)vexDrawable.Extent.Width, (float)vexDrawable.Extent.Height);
            BitmapRenderer.Draw(skCanvas, vexDrawable.Image, destination, vexDrawable.Opacity);
        }
        else
        {
            var destination = WorldToScreen(viewport, vexDrawable.Extent);
            BitmapRenderer.Draw(skCanvas, vexDrawable.Image, RoundToPixel(destination), vexDrawable.Opacity);
        }
    }

    /// <summary>
    /// Renders the VexTile vector data to an SKImage. This is the expensive operation
    /// that is performed on the background thread.
    /// </summary>
    private static SKImage RenderToImage(VexTileFeature feature, VexVectorStyle vectorStyle)
    {
        var skiaCanvas = new SkiaCanvas();
        VexTileRenderer.Render(feature.VectorTile, vectorStyle, skiaCanvas, feature.VexTileInfo);
        return skiaCanvas.ToSKImage();
    }

    private static SKMatrix CreateRotationMatrix(Viewport viewport, MRect rect, SKMatrix priorMatrix)
    {
        var userRotation = SKMatrix.CreateRotationDegrees((float)viewport.Rotation);
        var focalPointOffset = SKMatrix.CreateTranslation(
            (float)(rect.Left - viewport.CenterX),
            (float)(viewport.CenterY - rect.Top));
        var zoomScale = SKMatrix.CreateScale((float)(1.0 / viewport.Resolution), (float)(1.0 / viewport.Resolution));
        var centerInScreen = SKMatrix.CreateTranslation((float)(viewport.Width / 2.0), (float)(viewport.Height / 2.0));

        var matrix = SKMatrix.Concat(zoomScale, focalPointOffset);
        matrix = SKMatrix.Concat(userRotation, matrix);
        matrix = SKMatrix.Concat(centerInScreen, matrix);
        matrix = SKMatrix.Concat(priorMatrix, matrix);

        return matrix;
    }

    private static SKRect WorldToScreen(Viewport viewport, MRect rect)
    {
        var first = viewport.WorldToScreen(rect.Min.X, rect.Min.Y);
        var second = viewport.WorldToScreen(rect.Max.X, rect.Max.Y);
        return new SKRect
        (
            (float)Math.Min(first.X, second.X),
            (float)Math.Min(first.Y, second.Y),
            (float)Math.Max(first.X, second.X),
            (float)Math.Max(first.Y, second.Y)
        );
    }

    private static SKRect RoundToPixel(SKRect boundingBox)
    {
        return new SKRect(
            (float)Math.Round(boundingBox.Left),
            (float)Math.Round(Math.Min(boundingBox.Top, boundingBox.Bottom)),
            (float)Math.Round(boundingBox.Right),
            (float)Math.Round(Math.Max(boundingBox.Top, boundingBox.Bottom)));
    }
}
