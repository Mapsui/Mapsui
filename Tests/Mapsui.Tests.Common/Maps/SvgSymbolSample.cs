using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using System.Collections.Generic;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Tests.Common.Maps;

public class SvgSymbolSample : IMapControlSample
{
    public string Name => "Svg Symbol";
    public string Category => "Tests";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

    public static Map CreateMap()
    {
        var layer = new MemoryLayer
        {
            Style = null,
            Features = CreateFeatures(),
            Name = "Points with Svg"
        };

        var map = new Map
        {
            BackColor = Color.FromString("WhiteSmoke"),
            Home = n => n.NavigateTo(layer.Extent!.Grow(layer.Extent.Width * 2))
        };

        map.Layers.Add(layer);

        return map;
    }

    public static IEnumerable<IFeature> CreateFeatures()
    {
        var pinId = typeof(SvgSymbolSample).LoadSvgId("Resources.Images.Pin.svg");

        return new List<IFeature>
        {
            new PointFeature(new MPoint(50, 50)) {
                Styles = new[] {new SymbolStyle { BitmapId = pinId } }
            },
            new PointFeature(new MPoint(50, 100)) {
                Styles = new[] {new SymbolStyle { BitmapId = pinId } }
            },
            new PointFeature(new MPoint(100, 50)) {
                Styles = new[] {new SymbolStyle { BitmapId = pinId } }
            },
            new PointFeature(new MPoint(100, 100)) {
                Styles = new[] {new SymbolStyle { BitmapId = pinId } }
            }
        };
    }
}
