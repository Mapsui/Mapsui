using System;
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
using Newtonsoft.Json;
using SkiaSharp;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Mapsui.Samples.Common.Maps
{
    public class CustomStyleSample : ISample
    {
        private static Random Random = new Random();

        public static bool IsSkiaRenderer = false;

        public string Name => "6 Custom Style";
        public string Category => "Geometries";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
            IsSkiaRenderer = mapControl.Renderer is Rendering.Skia.MapRenderer;
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
                Style = new VectorStyle() { }
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

                var customStyle = CreateCustomStyle();
                feature.Styles.Add(customStyle);

                return feature;
            });
        }

        private static IStyle CreateCustomStyle()
        {
            var customStyle = new CustomStyle() { RotateWithMap = true };

            customStyle.OnRender += Render;

            return customStyle;
        }

        private static void Render(object sender, RenderStyleEventArgs e)
        {
            if (e.Canvas is SKCanvas canvas)//IsSkiaRenderer)
            {
                // Here goes code for Skia renderer

                // Owner drawn content should always stay in the top middle of the underlaying vector style
                canvas.Translate(new SKPoint(0, -16));
                // Than rotate
                canvas.RotateDegrees(e.Rotation);
                // Draw a random geometry
                if (Random.Next(0, 2) == 0)
                    canvas.DrawCircle(new SKPoint(0, 0), 5, new SKPaint() { Color = SKColors.Red, IsAntialias = true });
                else
                    canvas.DrawRect(new SKRect(-5, -5, 5, 5), new SKPaint() { Color = SKColors.Blue, IsAntialias = true });
            }
            else
            {
                // Here goes code for the WPF renderer
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

            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<List<T>>(jsonTextReader);
            }
        }
    }
}