using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Samples.Common.Maps.Geometries;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets.InfoWidgets;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Info;

public class SingleCalloutSample : ISample
{
    public string Name => "Single Callout";
    public string Category => "Info";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePointLayer());
        map.Navigator.CenterOnAndZoomTo(map.Layers.Get(1).Extent!.Centroid, map.Navigator.Resolutions[5]);
        map.Info += MapOnInfo;

        map.Widgets.Add(new MapInfoWidget(map));

        return Task.FromResult(map);
    }

    private static void MapOnInfo(object? sender, MapInfoEventArgs e)
    {
        var calloutStyle = e.MapInfo?.Feature?.Styles.OfType<CalloutStyle>().FirstOrDefault();
        if (calloutStyle is not null)
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
        var cities = DeserializeFromStream(stream!);

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
            Enabled = false,
            SymbolOffset = new Offset(0, SymbolStyle.DefaultHeight * 1f),
            BalloonDefinition = new CalloutBalloonDefinition
            {
                RectRadius = 10,
                ShadowWidth = 4,
            },
        };
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
        return JsonSerializer.Deserialize(stream, SingleCalloutContext.Default.ListCity) ?? [];
    }
}

[JsonSerializable(typeof(List<SingleCalloutSample.City>))]
internal partial class SingleCalloutContext : JsonSerializerContext
{
}
