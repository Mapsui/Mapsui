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

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable once ClassNeverInstantiated.Local
#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Samples.Common.Maps.Info;

public class CustomCalloutSample : ISample
{
    private static readonly Random _random = new(1);

    public string Name => "Custom Callout";
    public string Category => "Info";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePointLayer());
        map.Navigator.CenterOnAndZoomTo(map.Layers[1].Extent!.Centroid, map.Navigator.Resolutions[5]);

        map.Widgets.Add(new MapInfoWidget(map));

        return Task.FromResult(map);
    }

    private static Layer CreatePointLayer()
    {
        return new Layer
        {
            Name = "Point",
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

    private static IStyle CreateCalloutStyle(string ImageSource)
    {
        var calloutStyle = new CalloutStyle { ImageSource = ImageSource, ArrowPosition = _random.Next(1, 9) * 0.1f, RotateWithMap = true, Type = CalloutType.Image };
        switch (_random.Next(0, 4))
        {
            case 0:
                calloutStyle.ArrowAlignment = ArrowAlignment.Bottom;
                calloutStyle.Offset = new Offset(0, SymbolStyle.DefaultHeight * 0.5f);
                break;
            case 1:
                calloutStyle.ArrowAlignment = ArrowAlignment.Left;
                calloutStyle.Offset = new Offset(SymbolStyle.DefaultHeight * 0.5f, 0);
                break;
            case 2:
                calloutStyle.ArrowAlignment = ArrowAlignment.Top;
                calloutStyle.Offset = new Offset(0, -SymbolStyle.DefaultHeight * 0.5f);
                break;
            case 3:
                calloutStyle.ArrowAlignment = ArrowAlignment.Right;
                calloutStyle.Offset = new Offset(-SymbolStyle.DefaultHeight * 0.5f, 0);
                break;
        }
        calloutStyle.RectRadius = 10; // Random.Next(0, 9);
        calloutStyle.ShadowWidth = 4; // Random.Next(0, 9);
        calloutStyle.StrokeWidth = 0;
        return calloutStyle;
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
        return JsonSerializer.Deserialize(stream, CustomCalloutSampleContext.Default.ListCity) ?? [];
    }
}

[JsonSerializable(typeof(List<CustomCalloutSample.City>))]
internal partial class CustomCalloutSampleContext : JsonSerializerContext
{
}
