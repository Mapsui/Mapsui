using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Tests.Common.Maps;

public class BitmapSymbolSample : ISample
{
    public string Name => "Bitmap Symbol";
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
            BackColor = Color.FromString("WhiteSmoke"),
            Home = n => n.ZoomToBox(layer.Extent!.Grow(layer.Extent.Width * 2))
        };

        map.Layers.Add(layer);

        return map;
    }

    public static IEnumerable<IFeature> CreateFeatures()
    {
        var circleIconId = typeof(BitmapSymbolSample).LoadBitmapId("Resources.Images.circle.png");
        var checkeredIconId = typeof(BitmapSymbolSample).LoadBitmapId("Resources.Images.checkered.png");

        return new List<IFeature>
        {
            new PointFeature(new MPoint(50, 50))
            {
                Styles = new[] {new VectorStyle {Fill = new Brush(Color.Red)}}
            },
            new PointFeature(new MPoint(50, 100))
            {
                Styles = new[] {new SymbolStyle { BitmapId = circleIconId}}
            },
            new PointFeature(new MPoint(100, 50))
            {
                Styles = new[] {new SymbolStyle { BitmapId = checkeredIconId}}
            },
            new PointFeature(new MPoint(100, 100))
            {
                Styles = new[] {new VectorStyle {Fill = new Brush(Color.Green), Outline = null}}
            }
        };
    }
}
