using Mapsui.Layers;
using Mapsui.Rendering;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Layers;
using Mapsui.Tiling.Rendering;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Performance;


/// <summary>
/// This sample demonstrates how to use a RasterizingTileLayer with a CustomPointStyle renderer for improved 
/// performance when rendering one million points.
/// </summary>
public sealed class RasterizingTileLayerWithCustomPointStyleSample : ISample
{
    public string Name => "RasterizingTileLayerUsingCustomPointStyle";
    public string Category => "1";

    static readonly SKPaint _paint = new() { Color = new SKColor(79, 10, 107, 192), IsAntialias = true };

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateRasterizingTileLayer());
        var extent = map.Layers.Get(1).Extent!.Grow(map.Layers.Get(1).Extent!.Width * 0.1);
        map.Navigator.ZoomToBox(extent);

        Rendering.Skia.MapRenderer.RegisterPointStyleRenderer("custom-style-basic", MyBasicCustomStyleRenderer);
        // You can add the Mapsui.Experimental.Rendering.Skia package and use the Experimental RegisterPointStyleRenderer to get accss to the IFeature parameter.
        // Uncomment: Experimental.Rendering.Skia.MapRenderer.RegisterPointStyleRenderer("custom-style-basic", MyBasicCustomStyleRenderer);

        return Task.FromResult(map);
    }

    private static RasterizingTileLayer CreateRasterizingTileLayer() => new(
        CreateRandomPointLayer(),
        renderFetchStrategy: new ImprovedRenderFetchStrategy(),
        dataFetchStrategy: new DataFetchStrategy())
    {
        Style = new RasterStyle { Outline = new Pen(Color.Gray, 1) },
    };

    private static void MyBasicCustomStyleRenderer(SKCanvas canvas, IPointStyle style, RenderService renderService, float opacity)
    {
        canvas.DrawRect(-5f, -5f, 10f, 10f, _paint);
    }

    private static MemoryLayer CreateRandomPointLayer()
    {
        var rnd = new Random(3462); // Fix the random seed so the features don't move after a refresh
        var features = new List<IFeature>();
        for (var i = 0; i < 1_000_000; i++)
        {
            features.Add(new PointFeature(new MPoint(rnd.Next(-3_000_000, 3_000_000), rnd.Next(-3_000_000, 3_000_000))));
        }

        return new MemoryLayer
        {
            Name = "Points",
            Features = features,
            Style = new CustomPointStyle { RendererName = "custom-style-basic" },
        };
    }
}
