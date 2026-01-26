using Mapsui.Experimental.Rendering.Skia.Caching;
using Mapsui.Experimental.Rendering.Skia.SkiaStyles;
using Mapsui.Experimental.VectorTiles.Rendering;
using Mapsui.Experimental.VectorTiles.Tiling;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering;
using Mapsui.Rendering.Caching;
using Mapsui.Styles;
using SkiaSharp;
using System;
using VexTile.Renderer.Mvt.AliFlux;
using VexVectorStyle = VexTile.Renderer.Mvt.AliFlux.VectorStyle;

namespace Mapsui.Experimental.Rendering.Skia;

/// <summary>
/// Renderer for VexTileStyle. Renders VexTileFeature's vector data to SKImage,
/// caches the result, and draws to the canvas.
/// </summary>
public class VexTileStyleRenderer : ISkiaStyleRenderer
{
    /// <inheritdoc />
    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, RenderService renderService, long currentIteration)
    {
        try
        {
            if (feature is not VexTileFeature vexTileFeature)
            {
                Logger.Log(LogLevel.Warning, $"VexTileStyleRenderer expected feature of type {nameof(VexTileFeature)} but received {feature?.GetType().FullName ?? "null"} (Layer: {layer.Name}, Iteration: {currentIteration}).");
                return false;
            }

            if (style is not VexTileStyle vexTileStyle)
            {
                Logger.Log(LogLevel.Warning, $"VexTileStyleRenderer expected style of type {nameof(VexTileStyle)} but received {style?.GetType().FullName ?? "null"} (Layer: {layer.Name}).");
                return false;
            }


            var extent = feature.Extent;
            if (extent == null)
                return false;

            // Get or create cached rendered image using feature Id as key
#pragma warning disable IDISP001 // Dispose created - cache managed by RenderService
            var featureIdTileCache = renderService.GetLayerFeatureIdTileCache(layer.Id);
#pragma warning restore IDISP001
            featureIdTileCache.UpdateCache(currentIteration);

            var cacheEntry = featureIdTileCache.GetOrAdd(vexTileFeature.Id, _ => RenderToImage(vexTileFeature, vexTileStyle.VexStyle), currentIteration);
            if (cacheEntry?.Data is not SKImage image)
                return false;

            var opacity = (float)(layer.Opacity * style.Opacity);

            canvas.Save();

            if (viewport.IsRotated())
            {
                canvas.SetMatrix(CreateRotationMatrix(viewport, extent, canvas.TotalMatrix));
                var destination = new SKRect(0.0f, 0.0f, (float)extent.Width, (float)extent.Height);
                BitmapRenderer.Draw(canvas, image, destination, opacity);
            }
            else
            {
                var destination = WorldToScreen(viewport, extent);
                BitmapRenderer.Draw(canvas, image, RoundToPixel(destination), opacity);
            }

            canvas.Restore();
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
            return false;
        }

        return true;
    }

    private static ITileCacheEntry RenderToImage(VexTileFeature feature, VexVectorStyle vectorStyle)
    {
        var skiaCanvas = new SkiaCanvas();
        VexTileRenderer.Render(feature.VectorTile, vectorStyle, skiaCanvas, feature.VexTileInfo);
#pragma warning disable IDISP001 // Dispose created - ownership transferred to cache entry
        var image = skiaCanvas.ToSKImage();
#pragma warning restore IDISP001
        return new TileCacheEntry(image);
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
