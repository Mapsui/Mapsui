using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets.InfoWidgets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

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
        map.Navigator.CenterOnAndZoomTo(map.Layers.Get(1).Extent!.Centroid, map.Navigator.Resolutions[5]);

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

    internal class City
    {
        public string? Country { get; set; }
        public string? Name { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    private static List<City> DeserializeFromStream(Stream stream)
    {
        return JsonSerializer.Deserialize(stream, PointsSampleContext.Default.ListCity) ?? [];
    }

    private static SymbolStyle CreateBitmapStyle()
    {
        var imageSource = "embedded://Mapsui.Samples.Common.Images.home.png"; // Designed by Freepik http://www.freepik.com
        var bitmapHeight = 176; // To set the offset correct we need to know the bitmap height
        return new SymbolStyle { ImageSource = imageSource, SymbolScale = 0.20, SymbolOffset = new Offset(0, bitmapHeight * 0.5) };
    }
}

[JsonSerializable(typeof(List<PointsSample.City>))]
internal partial class PointsSampleContext : JsonSerializerContext
{
}
