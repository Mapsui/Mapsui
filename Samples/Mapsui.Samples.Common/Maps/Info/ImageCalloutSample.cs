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

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Info;

public class ImageCalloutSample : ISample
{
    private static readonly Random _random = new(1);

    public string Name => "Image Callout";
    public string Category => "Info";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePointLayer());
        map.Navigator.CenterOnAndZoomTo(map.Layers.Get(1).Extent!.Centroid, map.Navigator.Resolutions[5]);

        map.Widgets.Add(new MapInfoWidget(map));
        map.Info += MapOnInfo;

        return Task.FromResult(map);
    }

    private static void MapOnInfo(object? sender, MapInfoEventArgs e)
    {
        var calloutStyle = e.MapInfo?.Feature?.Styles.OfType<CalloutStyle>().FirstOrDefault();
        if (calloutStyle is not null)
        {
            calloutStyle.Enabled = !calloutStyle.Enabled;
            e.MapInfo?.Layer?.DataHasChanged();
        }
    }

    private static Layer CreatePointLayer()
    {
        return new Layer
        {
            Name = "Point with callout",
            DataSource = new MemoryProvider(GetCitiesFromEmbeddedResource()),
            IsMapInfoLayer = true
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
        });
    }

    private static CalloutStyle CreateCalloutStyle(string imageSource) => new()
    {
        BalloonDefinition = CreateBalloonDefinition(),
        ImageSource = imageSource,
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
