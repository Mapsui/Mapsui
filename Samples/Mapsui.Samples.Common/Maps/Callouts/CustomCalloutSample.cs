using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Samples.Common.Maps.Geometries;
using Mapsui.Styles;
using Mapsui.Tiling;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable once ClassNeverInstantiated.Local
#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Samples.Common.Maps.Callouts;

public class CustomCalloutSample : ISample
{
    private static readonly Random Random = new(1);

    public string Name => "2 Custom Callout";
    public string Category => "Info";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePointLayer());
        map.Home = n => n.NavigateTo(map.Layers[1].Extent!.Centroid, map.Resolutions[5]);
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
        var cities = DeserializeFromStream<City>(stream);

        return cities.Select(c =>
        {
            var feature = new PointFeature(SphericalMercator.FromLonLat(c.Lng, c.Lat).ToMPoint());
            feature["name"] = c.Name;
            feature["country"] = c.Country;
            var callbackImage = CreateCallbackImage(c);
            var bitmapId = BitmapRegistry.Instance.Register(callbackImage);
            var calloutStyle = CreateCalloutStyle(bitmapId);
            feature.Styles.Add(calloutStyle);
            return feature;
        });
    }

    private static IStyle CreateCalloutStyle(int bitmapId)
    {
        var calloutStyle = new CalloutStyle { Content = bitmapId, ArrowPosition = Random.Next(1, 9) * 0.1f, RotateWithMap = true, Type = CalloutType.Custom };
        switch (Random.Next(0, 4))
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

    private static MemoryStream CreateCallbackImage(City city)
    {
        using var paint = new SKPaint
        {
            Color = new SKColor((byte)Random.Next(0, 256), (byte)Random.Next(0, 256), (byte)Random.Next(0, 256)),
            Typeface = SKTypeface.FromFamilyName(null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright),
            TextSize = 20
        };

        SKRect bounds;
        using (var textPath = paint.GetTextPath(city.Name, 0, 0))
        {
            // Set transform to center and enlarge clip path to window height
            textPath.GetTightBounds(out bounds);
        }

        using var bitmap = new SKBitmap((int)(bounds.Width + 1), (int)(bounds.Height + 1));
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear();
        canvas.DrawText(city.Name, -bounds.Left, -bounds.Top, paint);
        var memStream = new MemoryStream();
        using (var wStream = new SKManagedWStream(memStream))
        {
            bitmap.Encode(wStream, SKEncodedImageFormat.Png, 100);
        }
        return memStream;
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
        using var sr = new StreamReader(stream);
        using var jsonTextReader = new JsonTextReader(sr);
        return serializer.Deserialize<List<T>>(jsonTextReader) ?? new List<T>();
    }
}
