using Mapsui.Layers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using System.IO;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Performance;

public class BigShapefilesSample : ISample
{
    static BigShapefilesSample()
    {
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("EZG_KB_LM.shp");
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("modell_ezgs_v02_ohneTalsperren_EPSG3857.shp");
    }

    public string Name => "BigShapesfiles";
    public string Category => "Performance";

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map();

        var tileLayer = OpenStreetMap.CreateTileLayer();
        var shapeLayer1 = await CreateShapeLayerAsync("EZG_KB_LM.shp");
        var shapeLayer2 = await CreateShapeLayerAsync("modell_ezgs_v02_ohneTalsperren_EPSG3857.shp");

        map.Layers.Add(tileLayer);
        map.Layers.Add(shapeLayer1);
        map.Layers.Add(shapeLayer2);

        return map;
    }

    private static async Task<ILayer[]> CreateShapeLayerAsync(string shapeName)
    {
        var fileLocation = Path.Combine(ShapeFilesDeployer.ShapeFilesLocation, shapeName);
        using var shapeFile = new Nts.Providers.Shapefile.ShapeFile(fileLocation, false)
        {
            CRS = "EPSG:3857"
        };

        using var blackLayer = new MemoryLayer
        {
            Name = shapeName,
            Features = await shapeFile.GetFeaturesAsync(new FetchInfo(new MSection(shapeFile.GetExtent()!, 156543), "EPSG:3857", ChangeType.Discrete)),
            Style = CreateBlackStyle(),
        };

        using var yellowLayer = new MemoryLayer
        {
            Name = shapeName,
            Features = await shapeFile.GetFeaturesAsync(new FetchInfo(new MSection(shapeFile.GetExtent()!, 156543), "EPSG:3857", ChangeType.Discrete)),
            Style = CreateYellowStyle(),
        };

        return [new RasterizingTileLayer(blackLayer), new RasterizingLayer(yellowLayer)];
    }

    private static VectorStyle CreateBlackStyle() => new()
    {
        Fill = null,
        Outline = new Pen
        {
            Color = Color.Black,
            Width = 3
        }
    };

    private static VectorStyle CreateYellowStyle() => new()
    {
        Fill = null,
        Outline = new Pen
        {
            Color = Color.DarkOrange,
            Width = 1
        }
    };
}
