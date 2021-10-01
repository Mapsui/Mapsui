﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Rendering.Skia;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Newtonsoft.Json;
using SkiaSharp;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Mapsui.Samples.Common.Maps
{
    public class CalloutSample : ISample
    {
        private static readonly Random Random = new Random();

        public string Name => "1 Callout";
        public string Category => "Geometries";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreatePointLayer());
            map.Home = n => n.NavigateTo(map.Layers[1].Envelope.Centroid, map.Resolutions[5]);
            return map;
        }

        private static MemoryLayer CreatePointLayer()
        {
            return new MemoryLayer
            {
                Name = "Points",
                IsMapInfoLayer = true,
                DataSource = new MemoryProvider(GetCitiesFromEmbeddedResource()),
                Style = new VectorStyle()
            };
        }

        private static IEnumerable<IFeature> GetCitiesFromEmbeddedResource()
        {
            var path = "Mapsui.Samples.Common.EmbeddedResources.congo.json";
            var assembly = typeof(PointsSample).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream(path);
            var cities = DeserializeFromStream<City>(stream);

            return cities.Select(c =>
            {
                var feature = new Feature();
                var point = SphericalMercator.FromLonLat(c.Lng, c.Lat);
                feature.Geometry = point;
                feature["name"] = c.Name;
                feature["country"] = c.Country;
                var calloutStyle = CreateCalloutStyle2(c.Name);
                feature.Styles.Add(calloutStyle);
                return feature;
            });
        }

        private static CalloutStyle CreateCalloutStyle2(string name)
        {
            return new CalloutStyle
            {
                Title = name,
                Type = CalloutType.Detail,
                Content = -1,
                TitleFont = { FontFamily = null, Size = 12, Italic = false, Bold = true },
                TitleFontColor = Color.Gray,
                TitleTextAlignment = Alignment.Center,
                Spacing = 2,
                MaxWidth = 111,
                RectRadius = 10,
                ShadowWidth = 4,
                StrokeWidth = 0,
                ArrowAlignment = ArrowAlignment.Bottom,
                Offset = new Offset(SymbolStyle.DefaultHeight * 0.5f, 0),
                RotateWithMap = true,
                ArrowPosition = 1,
                Enabled = false
            };
        }

        private static CalloutStyle CreateCalloutStyle(int bitmapId)
        {
            var calloutStyle = new CalloutStyle { Content = bitmapId, ArrowPosition = Random.Next(1, 9) * 0.1f, RotateWithMap = true };
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
            using (var bitmap = new SKBitmap((int)(bounds.Width + 1), (int)(bounds.Height + 1)))
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear();
                canvas.DrawText(city.Name, -bounds.Left, -bounds.Top, paint);
                var memStream = new MemoryStream();
                using (var wStream = new SKManagedWStream(memStream))
                {
                    bitmap.Encode(wStream, SKEncodedImageFormat.Png, 100);
                }

                // To check if the image is generated correctly you could add:
                // memStream.Position = 0;
                // File.WriteAllBytes("C:/temp/test-image.png", memStream.ToBytes());

                return memStream;
            }
        }

        private class City
        {
            public string Country { get; set; }
            public string Name { get; set; }
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        public static IEnumerable<T> DeserializeFromStream<T>(Stream stream)
        {
            var serializer = new JsonSerializer();

            using var sr = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(sr);
            return serializer.Deserialize<List<T>>(jsonTextReader);
        }
    }
}