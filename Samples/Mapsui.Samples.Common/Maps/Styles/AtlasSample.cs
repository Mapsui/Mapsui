using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets.InfoWidgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Styles;

public class AtlasSample : ISample
{
    private const string _layerName = "Sprites";
    private static readonly Random _random = new(1);

    public string Name => "Sprites";

    public string Category => "Styles";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateAtlasLayer(map.Extent));

        map.Widgets.Add(new MapInfoWidget(map));

        return Task.FromResult(map);
    }

    private static ILayer CreateAtlasLayer(MRect? envelope)
    {
        return new MemoryLayer
        {
            Name = _layerName,
            Features = CreateAtlasFeatures(RandomPointsBuilder.GenerateRandomPoints(envelope, 1000)),
            Style = null,
            IsMapInfoLayer = true
        };
    }

    private static IEnumerable<IFeature> CreateAtlasFeatures(IEnumerable<MPoint> randomPoints)
    {
        var counter = 0;

        return randomPoints.Select(p =>
        {
            var feature = new PointFeature(p) { ["Label"] = counter.ToString() };
            var x = 0 + _random.Next(0, 12) * 21;
            var y = 64 + _random.Next(0, 6) * 21;
            feature.Styles.Add(CreateSymbolStyle(x, y));
            counter++;
            return feature;
        }).ToList();
    }

    private static SymbolStyle CreateSymbolStyle(int x, int y) => new()
    {
        ImageSource = "embedded://mapsui.samples.common.images.osm-liberty.png",
        BitmapRegion = new BitmapRegion(x, y, 21, 21)
    };
}
