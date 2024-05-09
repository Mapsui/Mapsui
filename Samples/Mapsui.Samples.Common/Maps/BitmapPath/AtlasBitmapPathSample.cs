using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets.InfoWidgets;

namespace Mapsui.Samples.Common.Maps.BitmapPath;

public class AtlasBitmapPathSample : ISample
{
    private const string _atlasLayerName = "Atlas BitmapPath Layer";
    private static readonly Uri _atlasBitmapPath = new("embeddedresource://mapsui.samples.common.images.osm-liberty.png");
    private static readonly Random _random = new(1);

    public string Name => "Atlas BitmapPath";

    public string Category => "BitmapPath";

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
            Name = _atlasLayerName,
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
            var bitmapId = BitmapRegistry.Instance.Register(new Sprite(_atlasBitmapPath, x, y, 21, 21, 1));
            feature.Styles.Add(new SymbolStyle { BitmapId = bitmapId });
            counter++;
            return feature;
        }).ToList();
    }
}
