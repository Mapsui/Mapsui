using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps.Tests;

public class StackedLabelsTestSample : ISample
{
    private const string _labelColumn = "Label";

    public string Category => "Tests";
    public string Name => "StackedLabels";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var random = new Random(6);
        var features = RandomPointsBuilder.CreateRandomFeatures(new MRect(-100, -100, 100, 100), 20, random);
        var layer = CreateLayer(features);
        var stackedLabelLayer = CreateStackedLabelLayer(features, _labelColumn);

        var map = new Map
        {
            BackColor = Color.WhiteSmoke,
        };

        map.Navigator.ZoomToBox(layer.Extent!.Grow(layer.Extent.Width * 0.3));

        map.Layers.Add(stackedLabelLayer);
        map.Layers.Add(layer);

        return map;
    }

    private static Layer CreateStackedLabelLayer(IEnumerable<IFeature> features, string labelColumn)
    {
        return new Layer("StackedLabels")
        {
            DataSource = new StackedLabelProvider(new MemoryProvider(features), new LabelStyle { LabelColumn = labelColumn }),
            Style = null
        };
    }

    private static MemoryLayer CreateLayer(IEnumerable<IFeature> features) => new()
    {
        Features = features,
        Style = new SymbolStyle
        {
            SymbolScale = 1,
            Outline = new Pen(Color.Gray, 1f),
            Fill = new Brush(new Color { A = 128, R = 8, G = 20, B = 192 })
        }
    };
}
