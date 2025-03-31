using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets.InfoWidgets;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Styles;

public class SvgSample : ISample
{
    private static readonly int _numberOfSvgs = 2000;
    public string Name => $"Many SVGs ({_numberOfSvgs})";
    public string Category => "Style";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateSvgLayer(map.Extent));

        map.Widgets.Add(new MapInfoWidget(map, l => l.Name == "Svg Layer"));

        return Task.FromResult(map);
    }

    private static MemoryLayer CreateSvgLayer(MRect? envelope) => new()
    {
        Name = "Svg Layer",
        Features = CreateSvgFeatures(RandomPointsBuilder.GenerateRandomPoints(envelope, _numberOfSvgs)),
        Style = null,
    };

    private static IFeature[] CreateSvgFeatures(IEnumerable<MPoint> randomPoints)
    {
        var counter = 0;

        return randomPoints.Select(p =>
        {
            var feature = new PointFeature(p) { ["Label"] = counter.ToString() };
            feature.Styles.Add(CreateSvgStyle());
            counter++;
            return feature;
        }).ToArray();
    }

    private static ImageStyle CreateSvgStyle() => new()
    {
        Image = "embedded://Mapsui.Samples.Common.Images.Pin.svg",
        SymbolScale = 0.5,
        RelativeOffset = new RelativeOffset(0.0, 0.5)
    };
}
