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
    public string Name => "Svg";
    public string Category => "Styles";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateSvgLayer(map.Extent));

        map.Widgets.Add(new MapInfoWidget(map));

        return Task.FromResult(map);
    }

    private static ILayer CreateSvgLayer(MRect? envelope)
    {
        return new MemoryLayer
        {
            Name = "Svg Layer",
            Features = CreateSvgFeatures(RandomPointsBuilder.GenerateRandomPoints(envelope, 2000)),
            Style = null,
            IsMapInfoLayer = true
        };
    }

    private static IEnumerable<IFeature> CreateSvgFeatures(IEnumerable<MPoint> randomPoints)
    {
        var counter = 0;

        return randomPoints.Select(p =>
        {
            var feature = new PointFeature(p) { ["Label"] = counter.ToString() };
            feature.Styles.Add(CreateSvgStyle());
            counter++;
            return feature;
        });
    }

    private static SymbolStyle CreateSvgStyle() => new()
    {
        ImageSource = "embedded://Mapsui.Samples.Common.Images.Pin.svg",
        SymbolScale = 0.5,
        SymbolOffset = new RelativeOffset(0.0, 0.5)
    };
}
