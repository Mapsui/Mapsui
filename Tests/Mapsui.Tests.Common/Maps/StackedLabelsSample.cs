using System;
using System.Collections.Generic;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tests.Common.TestTools;
using Mapsui.UI;

namespace Mapsui.Tests.Common.Maps;

public class StackedLabelsTestSample : IMapControlSample
{
    private const string LabelColumn = "Label";
    public string Category => "Tests";
    public string Name => "Stacked Labels";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

    public static Map CreateMap()
    {
        var random = new Random(6);
        var features = RandomPointsBuilder.CreateRandomFeatures(new MRect(-100, -100, 100, 100), 20, random);
        var layer = CreateLayer(features);
        var stackedLabelLayer = CreateStackedLabelLayer(features, LabelColumn);

        var map = new Map
        {
            BackColor = Color.FromString("WhiteSmoke"),
            Home = n => n.NavigateTo(layer.Extent!.Grow(layer.Extent.Width * 0.3))
        };

        map.Layers.Add(stackedLabelLayer);
        map.Layers.Add(layer);

        return map;
    }

    private static TestLayer CreateStackedLabelLayer(IEnumerable<IFeature> provider, string labelColumn)
    {
        return new TestLayer
        {
            DataSource = new StackedLabelProvider(new MemoryProvider(provider), new LabelStyle { LabelColumn = labelColumn }),
            Style = null
        };
    }

    private static MemoryLayer CreateLayer(IEnumerable<IFeature> features)
    {
        return new MemoryLayer
        {
            Features = features,
            Style = new SymbolStyle { SymbolScale = 1, Fill = new Brush(new Color { A = 128, R = 8, G = 20, B = 192 }) }
        };
    }
}
