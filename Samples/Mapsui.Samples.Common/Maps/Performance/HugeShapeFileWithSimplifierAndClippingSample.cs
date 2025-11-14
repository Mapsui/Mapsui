using Mapsui.Layers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
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

    public string Name => "Huge Shape File With Simplifier and Clipping";
    public string Category => "1";

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map();

        var tileLayer = OpenStreetMap.CreateTileLayer();
        var shapeLayer1 = await CreateShapeLayerAsync("EZG_KB_LM.shp", "cache1");
        var shapeLayer2 = await CreateShapeLayerAsync("modell_ezgs_v02_ohneTalsperren_EPSG3857.shp", "cache2");

        map.Layers.Add(tileLayer);
        map.Layers.Add(shapeLayer1);
        map.Layers.Add(shapeLayer2);

        return map;
    }

    private static async Task<ILayer[]> CreateShapeLayerAsync(string shapeName, string cacheName)
    {
        using var shapeFile = new Nts.Providers.Shapefile.ShapeFile(
           Path.Combine(ShapeFilesDeployer.ShapeFilesLocation, shapeName), false)
        { CRS = "EPSG:3857" };

        using var layer = new MemoryLayer
        {
            Name = shapeName,
            Features = await shapeFile.GetFeaturesAsync(new FetchInfo(new MSection(shapeFile.GetExtent()!, 156543), "EPSG:3857", ChangeType.Discrete)),
            Style = CreateVectorThemeStyle(),
        };

        return [new RasterizingLayer(layer), new RasterizingTileLayer(layer)];
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
