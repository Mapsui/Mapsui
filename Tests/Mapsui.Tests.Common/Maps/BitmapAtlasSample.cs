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
#pragma warning disable IDISP001 // Dispose created
        var layer = CreateLayer();
#pragma warning restore IDISP001 // Dispose created

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
        var atlasImageSource = "embedded://Mapsui.Samples.Common.Images.osm-liberty.png";
        var svgTigerImageSource = "embedded://Mapsui.Samples.Common.Images.Ghostscript_Tiger.svg";
        var spriteAmusementPark15 = new BitmapRegion(106, 0, 21, 21);
        var spriteClothingStore15 = new BitmapRegion(84, 106, 21, 21);
        var spriteDentist15 = new BitmapRegion(147, 64, 21, 21);
        var spritePedestrianPolygon = new BitmapRegion(0, 0, 64, 64);

        return
        [
            new PointFeature(new MPoint(256, 124))
            {
                Styles = [new SymbolStyle { Image = new ResourceImage { Source = atlasImageSource } }]
            },
            new PointFeature(new MPoint(20, 280))
            {
                Styles = [new SymbolStyle { Image = new ResourceImage { Source = atlasImageSource, BitmapRegion = spriteAmusementPark15 } }]
            },
            new PointFeature(new MPoint(60, 280))
            {
                Styles = [new SymbolStyle {Image = new ResourceImage { Source = atlasImageSource, BitmapRegion = spriteClothingStore15 } }]
            },
            new PointFeature(new MPoint(100, 280))
            {
                Styles = [new SymbolStyle {Image = new ResourceImage { Source = atlasImageSource, BitmapRegion = spriteDentist15 } }]
            },
            new PointFeature(new MPoint(180, 300))
            {
                Styles = [new SymbolStyle {Image = new ResourceImage { Source = atlasImageSource, BitmapRegion = spritePedestrianPolygon } }]
            },
            new PointFeature(new MPoint(380, 280))
            {
                Styles = [new SymbolStyle { Image = new ResourceImage { Source = svgTigerImageSource }, SymbolScale = 0.1}]
            }
        ];
    }
}
