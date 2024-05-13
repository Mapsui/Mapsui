using Mapsui.Layers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Tests.Common.Maps;

public class BitmapAtlasSample : ISample
{
    public string Name => "Bitmap Atlas";
    public string Category => "Tests";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());


    public static Map CreateMap()
    {
        var layer = CreateLayer();

        var map = new Map
        {
            BackColor = Color.WhiteSmoke,
        };

        map.Navigator.CenterOnAndZoomTo(new MPoint(256, 200), 1);

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
        var atlasBitmapPath = new Uri("embeddedresource://mapsui.tests.common.resources.Images.osm-liberty.png");
        var spriteAmusementPark15 = new Sprite(106, 0, 21, 21, 1);
        var spriteClothingStore15 = new Sprite(84, 106, 21, 21, 1);
        var spriteDentist15 = new Sprite(147, 64, 21, 21, 1);
        var spritePedestrianPolygon = new Sprite(0, 0, 64, 64, 1);
        var svgTigerBitmapId = typeof(BitmapAtlasSample).LoadSvgId("Resources.Images.Ghostscript_Tiger.svg");

        return new List<IFeature>
        {
            new PointFeature(new MPoint(256, 124))
            {
                Styles = new[] {new SymbolStyle { BitmapPath = atlasBitmapPath} }
            },
            new PointFeature(new MPoint(20, 280))
            {
                Styles = new[] {new SymbolStyle { BitmapPath = atlasBitmapPath, Sprite = spriteAmusementPark15} }
            },
            new PointFeature(new MPoint(60, 280))
            {
                Styles = new[] {new SymbolStyle { BitmapPath = atlasBitmapPath, Sprite = spriteClothingStore15} }
            },
            new PointFeature(new MPoint(100, 280))
            {
                Styles = new[] {new SymbolStyle { BitmapPath = atlasBitmapPath, Sprite = spriteDentist15} }
            },
            new PointFeature(new MPoint(180, 300))
            {
                Styles = new[] {new SymbolStyle { BitmapPath = atlasBitmapPath, Sprite = spritePedestrianPolygon} }
            },
            new PointFeature(new MPoint(380, 280))
            {
                Styles = new[] {new SymbolStyle { BitmapId = svgTigerBitmapId, SymbolScale = 0.1} }
            }
        };
    }
}
