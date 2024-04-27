using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Samples.Common.Maps.Styles;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets.InfoWidgets;

namespace Mapsui.Tests.Common.Maps;

public class AtlasBitmapPathSample : ISample
{
    private const string AtlasLayerName = "Atlas BitmapPath Layer";
    private static Uri _atlasBitmapPath;
    private static readonly Random Random = new(1);

    public string Name => "Atlas BitmapPath";

    public string Category => "Tests";

    public Task<Map> CreateMapAsync()
    {
        _atlasBitmapPath = typeof(AtlasSample).LoadBitmapPath("Images.osm-liberty.png");
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
            Name = AtlasLayerName,
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

            var x = 0 + Random.Next(0, 12) * 21;
            var y = 64 + Random.Next(0, 6) * 21;
            var bitmapId = BitmapRegistry.Instance.Register(new Sprite(_atlasBitmapPath, x, y, 21, 21, 1));
            feature.Styles.Add(new SymbolStyle { BitmapId = bitmapId });
            counter++;
            return feature;
        }).ToList();
    }
}
