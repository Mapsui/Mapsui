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

public class SymbolsSample : ISample
{
    public string Name => "Symbols";
    public string Category => "Styles";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateStylesLayer(map.Extent));

        map.Widgets.Add(new MapInfoWidget(map, l => l.Name == "Styles Layer"));

        return Task.FromResult(map);
    }

    private static ILayer CreateStylesLayer(MRect? envelope)
    {
        return new MemoryLayer
        {
            Name = "Styles Layer",
            Features = CreateDiverseFeatures(RandomPointsBuilder.GenerateRandomPoints(envelope, 25)),
            Style = null,
        };
    }

    private static IEnumerable<IFeature> CreateDiverseFeatures(IEnumerable<MPoint> randomPoints)
    {
        var features = new List<IFeature>();
        var counter = 0;
        var styles = CreateDiverseStyles().ToList();
        foreach (var point in randomPoints)
        {
            var feature = new PointFeature(point)
            {
                ["Label"] = counter.ToString()
            };

            feature.Styles.Add(styles[counter]);
            feature.Styles.Add(SmalleDot());
            features.Add(feature);
            counter++;
            if (counter == styles.Count) counter = 0;

        }
        features.Add(CreatePointWithStackedStyles());
        return features;
    }

    private static IStyle SmalleDot()
    {
        return new SymbolStyle { SymbolScale = 0.2, Fill = new Brush(new Color(40, 40, 40)) };
    }

    private static IEnumerable<IStyle> CreateDiverseStyles()
    {
        const int radius = 16;
        return
        [
            new SymbolStyle {SymbolScale = 0.8, Offset = new Offset(0, 0), SymbolType = SymbolType.Rectangle},
            new SymbolStyle {SymbolScale = 0.6, Offset = new Offset(radius, radius), SymbolType = SymbolType.Rectangle, Fill = new Brush(Color.Red)},
            new SymbolStyle {SymbolScale = 1, Offset = new Offset(radius, -radius), SymbolType = SymbolType.Rectangle},
            new SymbolStyle {SymbolScale = 1, Offset = new Offset(-radius, -radius), SymbolType = SymbolType.Rectangle},
            new SymbolStyle {SymbolScale = 0.8, Offset = new Offset(0, 0)},
            new SymbolStyle {SymbolScale = 1.2, Offset = new Offset(radius, 0)},
            new SymbolStyle {SymbolScale = 1, Offset = new Offset(0, radius)},
            new SymbolStyle {SymbolScale = 1, Offset = new Offset(radius, radius)},
            CreateBitmapStyle("embedded://Mapsui.Samples.Common.Images.ic_place_black_24dp.png", 0.7),
            CreateBitmapStyle("embedded://Mapsui.Samples.Common.Images.ic_place_black_24dp.png", 0.8),
            CreateBitmapStyle("embedded://Mapsui.Samples.Common.Images.ic_place_black_24dp.png", 0.9),
            CreateBitmapStyle("embedded://Mapsui.Samples.Common.Images.ic_place_black_24dp.png", 1.0),
            CreateSvgStyle("embedded://Mapsui.Samples.Common.Images.Pin.svg", 0.7),
            CreateSvgStyle("embedded://Mapsui.Samples.Common.Images.Pin.svg", 0.8),
            CreateSvgStyle("embedded://Mapsui.Samples.Common.Images.Ghostscript_Tiger.svg", 0.05),
            CreateSvgStyle("embedded://Mapsui.Samples.Common.Images.Ghostscript_Tiger.svg", 0.1),
        ];
    }

    private static SymbolStyle CreateBitmapStyle(string embeddedResourcePath, double scale)
    {
        return new SymbolStyle
        {
            Image = new ResourceImage { Source = embeddedResourcePath },
            SymbolScale = scale,
            Offset = new Offset(0, 32),
        };
    }

    private static SymbolStyle CreateSvgStyle(string embeddedResourcePath, double scale)
    {
        return new SymbolStyle { Image = new ResourceImage { Source = embeddedResourcePath }, SymbolScale = scale, RelativeOffset = new RelativeOffset(0.0, 0.5) };
    }

    private static PointFeature CreatePointWithStackedStyles() => new(new MPoint(5000000, -5000000))
    {
        Styles =
        [
            new SymbolStyle
            {
                SymbolScale = 2.0f,
                Fill = null,
                Outline = new Pen { Color = Color.Yellow }
            },
            new SymbolStyle
            {
                SymbolScale = 0.8f,
                Fill = new Brush { Color = Color.Red }
            },
            new SymbolStyle
            {
                SymbolScale = 0.5f,
                Fill = new Brush { Color = Color.Black }
            },
            new LabelStyle
            {
                Text = "Stacked Styles",
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left
            }
        ]
    };
}
