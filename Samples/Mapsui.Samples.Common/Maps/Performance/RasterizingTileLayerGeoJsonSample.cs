using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using System.IO;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class RasterizingTileLayerGeoJsonSample : ISample
{
    static RasterizingTileLayerGeoJsonSample()
    {
        GeoJsonDeployer.CopyEmbeddedResourceToFile("cities.geojson");
    }

    public string Name => "RasterizingTileLayerWithGeoJsonVectorStyle";
    public string Category => "Performance";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map
        {
            CRS = "EPSG:3857", // The Map CRS needs to be set   
        };

        var examplePath = Path.Combine(GeoJsonDeployer.GeoJsonLocation, "cities.geojson");
        var geoJson = new GeoJsonProvider(examplePath)
        {
            CRS = "EPSG:4326" // The DataSource CRS needs to be set
        };

        var dataSource = new ProjectingProvider(geoJson)
        {
            CRS = "EPSG:3857",
        };

        map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        map.Layers.Add(new RasterizingTileLayer(CreateCityVectorLayer(dataSource)));

        return map;
    }

    private static ILayer CreateCityVectorLayer(IProvider citiesProvider)
    {
        return new Layer("City labels")
        {
            DataSource = citiesProvider,
            Enabled = true,
            Style = CreateCityStyle()
        };
    }

    private static VectorStyle CreateCityStyle()
    {
        return new VectorStyle
        {
            Fill = new Brush(Color.White),
            Outline = new Pen(Color.Black, width: 10),
            Line = new Pen(Color.Black, width: 10),
        };
    }
}
