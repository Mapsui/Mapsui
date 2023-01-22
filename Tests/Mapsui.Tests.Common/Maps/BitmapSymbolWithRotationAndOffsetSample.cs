using System.Collections.Generic;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;
using NetTopologySuite.Geometries;

namespace Mapsui.Tests.Common.Maps;

public class BitmapSymbolWithRotationAndOffsetSample : IMapControlSample
{
    public string Name => "Symbol rotation and offset";
    public string Category => "Tests";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

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
            BackColor = Color.FromString("WhiteSmoke"),
            Home = n => n.NavigateTo(layer.Extent!.Grow(layer.Extent.Width * 2))
        };

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
        var bitmapId = typeof(BitmapSymbolWithRotationAndOffsetSample).LoadBitmapId("Resources.Images.iconthatneedsoffset.png");

        var feature = new GeometryFeature { Geometry = new Point(x, y) };

        feature.Styles.Add(new SymbolStyle
        {
            BitmapId = bitmapId,
            SymbolOffset = new Offset { Y = -24 },
            SymbolRotation = rotation,
            RotateWithMap = true,
        });
        return feature;
    }
}
