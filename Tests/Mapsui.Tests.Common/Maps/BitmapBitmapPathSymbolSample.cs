using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Samples.Common;
using Mapsui.Styles;

namespace Mapsui.Tests.Common.Maps;

public class BitmapBitmapPathSymbolSample : ISample
{
    public string Name => "Bitmap BitmapPath Symbol";
    public string Category => "Tests";

    public async Task<Map> CreateMapAsync()
    {
        var layer = new MemoryLayer
        {
            Style = null,
            Features = await CreateFeaturesAsync(),
            Name = "Points with Uri bitmaps"
        };

        var map = new Map
        {
            BackColor = Color.WhiteSmoke,
        };

        map.Navigator.ZoomToBox(layer.Extent!.Grow(layer.Extent.Width * 2));

        map.Layers.Add(layer);

        return map;
    }

    public static async Task<IEnumerable<IFeature>> CreateFeaturesAsync()
    {
        var circleIconPath = typeof(BitmapBitmapPathSymbolSample).LoadBitmapPath("Resources.Images.circle.png");
        var checkeredIconPath = typeof(BitmapBitmapPathSymbolSample).LoadBitmapPath("Resources.Images.checkered.png");

        return new List<IFeature>
        {
            new PointFeature(new MPoint(50, 50))
            {
                Styles = new[] {new VectorStyle {Fill = new Brush(Color.Red)}}
            },
            new PointFeature(new MPoint(50, 100))
            {
                Styles = new[] {new SymbolStyle { BitmapPath = circleIconPath}}
            },
            new PointFeature(new MPoint(100, 50))
            {
                Styles = new[] {new SymbolStyle { BitmapPath = checkeredIconPath}}
            },
            new PointFeature(new MPoint(100, 100))
            {
                Styles = new[] {new VectorStyle {Fill = new Brush(Color.Green), Outline = null}}
            }
        };
    }
}
