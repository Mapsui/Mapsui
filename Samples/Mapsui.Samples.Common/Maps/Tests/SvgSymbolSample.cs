using Mapsui.Layers;
using Mapsui.Styles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Tests;
public class SvgSymbolSample : ISample
{
    public string Name => "SvgSymbol";
    public string Category => "Tests";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

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
            BackColor = Color.WhiteSmoke,
        };

        map.Navigator.ZoomToBox(layer.Extent!.Grow(layer.Extent.Width * 2));

        map.Layers.Add(layer);

        return map;
    }

    public static IEnumerable<IFeature> CreateFeatures() =>
    [
        new PointFeature(new MPoint(50, 50))
        {
            Styles = [CreateSymbolStyle()]
        },
        new PointFeature(new MPoint(50, 100))
        {
            Styles = [CreateSymbolStyle()]
        },
        new PointFeature(new MPoint(100, 50))
        {
            Styles = [CreateSymbolStyle()]
        },
        new PointFeature(new MPoint(100, 100))
        {
            Styles = [CreateSymbolStyle()]
        }
    ];

    private static ImageStyle CreateSymbolStyle() => new()
    {
        Image = new Image
        {
            Source = "embedded://mapsui.resources.images.pin.svg",
            SvgFillColor = Color.FromRgba(0, 177, 0, 255),
            SvgStrokeColor = Color.FromRgba(32, 96, 32, 255),
        }
    };
}
