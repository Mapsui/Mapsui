using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Samples.Common.Maps.Geometries;
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

#pragma warning disable IDISP001 // Dispose created
#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Info;

public class ImageCalloutSample : ISample
{
    private static readonly Random _random = new(1);

    public string Name => "Image Callout";
    public string Category => "MapInfo";

    private const string _pointLayerName = "Point with callout";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    private static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePointLayer());
        map.Navigator.CenterOnAndZoomTo(map.Layers.Get(1).Extent!.Centroid, map.Navigator.Resolutions[5]);
        map.Widgets.Add(new MapInfoWidget(map, l => l.Name == _pointLayerName));
        map.Tapped += MapTapped;
        return map;
    }

    private static bool MapTapped(Map map, MapEventArgs e)
    {
        var mapInfo = e.GetMapInfo(map.Layers.Where(l => l.Name == _pointLayerName));
        var calloutStyle = mapInfo.Feature?.Styles.OfType<CalloutStyle>().FirstOrDefault();
        if (calloutStyle is not null)
        {
            calloutStyle.Enabled = !calloutStyle.Enabled;
            mapInfo.Layer?.DataHasChanged();
            return true;
        }
        return false;
    }

    private static Layer CreatePointLayer()
    {
        return new Layer
        {
            Name = _pointLayerName,
            DataSource = new MemoryProvider(GetCitiesFromEmbeddedResource()),
        };
    }

    private static IEnumerable<IFeature> GetCitiesFromEmbeddedResource()
    {
        const string path = "Mapsui.Samples.Common.GeoData.Json.congo.json";
        var assembly = typeof(PointsSample).GetTypeInfo().Assembly;
        using var stream = assembly.GetManifestResourceStream(path) ?? throw new NullReferenceException();
        var cities = DeserializeFromStream(stream);

        return cities.Select(c =>
        {
            var feature = new PointFeature(SphericalMercator.FromLonLat(c.Lng, c.Lat).ToMPoint());
            feature["name"] = c.Name;
            feature["country"] = c.Country;

            var calloutStyle = CreateCalloutStyle("embedded://Mapsui.Samples.Common.Images.loc.png");
            feature.Styles.Add(calloutStyle);
            return feature;
        }).ToArray();
    }

    private static CalloutStyle CreateCalloutStyle(string imageSource) => new()
    {
        BalloonDefinition = CreateBalloonDefinition(),
        Image = imageSource,
        Type = CalloutType.Image,
        Enabled = false
    };

    private static CalloutBalloonDefinition CreateBalloonDefinition()
    {
        var tailAlignment = _random.Next(0, 4);
        return new CalloutBalloonDefinition
        {
            TailPosition = _random.Next(1, 9) * 0.1f,
            RectRadius = 10,
            ShadowWidth = 4,
            StrokeWidth = 0,
            TailAlignment = GetTailAlignment(tailAlignment),
            Offset = GetOffset(tailAlignment),
        };
    }
    private static Offset GetOffset(int tailAlignment) => tailAlignment switch
    {
        0 => new Offset(0, SymbolStyle.DefaultHeight * 0.5f),
        1 => new Offset(SymbolStyle.DefaultHeight * 0.5f, 0),
        2 => new Offset(0, -SymbolStyle.DefaultHeight * 0.5f),
        3 => new Offset(-SymbolStyle.DefaultHeight * 0.5f, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(tailAlignment)),
    };

    private static TailAlignment GetTailAlignment(int tailAlignment) => tailAlignment switch
    {
        0 => TailAlignment.Bottom,
        1 => TailAlignment.Left,
        2 => TailAlignment.Top,
        3 => TailAlignment.Right,
        _ => throw new ArgumentOutOfRangeException(nameof(tailAlignment)),
    };

    internal class City
    {
        public string? Country { get; set; }
        public string? Name { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    private static List<City> DeserializeFromStream(Stream stream)
    {
        return JsonSerializer.Deserialize(stream, ImageCalloutSampleContext.Default.ListCity) ?? [];
    }
}

[JsonSerializable(typeof(List<ImageCalloutSample.City>))]
internal partial class ImageCalloutSampleContext : JsonSerializerContext
{
}
