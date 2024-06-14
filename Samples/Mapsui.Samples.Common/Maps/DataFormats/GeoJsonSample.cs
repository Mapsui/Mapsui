using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling.Layers;
using System;
using System.Collections.Concurrent;
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
        map.Layers.Add(CountriesLayerBuilder.CreateCountriesLayer());
        map.Layers.Add(CitiesLayerBuilder.CreateCitiesLayer());
        return map;
    }  
}

internal static class CountriesLayerBuilder
{
    private static readonly ConcurrentDictionary<string, VectorStyle> _styles = new();
    private static readonly Random _random = new();

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
            return _styles.GetOrAdd(name, (k) =>
                new VectorStyle
                {
                    Fill = new Brush(GenerateRandomColor()),
                    Outline = new Pen(Color.Black, 1),
                });
        });
    

    public static Color GenerateRandomColor()
    {
        byte[] rgb = new byte[3];
        _random.NextBytes(rgb);
        return new Color(rgb[0], rgb[1], rgb[2], 128);
    }
}

internal static class CitiesLayerBuilder
{
    public static RasterizingTileLayer CreateCitiesLayer() => new(CreateCitiesLabelLayer());

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
            BackColor = new Brush(Color.White),
            LabelColumn = "city",
        };
}
