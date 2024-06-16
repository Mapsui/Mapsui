using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling.Layers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable IDISP001 // Dispose created
#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class GeoJsonSample : ISample
{
    private static readonly MRect _extent = new(-2066786, 3982655, 3558942, 7705420);

    static GeoJsonSample()
    {
        GeoJsonDeployer.CopyEmbeddedResourceToFile("cities.geojson");
        GeoJsonDeployer.CopyEmbeddedResourceToFile("countries.geojson");
    }

    public string Name => "13 GeoJson";
    public string Category => "Data Formats";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();
        map.Home = (n) => n.ZoomToBox(_extent);
        map.BackColor = new Color(55, 84, 162);
        map.Layers.Add(CountriesLayerBuilder.CreateCountriesLayer());
        map.Layers.Add(CitiesLayerBuilder.CreateCitiesLayer());
        return map;
    }
}

internal static class CountriesLayerBuilder
{
    private static readonly string[] _euroZoneCountries = new string[] {
        "Austria","Belgium", "Croatia", "Cyprus", "Estonia", "Finland", "France", "Germany", "Greece", "Ireland",
        "Italy", "Latvia", "Lithuania", "Luxembourg", "Malta", "the Netherlands", "Portugal", "Slovakia", "Slovenia", "Spain"
    };
    private static readonly VectorStyle _euroStyle = new()
    {
        Fill = new Brush(new Color(245, 245, 242)),
        Outline = new Pen(new Color(55, 84, 162), 2),
    };
    private static readonly VectorStyle _nonEuroStyle = new()
    {
        Fill = new Brush(new Color(86, 109, 176)),
        Outline = new Pen(new Color(55, 84, 162), 2),
    };    

    public static RasterizingTileLayer CreateCountriesLayer() => new(CreateCountriesGeoJsonLayer());

    private static Layer CreateCountriesGeoJsonLayer()
        => new("Countries")
        {
            DataSource = CreateCountriesProvider(),
            Style = new StyleCollection
            {
                Styles =
                {
                    CreateCountriesStyle(),
                    new CalloutStyle()
                }
            }
        };

    private static ProjectingProvider CreateCountriesProvider()
    {
        var path = Path.Combine(GeoJsonDeployer.GeoJsonLocation, "countries.geojson");
        var provider = new GeoJsonProvider(path) { CRS = "EPSG:4326" }; // The ProjectingProvider needs to know the source CRS (EPSG:4326)
        return new ProjectingProvider(provider) { CRS = "EPSG:3857" }; // The ProjectingProvider needs to know the target CRS (EPSG:3857)
    }

    private static IStyle CreateCountriesStyle()
        => new ThemeStyle((f) =>
        {
            // With a ThemeStyle it is possible to create a specific style for each feature
            var name = f["name"] as string ?? throw new Exception("Feature should have name field");
            return IsInEuroZone(name) ? _euroStyle : _nonEuroStyle;
        });

    private static bool IsInEuroZone(string name) => _euroZoneCountries.Contains(name);

}

internal static class CitiesLayerBuilder
{
    public static Layer CreateCitiesLayer() => CreateCitiesLabelLayer();

    private static Layer CreateCitiesLabelLayer()
        => new("Cities labels")
        {
            DataSource = CreateCitiesProvider(),
            Style = CreateCitiesLabelStyle()
        };

    private static ProjectingProvider CreateCitiesProvider()
    {
        var path = Path.Combine(GeoJsonDeployer.GeoJsonLocation, "cities.geojson");
        var provider = new GeoJsonProvider(path) { CRS = "EPSG:4326" }; // The ProjectingProvider needs to know the source CRS (EPSG:4326)
        return new ProjectingProvider(provider) { CRS = "EPSG:3857" }; // The ProjectingProvider needs to know the target CRS (EPSG:3857)
    }

    private static LabelStyle CreateCitiesLabelStyle()
        => new()
        {
            ForeColor = Color.Black,
            BackColor = new Brush(new Color(245, 245, 242)),
            LabelColumn = "city",
        };
}
