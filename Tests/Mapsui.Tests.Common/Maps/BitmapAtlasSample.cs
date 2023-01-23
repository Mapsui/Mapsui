using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using System.Collections.Generic;

namespace Mapsui.Tests.Common.Maps;

public class BitmapAtlasSample : IMapControlSample
{
    public string Name => "Bitmap Atlas";
    public string Category => "Tests";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

    public static Map CreateMap()
    {
        var layer = CreateLayer();

        var map = new Map
        {
            BackColor = Color.FromString("WhiteSmoke"),
            Home = n => n.NavigateTo(new MPoint(256, 200), 1)
        };

        map.Layers.Add(layer);

        return map;
    }

    private static MemoryLayer CreateLayer()
    {
        return new MemoryLayer
        {
            Style = null,
            Features = CreateFeatures(),
            Name = "Points with bitmaps"
        };
    }

    public static List<IFeature> CreateFeatures()
    {
        var atlas = typeof(BitmapAtlasSample).LoadBitmapId("Resources.Images.osm-liberty.png");
        var spriteAmusementPark15 = new Sprite(atlas, 106, 0, 21, 21, 1);
        var spriteClothingStore15 = new Sprite(atlas, 84, 106, 21, 21, 1);
        var spriteDentist15 = new Sprite(atlas, 147, 64, 21, 21, 1);
        var spritePedestrianPolygon = new Sprite(atlas, 0, 0, 64, 64, 1);
        var svgTigerBitmapId = typeof(BitmapAtlasSample).LoadSvgId("Resources.Images.Ghostscript_Tiger.svg");

        return new List<IFeature>
        {
            new PointFeature(new MPoint(256, 124))
            {
                Styles = new[] {new SymbolStyle { BitmapId = atlas} }
            },
            new PointFeature(new MPoint(20, 280))
            {
                Styles = new[] {new SymbolStyle { BitmapId = BitmapRegistry.Instance.Register(spriteAmusementPark15)} }
            },
            new PointFeature(new MPoint(60, 280))
            {
                Styles = new[] {new SymbolStyle { BitmapId = BitmapRegistry.Instance.Register(spriteClothingStore15)} }
            },
            new PointFeature(new MPoint(100, 280))
            {
                Styles = new[] {new SymbolStyle { BitmapId = BitmapRegistry.Instance.Register(spriteDentist15)} }
            },
            new PointFeature(new MPoint(180, 300))
            {
                Styles = new[] {new SymbolStyle { BitmapId = BitmapRegistry.Instance.Register(spritePedestrianPolygon)} }
            },
            new PointFeature(new MPoint(380, 280))
            {
                Styles = new[] {new SymbolStyle { BitmapId = svgTigerBitmapId, SymbolScale = 0.1} }
            }
        };
    }
}
