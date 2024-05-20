using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using NetTopologySuite.Geometries;

namespace Mapsui.Tests.Common.Maps;

public class BitmapSymbolWithRotationAndOffsetSample : ISample
{
    public string Name => "Symbol rotation and offset";
    public string Category => "Tests";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());


    public static Map CreateMap()
    {
        var layer = new MemoryLayer
        {
            Features = CreateProviderWithRotatedBitmapSymbols(),
            Name = "Points with rotated bitmaps",
            Style = null
        };

        var map = new Map
        {
            BackColor = Color.WhiteSmoke,
        };

        map.Navigator.ZoomToBox(layer.Extent!.Grow(layer.Extent.Width * 2));

        map.Layers.Add(layer);

        return map;
    }

    private static IEnumerable<IFeature> CreateProviderWithRotatedBitmapSymbols()
    {
        return new List<IFeature>
        {
            new GeometryFeature
            {
                Geometry = new Point(75, 75),
                Styles = new[] {new SymbolStyle {Fill = new Brush(Color.Red)}}
            }, // for reference
            CreateFeatureWithRotatedBitmapSymbol(75, 125, 90),
            CreateFeatureWithRotatedBitmapSymbol(125, 125, 180),
            CreateFeatureWithRotatedBitmapSymbol(125, 75, 270)
        };
    }

    private static GeometryFeature CreateFeatureWithRotatedBitmapSymbol(double x, double y, double rotation)
    {
        var imageSource = "embedded://Mapsui.Tests.Common.Resources.Images.iconthatneedsoffset.png";

        var feature = new GeometryFeature { Geometry = new Point(x, y) };

        feature.Styles.Add(new SymbolStyle
        {
            ImageSource = imageSource,
            SymbolOffset = new Offset { Y = -24 },
            SymbolRotation = rotation,
            RotateWithMap = true,
        });
        return feature;
    }
}
