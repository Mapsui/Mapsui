using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;

namespace Mapsui.Tests.Common.Maps;

public class SymbolTypesSample : IMapControlSample
{
    public string Name => "Symbol Types";
    public string Category => "Tests";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001", MessageId = "Dispose created.")]
    public static Map CreateMap()
    {
        var layer = CreateLayer();

        var map = new Map
        {
            BackColor = Color.FromString("WhiteSmoke"),
            Home = n => n.NavigateTo(layer.Extent!.Grow(layer.Extent.Width * 2))
        };

        map.Layers.Add(layer);

        return map;
    }

    private static MemoryLayer CreateLayer()
    {
        return new MemoryLayer
        {
            Features = CreateFeatures(),
            Name = "Symbol Types",
            Style = null
        };
    }

    private static IEnumerable<IFeature> CreateFeatures()
    {
        return new List<IFeature>()
        {
            new PointFeature(new MPoint(0, 00)) {
                Styles = new List<IStyle>
                {
                    new SymbolStyle
                    {
                        Fill = new Brush {Color = Color.Gray},
                        Outline = new Pen(Color.Black),
                        SymbolType = SymbolType.Ellipse
                    }
                }
            },
            new PointFeature(new MPoint(50, 0)) {
                Styles = new List<IStyle>
                {
                    new SymbolStyle
                    {
                        Fill = new Brush {Color = Color.Gray},
                        Outline = new Pen(Color.Black),
                        SymbolType = SymbolType.Rectangle
                    }
                }
            },
            new PointFeature(new MPoint(0, 50)) {
                Styles = new List<IStyle>
                {
                    new SymbolStyle
                    {
                        Fill = new Brush {Color = Color.Gray},
                        Outline = new Pen(Color.Black),
                        SymbolType = SymbolType.Triangle
                    }
                }
            }
        };
    }
}
