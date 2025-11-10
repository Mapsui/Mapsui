using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps.Tests;

public class BitmapSymbolSample : ISample
{
    public string Name => "BitmapSymbol";
    public string Category => "Tests";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var layer = new MemoryLayer
        {
            Style = null,
            Features = CreateFeatures(),
            Name = "Points with bitmaps"
        };

        var map = new Map
        {
            BackColor = Color.WhiteSmoke,
        };

        map.Navigator.ZoomToBox(layer.Extent!.Grow(layer.Extent.Width * 2));

        map.Layers.Add(layer);

        return map;
    }

    public static IEnumerable<IFeature> CreateFeatures()
    {
        var circleImageSource = "embedded://Mapsui.Samples.Common.Images.circle.png";
        var checkeredImageSource = "embedded://Mapsui.Samples.Common.Images.checkered.png";

        return
        [
            new PointFeature(new MPoint(50, 50))
            {
                Styles = [new VectorStyle { Fill = new Brush(Color.Red) }]
            },
            new PointFeature(new MPoint(50, 100))
            {
                Styles = [new ImageStyle { Image = circleImageSource }]
            },
            new PointFeature(new MPoint(100, 50))
            {
                Styles = [new ImageStyle { Image = checkeredImageSource }]
            },
            new PointFeature(new MPoint(100, 100))
            {
                Styles = [new VectorStyle { Fill = new Brush(Color.Green), Outline = null }]
            }
        ];
    }
}
