using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Utilities;
using System.Collections.Generic;
using System.Linq;
/* Unmerged change from project 'Mapsui.Samples.Common (netstandard2.0)'
Before:
using System.Threading.Tasks;
After:
using System.Threading.Tasks;
using Mapsui;
using Mapsui.Samples;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.Common.Maps.Styles;
*/
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

        return Task.FromResult(map);
    }

    private static ILayer CreateStylesLayer(MRect? envelope)
    {
        return new MemoryLayer
        {
            Name = "Styles Layer",
            Features = CreateDiverseFeatures(RandomPointsBuilder.GenerateRandomPoints(envelope, 25)),
            Style = null,
            IsMapInfoLayer = true
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
        const int diameter = 16;
        return new List<IStyle>
        {
            new SymbolStyle {SymbolScale = 0.8, SymbolOffset = new Offset(0, 0), SymbolType = SymbolType.Rectangle},
            new SymbolStyle {SymbolScale = 0.6, SymbolOffset = new Offset(diameter, diameter), SymbolType = SymbolType.Rectangle, Fill = new Brush(Color.Red)},
            new SymbolStyle {SymbolScale = 1, SymbolOffset = new Offset(diameter, -diameter), SymbolType = SymbolType.Rectangle},
            new SymbolStyle {SymbolScale = 1, SymbolOffset = new Offset(-diameter, -diameter), SymbolType = SymbolType.Rectangle},
            new SymbolStyle {SymbolScale = 0.8, SymbolOffset = new Offset(0, 0)},
            new SymbolStyle {SymbolScale = 1.2, SymbolOffset = new Offset(diameter, 0)},
            new SymbolStyle {SymbolScale = 1, SymbolOffset = new Offset(0, diameter)},
            new SymbolStyle {SymbolScale = 1, SymbolOffset = new Offset(diameter, diameter)},
            CreateBitmapStyle("Images.ic_place_black_24dp.png", 0.7),
            CreateBitmapStyle("Images.ic_place_black_24dp.png", 0.8),
            CreateBitmapStyle("Images.ic_place_black_24dp.png", 0.9),
            CreateBitmapStyle("Images.ic_place_black_24dp.png", 1.0),
            CreateSvgStyle("Images.Pin.svg", 0.7),
            CreateSvgStyle("Images.Pin.svg", 0.8),
            CreateSvgStyle("Images.Ghostscript_Tiger.svg", 0.05),
            CreateSvgStyle("Images.Ghostscript_Tiger.svg", 0.1),
        };
    }

    private static SymbolStyle CreateBitmapStyle(string embeddedResourcePath, double scale)
    {
        var bitmapId = typeof(SymbolsSample).LoadBitmapId(embeddedResourcePath);
        return new SymbolStyle { BitmapId = bitmapId, SymbolScale = scale, SymbolOffset = new Offset(0, 32) };
    }

    private static SymbolStyle CreateSvgStyle(string embeddedResourcePath, double scale)
    {
        var bitmapId = typeof(SymbolsSample).LoadSvgId(embeddedResourcePath);
        return new SymbolStyle { BitmapId = bitmapId, SymbolScale = scale, SymbolOffset = new RelativeOffset(0.0, 0.5) };
    }

    private static IFeature CreatePointWithStackedStyles()
    {
        var feature = new PointFeature(new MPoint(5000000, -5000000));

        feature.Styles.Add(new SymbolStyle
        {
            SymbolScale = 2.0f,
            Fill = null,
            Outline = new Pen { Color = Color.Yellow }
        });

        feature.Styles.Add(new SymbolStyle
        {
            SymbolScale = 0.8f,
            Fill = new Brush { Color = Color.Red }
        });

        feature.Styles.Add(new SymbolStyle
        {
            SymbolScale = 0.5f,
            Fill = new Brush { Color = Color.Black }
        });

        feature.Styles.Add(new LabelStyle
        {
            Text = "Stacked Styles",
            HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left
        });

        return feature;
    }
}
