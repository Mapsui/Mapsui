﻿using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tiling;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps.Geometries;

public class VariousSample : ISample, ISampleTest
{
    public string Name => "Various geometries";
    public string Category => "Geometries";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(PolygonGeometrySample.CreateLayer());
        map.Layers.Add(LineStringSample.CreateLineStringLayer(LineStringSample.CreateLineStringStyle()));
        map.Layers.Add(CreateLayerWithStyleOnLayer(map.Extent, 10));
        map.Layers.Add(CreateLayerWithStyleOnFeature(map.Extent, 10));

        return Task.FromResult(map);
    }

    private static ILayer CreateLayerWithStyleOnLayer(MRect? envelope, int count = 25)
    {
        return new Layer("Style on Layer")
        {
            DataSource = new MemoryProvider(RandomPointsBuilder.GenerateRandomPoints(envelope, count).ToFeatures()),
            Style = CreateBitmapStyle("embedded://Mapsui.Samples.Common.Images.ic_place_black_24dp.png")
        };
    }

    private static ILayer CreateLayerWithStyleOnFeature(MRect? envelope, int count = 25)
    {
        var style = CreateBitmapStyle("embedded://Mapsui.Samples.Common.Images.loc.png");

        return new Layer("Style on feature")
        {
            DataSource = new MemoryProvider(GenerateRandomFeatures(envelope, count, style)),
            Style = null
        };
    }

    private static IEnumerable<IFeature> GenerateRandomFeatures(MRect? envelope, int count, IStyle style)
    {
        return RandomPointsBuilder.GenerateRandomPoints(envelope, count, new System.Random(123))
            .Select(p => new PointFeature(p) { Styles = new List<IStyle> { style } }).ToList();
    }

    private static ImageStyle CreateBitmapStyle(string embeddedResourcePath)
    {
        return new ImageStyle { Image = embeddedResourcePath, SymbolScale = 0.75 };
    }

    public async Task InitializeTestAsync(IMapControl mapControl)
    {
        await Task.Delay(1000);
    }
}
