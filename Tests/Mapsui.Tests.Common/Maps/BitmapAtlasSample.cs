using Mapsui.Layers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
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
        var atlasImageSource = "embeddedresource://Mapsui.Samples.Common.Images.osm-liberty.png";
        var svgTigerImageSource = "embeddedresource://Mapsui.Samples.Common.Images.Ghostscript_Tiger.svg";
        var spriteAmusementPark15 = new Sprite(106, 0, 21, 21);
        var spriteClothingStore15 = new Sprite(84, 106, 21, 21);
        var spriteDentist15 = new Sprite(147, 64, 21, 21);
        var spritePedestrianPolygon = new Sprite(0, 0, 64, 64);

        return new List<IFeature>
        {
            new PointFeature(new MPoint(256, 124))
            {
                Styles = new[] {new SymbolStyle { ImageSource = atlasImageSource} }
            },
            new PointFeature(new MPoint(20, 280))
            {
                Styles = new[] {new SymbolStyle { ImageSource = atlasImageSource, Sprite = spriteAmusementPark15} }
            },
            new PointFeature(new MPoint(60, 280))
            {
                Styles = new[] {new SymbolStyle { ImageSource = atlasImageSource, Sprite = spriteClothingStore15} }
            },
            new PointFeature(new MPoint(100, 280))
            {
                Styles = new[] {new SymbolStyle { ImageSource = atlasImageSource, Sprite = spriteDentist15} }
            },
            new PointFeature(new MPoint(180, 300))
            {
                Styles = new[] {new SymbolStyle { ImageSource = atlasImageSource, Sprite = spritePedestrianPolygon} }
            },
            new PointFeature(new MPoint(380, 280))
            {
                Styles = new[] {new SymbolStyle { ImageSource = svgTigerImageSource, SymbolScale = 0.1} }
            }
        };
    }
}
