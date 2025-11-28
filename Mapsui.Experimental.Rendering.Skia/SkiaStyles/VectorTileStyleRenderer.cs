using Mapsui.Experimental.Rendering.Skia.Extensions;
using Mapsui.Experimental.Rendering.Skia.SkiaStyles;
using Mapsui.Experimental.VectorTiles.Extensions;
using Mapsui.Experimental.VectorTiles.Tiling;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia;

public class VectorTileStyleRenderer(MapRenderer? mapRenderer = null) : ISkiaStyleRenderer
{
    private readonly MapRenderer _mapRenderer = mapRenderer ?? new MapRenderer();

    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, Mapsui.Rendering.RenderService renderService, long iteration)
    {
        if (feature is not VectorTileFeature vectorStyleFeature)
            return false; // Todo: Log warning
        if (style is not VectorTileStyle vectorTileStyle)
            return false; // Todo: Log warning


        foreach (var vectorTileLayer in vectorStyleFeature.VectorTile.Layers)
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
                        _mapRenderer.TryGetStyleRenderer(featureStyle.GetType(), out var styleRenderer);
                        var skiaStyleRenderer = (ISkiaStyleRenderer?)styleRenderer!;
                        if (mapsuiFeature is Point)
                            return false;
                        skiaStyleRenderer.Draw(canvas, viewport, layer, mapsuiFeature, featureStyle, renderService, iteration);
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
}
