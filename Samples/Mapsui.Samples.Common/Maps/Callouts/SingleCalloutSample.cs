using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Samples.Common.Maps.Geometries;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable once ClassNeverInstantiated.Local

namespace Mapsui.Samples.Common.Maps.Callouts;

public class SingleCalloutSample : ISample
{
    public string Name => "1 Single Callout";
    public string Category => "Info";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePointLayer());
        map.Home = n => n.NavigateTo(map.Layers[1].Extent!.Centroid, map.Resolutions[5]);
        map.Info += MapOnInfo;

        return Task.FromResult(map);
    }

    private static void MapOnInfo(object? sender, MapInfoEventArgs e)
    {
        var calloutStyle = e.MapInfo?.Feature?.Styles.Where(s => s is CalloutStyle).Cast<CalloutStyle>().FirstOrDefault();
        if (calloutStyle != null)
        {
            calloutStyle.Enabled = !calloutStyle.Enabled;
            e.MapInfo?.Layer?.DataHasChanged(); // To trigger a refresh of graphics.
        }
    }

    private static MemoryLayer CreatePointLayer()
    {
        return new MemoryLayer
        {
            Name = "Cities with callouts",
            IsMapInfoLayer = true,
            Features = new MemoryProvider(GetCitiesFromEmbeddedResource()).Features,
            Style = SymbolStyles.CreatePinStyle(symbolScale: 0.7),
        };
    }

    private static IEnumerable<IFeature> GetCitiesFromEmbeddedResource()
    {
        var path = "Mapsui.Samples.Common.GeoData.Json.congo.json";
        var assembly = typeof(PointsSample).GetTypeInfo().Assembly;
        using var stream = assembly.GetManifestResourceStream(path);
        var cities = DeserializeFromStream<City>(stream!);

        return cities.Select(c =>
        {
            var feature = new PointFeature(SphericalMercator.FromLonLat(c.Lng, c.Lat).ToMPoint());
            feature[nameof(City.Name)] = c.Name;
            feature[nameof(City.Country)] = c.Country;
            feature[nameof(City.Lat)] = c.Lat;
            feature[nameof(City.Lng)] = c.Lng;
            feature.Styles.Add(CreateCalloutStyle(feature.ToStringOfKeyValuePairs()));
            return feature;
        });
    }

    private static CalloutStyle CreateCalloutStyle(string content)
    {
        return new CalloutStyle
        {
            Title = content,
            TitleFont = { FontFamily = null, Size = 12, Italic = false, Bold = true },
            TitleFontColor = Color.Gray,
            MaxWidth = 120,
            RectRadius = 10,
            ShadowWidth = 4,
            Enabled = false,
            SymbolOffset = new Offset(0, SymbolStyle.DefaultHeight * 1f)
        };
    }

    private class City
    {
        public string? Country { get; set; }
        public string? Name { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public static IEnumerable<T> DeserializeFromStream<T>(Stream stream)
    {
        var serializer = new JsonSerializer();
        using var streamReader = new StreamReader(stream);
        using var jsonTextReader = new JsonTextReader(streamReader);
        return serializer.Deserialize<List<T>>(jsonTextReader) ?? new List<T>();
    }
}
