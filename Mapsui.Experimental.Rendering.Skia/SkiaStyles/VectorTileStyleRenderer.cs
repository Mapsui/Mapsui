using Mapsui.Layers;
using Mapsui.Experimental.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using SkiaSharp;
using Mapsui.Experimental.VectorTiles.Tiling;
using Mapsui.Experimental.Rendering.Skia.Extensions;
using Mapsui.Extensions;
using Mapsui.Experimental.VectorTiles.Extensions;
using NetTopologySuite.Geometries;

namespace Mapsui.Experimental.Rendering.Skia;

public class VectorTileStyleRenderer : ISkiaStyleRenderer
{
    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, Mapsui.Rendering.RenderService renderService, long iteration)
    {
        if (feature is not VectorTileFeature vectorStyleFeature)
            return false; // Todo: Log warning
#pragma warning disable IDE0059 // Unnecessary assignment of a value
        if (style is not VectorTileStyle vectorTileStyle)
            return false; // Todo: Log warning
#pragma warning restore IDE0059 // Unnecessary assignment of a value

        var vectorStyleRenderer = new VectorStyleRenderer();
        foreach (var vectorTileLayer in vectorStyleFeature.VectorTile.Layers)
        {
            if (feature.Extent is null)
                continue;

            var vectorStyle = CreateVectorTileStyle();

            foreach (var childFeature in vectorTileLayer.Features)
            {
                var saveCount = canvas.Save();
                try
                {
                    var screenExtent = viewport.WorldToScreen(feature.Extent);

                    if (childFeature.Geometry is not Point)
                    {
                        using var clipPath = screenExtent.ToSkiaPath();
                        canvas.ClipPath(clipPath);
                    }
                    var mapsuiChild = childFeature.ToMapsui();
                    vectorStyleRenderer.Draw(canvas, viewport, layer, mapsuiChild, vectorStyle, renderService, iteration);
                }
                finally
                {
                    canvas.RestoreToCount(saveCount);
                }
            }
        }
        return true;
    }

    private static IStyle CreateVectorTileStyle()
    {
        return new VectorStyle
        {
            Fill = null,
            Outline = new Pen { Color = Color.Blue, Width = 1 },
        };
    }
}
