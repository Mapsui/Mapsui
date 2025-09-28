using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tests.Common.TestTools;

namespace Mapsui.Samples.Common.Maps.Tests;

public class StackedLabelsTestSample : ISample
{
    private const string _labelColumn = "Label";

    public string Category => "Tests";
    public string Name => "Stacked Labels";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var random = new Random(6);
        var features = RandomPointsBuilder.CreateRandomFeatures(new MRect(-100, -100, 100, 100), 20, random);
#pragma warning disable IDISP001 // Dispose created
        var layer = CreateLayer(features);
        var stackedLabelLayer = CreateStackedLabelLayer(features, _labelColumn);
#pragma warning restore IDISP001 // Dispose created

        var map = new Map
        {
            BackColor = Color.WhiteSmoke,
        };

        map.Navigator.ZoomToBox(layer.Extent!.Grow(layer.Extent.Width * 0.3));

        map.Layers.Add(stackedLabelLayer);
        map.Layers.Add(layer);

        return map;
    }

    private static TestLayer CreateStackedLabelLayer(IEnumerable<IFeature> provider, string labelColumn) => new()
    {
        DataSource = new StackedLabelProvider(new MemoryProvider(provider), new LabelStyle { LabelColumn = labelColumn }),
        Style = null
    };

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
