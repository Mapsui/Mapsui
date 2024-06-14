using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using System.IO;
using System.Threading.Tasks;

#pragma warning disable IDISP001 // Dispose created
#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class GeoJsonSample : ISample
{
    static GeoJsonSample()
    {
        GeoJsonDeployer.CopyEmbeddedResourceToFile("cities.geojson");
        GeoJsonDeployer.CopyEmbeddedResourceToFile("countries.geojson");
    }

    public string Name => "1";
    public string Category => "1";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(Tiling.OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateCitiesLayer());
        return map;
    }

    private static RasterizingTileLayer CreateCitiesLayer()
    {
        return new RasterizingTileLayer(CreateCityLabelLayer());
    }

    private static ProjectingProvider CreateCitiesProvider()
    {
        var path = Path.Combine(GeoJsonDeployer.GeoJsonLocation, "cities.geojson");
        var provider = new GeoJsonProvider(path) { CRS = "EPSG:4326" }; // The ProjectingProvider needs to know the source CRS (EPSG:4326)
        return new ProjectingProvider(provider) { CRS = "EPSG:3857" }; // The ProjectingProvider needs to know the target CRS (EPSG:3857)
    }

    private static Layer CreateCityLabelLayer()
        => new("City labels")
        {
            DataSource = CreateCitiesProvider(),
            Style = CreateCityLabelStyle()
        };

    private static LabelStyle CreateCityLabelStyle()
        => new()
        {
            ForeColor = Color.Black,
            BackColor = new Brush(Color.White),
            LabelColumn = "city",
        };
}
