using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets.InfoWidgets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Mapsui.Samples.Common.Maps.Geometries;

public class PointsSample : ISample
{
    public string Name => "Points";
    public string Category => "Geometries";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePointLayer());
        map.Navigator.CenterOnAndZoomTo(map.Layers[1].Extent!.Centroid, map.Navigator.Resolutions[5]);

        map.Widgets.Add(new MapInfoWidget(map));

        return Task.FromResult(map);
    }

    private static MemoryLayer CreatePointLayer()
    {
        return new MemoryLayer
        {
            Name = "Points",
            IsMapInfoLayer = true,
            Features = GetCitiesFromEmbeddedResource(),
            Style = CreateBitmapStyle()
        };
    }

    private static IEnumerable<IFeature> GetCitiesFromEmbeddedResource()
    {
        var path = "Mapsui.Samples.Common.GeoData.Json.congo.json";
        var assembly = typeof(PointsSample).GetTypeInfo().Assembly;
        using var stream = assembly.GetManifestResourceStream(path) ?? throw new InvalidOperationException($"{path} not found");
        var cities = DeserializeFromStream(stream);

        return cities.Select(c =>
        {
            var feature = new PointFeature(SphericalMercator.FromLonLat(c.Lng, c.Lat).ToMPoint());
            feature["name"] = c.Name;
            feature["country"] = c.Country;
            return feature;
        });
    }

    private class City
    {
        public string? Country { get; set; }
        public string? Name { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    private static List<City> DeserializeFromStream(Stream stream)
    {
        using var streamReader = new StreamReader(stream);

        var str = streamReader.ReadToEnd();
        JObject jObject = JObject.Parse(str);
        var cities = jObject["features"]?.Select(c => new City
        {
            Name = c["properties"]?["name"]?.Value<string>(),
            Country = c["properties"]?["country"]?.Value<string>(),
            Lat = c["geometry"]?["coordinates"]?[1]?.Value<double>() ?? 0,
            Lng = c["geometry"]?["coordinates"]?[0]?.Value<double>() ?? 0
        }).ToList();

        return cities ?? [];
    }

    private static SymbolStyle CreateBitmapStyle()
    {
        // For this sample we get the bitmap from an embedded resouce
        // but you could get the data stream from the web or anywhere
        // else.
        var bitmapId = typeof(PointsSample).LoadBitmapId(@"Images.home.png"); // Designed by Freepik http://www.freepik.com
        var bitmapHeight = 176; // To set the offset correct we need to know the bitmap height
        return new SymbolStyle { BitmapId = bitmapId, SymbolScale = 0.20, SymbolOffset = new Offset(0, bitmapHeight * 0.5) };
    }
}
