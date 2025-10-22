﻿using Mapsui.Extensions;
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

namespace Mapsui.Samples.Common.Maps.MapInfo;

public class SingleCalloutSample : ISample
{
    public string Name => "SingleCallout";
    public string Category => "MapInfo";

    private const string _calloutLayerName = "Cities with callouts";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePointLayer());
        map.Navigator.CenterOnAndZoomTo(map.Layers.Get(1).Extent!.Centroid, map.Navigator.Resolutions[5]);
        map.Tapped += MapTapped;

        map.Widgets.Add(new MapInfoWidget(map, l => l.Name == _calloutLayerName));

        return Task.FromResult(map);
    }

    private static void MapTapped(object? s, MapEventArgs e)
    {
        var mapInfo = e.GetMapInfo(e.Map.Layers.Where(l => l.Name == _calloutLayerName));
        var calloutStyle = mapInfo.Feature?.Styles.OfType<CalloutStyle>().FirstOrDefault();
        if (calloutStyle is not null)
        {
            calloutStyle.Enabled = !calloutStyle.Enabled;
            mapInfo.Layer?.DataHasChanged(); // To trigger a refresh of graphics.
            e.Handled = true;
        }
    }

    private static MemoryLayer CreatePointLayer()
    {
        return new MemoryLayer
        {
            Name = _calloutLayerName,
            Features = new MemoryProvider(GetCitiesFromEmbeddedResource()).Features,
            Style = ImageStyles.CreatePinStyle(symbolScale: 0.7),
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
        }).ToArray();
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
            Offset = new Offset(0, SymbolStyle.DefaultHeight * 1f),
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
