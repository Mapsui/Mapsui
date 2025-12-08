using Mapsui.Experimental.Rendering.Skia.Extensions;
using Mapsui.Experimental.Rendering.Skia.MapInfos;
using Mapsui.Experimental.Rendering.Skia.SkiaStyles;
using Mapsui.Experimental.VectorTiles.Extensions;
using Mapsui.Experimental.VectorTiles.Tiling;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Manipulations;
using Mapsui.Rendering;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Mapsui.Experimental.Rendering.Skia;

/// <summary>
/// Renderer for VectorTileStyle.
/// </summary>
/// <param name="mapRenderer">The map renderer to use for rendering the vector tile features.</param>
public class VectorTileStyleRenderer(MapRenderer? mapRenderer = null) : ISkiaStyleRenderer, IMapInfoRenderer
{
    private readonly MapRenderer _mapRenderer = mapRenderer ?? new MapRenderer();

    /// <summary>
    /// Draws the vector tile feature.
    /// </summary>
    /// <param name="canvas">The Skia canvas to draw on.</param>
    /// <param name="viewport">The current viewport.</param>
    /// <param name="layer">The layer the feature belongs to.</param>
    /// <param name="feature">The feature to draw.</param>
    /// <param name="style">The style to apply.</param>
    /// <param name="renderService">The render service.</param>
    /// <param name="iteration">The current iteration.</param>
    /// <returns>True if the feature was drawn, false otherwise.</returns>
    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, RenderService renderService, long iteration)
    {
        if (feature is not VectorTileFeature vectorTileFeature)
        {
            Logger.Log(LogLevel.Warning, $"VectorTileStyleRenderer expected feature of type {nameof(VectorTileFeature)} but received {feature?.GetType().FullName ?? "null"} (Layer: {layer.Name}, Iteration: {iteration}).");
            return false;
        }
        if (style is not VectorTileStyle vectorTileStyle)
        {
            Logger.Log(LogLevel.Warning, $"VectorTileStyleRenderer expected style of type {nameof(VectorTileStyle)} but received {style?.GetType().FullName ?? "null"} (Layer: {layer.Name}).");
            return false;
        }

        foreach (var vectorTileLayer in vectorTileFeature.VectorTile.Layers)
        {
            if (feature.Extent is null)
                continue;

            foreach (var ntsFeature in vectorTileLayer.Features)
            {
                var saveCount = canvas.Save();
                try
                {
                    var screenExtent = viewport.WorldToScreen(feature.Extent);

                    if (ntsFeature.Geometry is Polygon || ntsFeature.Geometry is MultiPolygon)
                    {
                        using var clipPath = screenExtent.ToSkiaPath();
                        canvas.ClipPath(clipPath);
                    }
                    var mapsuiFeature = ntsFeature.ToMapsui();

                    var featureStyles = vectorTileStyle.Style.GetStylesToApply(mapsuiFeature, viewport);
                    foreach (var featureStyle in featureStyles)
                    {
                        if (_mapRenderer.TryGetStyleRenderer(featureStyle.GetType(), out var styleRenderer))
                        {
                            var skiaStyleRenderer = styleRenderer as ISkiaStyleRenderer;
                            if (skiaStyleRenderer != null)
                            {
                                if (mapsuiFeature is Point)
                                    return false;
                                skiaStyleRenderer.Draw(canvas, viewport, layer, mapsuiFeature, featureStyle, renderService, iteration);
                            }
                            else
                            {
                                Logger.Log(LogLevel.Warning, $"Registered style renderer for style type {featureStyle.GetType().FullName} does not implement {nameof(ISkiaStyleRenderer)} (Layer: {layer.Name}, Iteration: {iteration}).");
                                continue;
                            }
                        }
                        else
                        {
                            Logger.Log(LogLevel.Warning, $"No style renderer registered for style type {featureStyle.GetType().FullName} (Layer: {layer.Name}, Iteration: {iteration}).");
                            continue;
                        }
                    }
                }
                finally
                {
                    canvas.RestoreToCount(saveCount);
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Gets the map info for the vector tile feature.
    /// </summary>
    /// <param name="canvas">The Skia canvas.</param>
    /// <param name="screenPosition">The screen position to check.</param>
    /// <param name="viewport">The current viewport.</param>
    /// <param name="feature">The feature to check.</param>
    /// <param name="style">The style to apply.</param>
    /// <param name="layer">The layer the feature belongs to.</param>
    /// <param name="renderService">The render service.</param>
    /// <param name="margin">The margin to use for hit testing.</param>
    /// <returns>A collection of map info records.</returns>
    public IEnumerable<MapInfoRecord> GetMapInfo(
        SKCanvas canvas, ScreenPosition screenPosition, Viewport viewport, IFeature feature, IStyle style, ILayer layer, RenderService renderService, int margin = 0)
    {
        var mapInfoRecords = new List<MapInfoRecord>();

        if (feature is not VectorTileFeature vectorTileFeature)
        {
            Logger.Log(LogLevel.Warning, $"VectorTileStyleRenderer.GetMapInfo expected feature of type {nameof(VectorTileFeature)} but received {feature?.GetType().FullName ?? "null"} (Layer: {layer.Name}).");
            return mapInfoRecords;
        }
        if (style is not VectorTileStyle vectorTileStyle)
        {
            Logger.Log(LogLevel.Warning, $"VectorTileStyleRenderer.GetMapInfo expected style of type {nameof(VectorTileStyle)} but received {style?.GetType().FullName ?? "null"} (Layer: {layer.Name}).");
            return mapInfoRecords;
        }

        var intX = (int)screenPosition.X;
        var intY = (int)screenPosition.Y;

        // Get the surface from the canvas to access pixels
        var surface = canvas.Surface;
        if (surface == null)
        {
            Logger.Log(LogLevel.Warning, "Canvas surface is null in VectorTileStyleRenderer.GetMapInfo");
            return mapInfoRecords;
        }

        using var pixMap = surface.PeekPixels();
        using var clearPixelPaint = new SKPaint { Color = SKColors.Transparent, BlendMode = SKBlendMode.Src };

        foreach (var vectorTileLayer in vectorTileFeature.VectorTile.Layers)
        {
            if (feature.Extent is null)
                continue;

            foreach (var ntsFeature in vectorTileLayer.Features)
            {
                try
                {
                    var screenExtent = viewport.WorldToScreen(feature.Extent);

                    var mapsuiFeature = ntsFeature.ToMapsui();
                    var featureStyles = vectorTileStyle.Style.GetStylesToApply(mapsuiFeature, viewport);

                    foreach (var featureStyle in featureStyles)
                    {
                        if (_mapRenderer.TryGetStyleRenderer(featureStyle.GetType(), out var styleRenderer))
                        {
                            canvas.DrawPoint(intX, intY, clearPixelPaint);
                            var originalColor = pixMap.GetPixelColor(intX, intY);

                            if (styleRenderer is ISkiaStyleRenderer skiaStyleRenderer)
                            {
                                skiaStyleRenderer.Draw(canvas, viewport, layer, mapsuiFeature, featureStyle, renderService, 0);

                                // Check if the pixel has changed (i.e., something was rendered at the top position)
                                if (originalColor != pixMap.GetPixelColor(intX, intY))
                                {
                                    mapInfoRecords.Add(new MapInfoRecord(mapsuiFeature, featureStyle, layer));
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logger.Log(LogLevel.Error, "Unexpected error in VectorTileStyleRenderer.GetMapInfo", exception);
                }
            }
        }

        return mapInfoRecords;
    }
}
