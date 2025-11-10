using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using System.IO;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Performance;

public class HugeShapeFileWithSimplifierAndClippingSample : ISample
{
    static HugeShapeFileWithSimplifierAndClippingSample()
    {
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("EZG_KB_LM.shp");
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("modell_ezgs_v02_ohneTalsperren_EPSG3857.shp");
    }

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public string Name => "Huge Shape File With Simplifier and Clipping";
    public string Category => "Performance";

    public static Map CreateMap()
    {
        var map = new Map();

        var tileLayer = OpenStreetMap.CreateTileLayer();
        var shapeLayer1 = CreateShapeLayer("EZG_KB_LM.shp", "cache1");
        var shapeLayer2 = CreateShapeLayer("modell_ezgs_v02_ohneTalsperren_EPSG3857.shp", "cache2");

        map.Layers.Add(tileLayer);
        map.Layers.Add(shapeLayer1);
        map.Layers.Add(shapeLayer2);

        return map;
    }

    private static ILayer CreateShapeLayer(string shapeName, string cacheName)
    {
        using var shapeFile = new Nts.Providers.Shapefile.ShapeFile(
           Path.Combine(ShapeFilesDeployer.ShapeFilesLocation, shapeName), false)
        { CRS = "EPSG:3857" };

        //option 1: with clipping
        //var provider = new GeometrySimplifyAndClippingProvider(shapeFile);

        //option 2: without clipping
        var provider = new GeometrySimplifyProvider(shapeFile);

        //var sqlitePersistentCache = new SqlitePersistentCache(cacheName);
        //sqlitePersistentCache.Clear();

        using var layer = new Layer
        {
            Name = shapeName,
            DataSource = provider,
            Style = CreateVectorThemeStyle(),
        };

        //return layer;
        return new RasterizingLayer(layer);
        //return new RasterizingTileLayer(layer) { Enabled = false }; //really slow
        //return new RasterizingTileLayer(layer, persistentCache: sqlitePersistentCache) { Enabled = false }; //really slow
    }

    private static IThemeStyle CreateVectorThemeStyle()
    {
        var style = new VectorStyle()
        {
            Fill = new Brush(Color.Transparent),
            Line = new Pen
            {
                Color = Color.Black,
                Width = 2
            },
            //Opacity = vectorTheme.Opacity,
            Outline = new Pen
            {
                Color = Color.Black,
                Width = 2
            }
        };

        return new ThemeStyle(f =>
        {
            return style;
        });
    }
}
