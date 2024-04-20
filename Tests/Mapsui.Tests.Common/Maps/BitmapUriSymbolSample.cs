using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Samples.Common;
using Mapsui.Styles;

namespace Mapsui.Tests.Common.Maps;

public class BitmapUriSymbolSample : ISample
{
    public string Name => "Bitmap Uri Symbol";
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
        var circleIconPath = typeof(BitmapUriSymbolSample).LoadBitmapPath("Resources.Images.circle.png");
        var circleIconId = await BitmapRegistry.Instance.RegisterAsync(circleIconPath);
        var checkeredIconPath = typeof(BitmapUriSymbolSample).LoadBitmapPath("Resources.Images.checkered.png");
        var checkeredIconId = await BitmapRegistry.Instance.RegisterAsync(checkeredIconPath);

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
