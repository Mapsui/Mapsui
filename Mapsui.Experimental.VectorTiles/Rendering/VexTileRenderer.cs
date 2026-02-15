// defining this will add a box around the tile boundary and also
// burn in the XYZ value of the tile. This is very handy for debugging
//#define USE_DEBUG_BOX

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SkiaSharp;
using VexTile.Renderer.Mvt.AliFlux;
using VexTile.Renderer.Mvt.AliFlux.Drawing;
using VexTile.Renderer.Mvt.AliFlux.Enums;

namespace Mapsui.Experimental.VectorTiles.Rendering;

public static class VexTileRenderer
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Renders a pre-fetched vector tile to the canvas synchronously.
    /// </summary>
    /// <param name="vectorTile">The pre-fetched vector tile data.</param>
    /// <param name="style">The style to use for rendering.</param>
    /// <param name="canvas">The canvas to draw to.</param>
    /// <param name="x">The tile X coordinate.</param>
    /// <param name="y">The tile Y coordinate.</param>
    /// <param name="zoom">The zoom level.</param>
    /// <param name="sizeX">The tile width, defaults to 512.</param>
    /// <param name="sizeY">The tile height, defaults to 512.</param>
    /// <param name="scale">The scale, defaults to 1.</param>
    /// <param name="whiteListLayers">Optional whitelist to reduce layers to render.</param>
    /// <param name="overrideBackground">Override the default background color.</param>
    public static void Render(
        VectorTile vectorTile,
        VectorStyle style,
        ICanvas canvas,
        int x, int y, double zoom,
        double sizeX = 512, double sizeY = 512,
        double scale = 1,
        List<string>? whiteListLayers = null,
        Color? overrideBackground = null)
    {
        var tileInfo = new TileInfo(x, y, zoom, sizeX, sizeY, scale, whiteListLayers);
        Render(vectorTile, style, canvas, tileInfo, overrideBackground);
    }

    /// <summary>
    /// Renders a pre-fetched vector tile to the canvas synchronously.
    /// </summary>
    /// <param name="vectorTile">The pre-fetched vector tile data.</param>
    /// <param name="style">The style to use for rendering.</param>
    /// <param name="canvas">The canvas to draw to.</param>
    /// <param name="tileInfo">The tile information.</param>
    /// <param name="overrideBackground">Override the default background color.</param>
    public static void Render(
        VectorTile vectorTile,
        VectorStyle style,
        ICanvas canvas,
        TileInfo tileInfo,
        Color? overrideBackground = null)
    {
        Dictionary<string, List<VectorTileLayer>> categorizedVectorLayers = new();

        double actualZoom = tileInfo.Zoom;

        if (tileInfo.SizeX < 1024)
        {
            double ratio = 1024 / tileInfo.SizeX;
            double zoomDelta = Math.Log(ratio, 2);

            actualZoom = tileInfo.Zoom - zoomDelta;
        }

        var sizeX = tileInfo.ScaledSizeX;
        var sizeY = tileInfo.ScaledSizeY;

        canvas.StartDrawing(sizeX, sizeY);

        var visualLayers = new List<VisualLayer>();

        // Process the pre-fetched tile
        if (vectorTile.IsOverZoomed)
        {
            canvas.ClipOverflow = true;
        }

        // Note: Geometry normalization (from tile-internal space to pixel space) is now
        // performed at fetch time in VexTileSource.NormalizeGeometry, so the renderer
        // receives pre-normalized coordinates and does not mutate the tile geometry.

        foreach (var tileLayer in vectorTile.Layers)
        {
            if (!categorizedVectorLayers.ContainsKey(tileLayer.Name))
            {
                categorizedVectorLayers[tileLayer.Name] = new();
            }

            categorizedVectorLayers[tileLayer.Name].Add(tileLayer);
        }

        // Apply styling
        foreach (var layer in style.Layers)
        {
            if (tileInfo.LayerWhiteList != null &&
                layer.Type != "background" &&
                layer.SourceLayer != "" &&
                !tileInfo.LayerWhiteList.Contains(layer.SourceLayer))
            {
                continue;
            }

            if (layer.Source != null)
            {
                if (layer.Source.Type == "raster")
                {
                    continue;
                }

                if (categorizedVectorLayers.TryGetValue(layer.SourceLayer, out var tileLayers))
                {
                    foreach (var tileLayer in tileLayers)
                    {
                        foreach (var feature in tileLayer.Features)
                        {
                            Dictionary<string, object> attributes = new(feature.Attributes)
                            {
                                ["$type"] = feature.GeometryType,
                                ["$id"] = layer.ID,
                                ["$zoom"] = actualZoom
                            };

                            if (style.ValidateLayer(layer, actualZoom, attributes))
                            {
                                var brush = style.ParseStyle(layer, tileInfo.Scale, attributes);

                                if (!brush.Paint.Visibility)
                                {
                                    continue;
                                }

                                visualLayers.Add(new()
                                {
                                    Type = VisualLayerType.Vector,
                                    VectorTileFeature = feature,
                                    Geometry = feature.Geometry,
                                    Brush = brush,
                                    LayerId = layer.ID,
                                    SourceName = layer.SourceName,
                                    SourceLayer = layer.SourceLayer,
                                });
                            }
                        }
                    }
                }
            }
            else if (layer.Type == "background")
            {
                var brushes = style.GetStyleByType("background", actualZoom, tileInfo.Scale);
                foreach (var brush in brushes)
                {
                    if (overrideBackground is { } c)
                    {
                        brush.Paint.BackgroundColor = new SKColor(c.R, c.G, c.B, c.A);
                    }

                    canvas.DrawBackground(brush.Paint.BackgroundColor);
                }
            }
        }

        RenderVisualLayers(canvas, visualLayers);
    }

    private static void RenderVisualLayers(ICanvas canvas, List<VisualLayer> visualLayers)
    {
        // deferred rendering to preserve text drawing order
        foreach (var layer in visualLayers.OrderBy(item => item.Brush.ZIndex))
        {
            if (layer.Type == VisualLayerType.Vector)
            {
                var feature = layer.VectorTileFeature;
                var geometry = layer.Geometry;
                var brush = layer.Brush;

                if (!brush.Paint.Visibility)
                {
                    continue;
                }

                try
                {
                    if (feature.GeometryType == "Point")
                    {
                        foreach (var point in geometry)
                        {
                            canvas.DrawPoint(point.First(), brush);
                        }
                    }
                    else if (feature.GeometryType == "LineString")
                    {
                        foreach (var line in geometry)
                        {
                            canvas.DrawLineString(line, brush);
                        }
                    }
                    else if (feature.GeometryType == "Polygon")
                    {
                        foreach (var polygon in geometry)
                        {
                            //we know water is broken, so for now we are special casing it
                            if (layer.SourceLayer == "water")
                            {
                                canvas.DrawPolygon(polygon, brush, canvas.BackgroundColor);
                            }
                            else
                            {
                                canvas.DrawPolygon(polygon, brush, null);
                            }
                        }
                    }
                    else if (feature.GeometryType == "Unknown")
                    {
                        canvas.DrawUnknown(geometry, brush);
                    }
                    else
                    {
                        Log.Debug($"unknown Geometry type {feature.GeometryType}");
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            else if (layer.Type == VisualLayerType.Raster)
            {
                canvas.DrawImage(layer.RasterData, layer.Brush);
            }
        }

        foreach (var layer in visualLayers.OrderBy(item => item.Brush.ZIndex).Reverse())
        {
            if (layer.Type == VisualLayerType.Vector)
            {
                var feature = layer.VectorTileFeature;
                var geometry = layer.Geometry;
                var brush = layer.Brush;

                if (!brush.Paint.Visibility)
                {
                    continue;
                }

                if (feature.GeometryType == "Point")
                {
                    foreach (var point in geometry)
                    {
                        if (brush.Text != null)
                        {
                            canvas.DrawText(point.First(), brush);
                        }
                    }
                }
                else if (feature.GeometryType == "LineString")
                {
                    foreach (var line in geometry)
                    {
                        if (brush.Text != null)
                        {
                            canvas.DrawTextOnPath(line, brush);
                        }
                    }
                }
            }
        }

#if USE_DEBUG_BOX
        canvas.DrawDebugBox(tileData, SKColors.Black);
#endif
    }
}
