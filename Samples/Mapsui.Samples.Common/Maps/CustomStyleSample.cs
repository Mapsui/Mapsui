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
    public class OwnerDrawnStyle : CustomStyle
    {
        private static Random Random = new Random();

        public override void Render(object canvas, IFeature feature, float mapRotation)
        {
            if (canvas is SKCanvas skCanvas)
            {
                // Here goes code for Skia renderer

                // Owner drawn content should always stay in the top middle of the underlaying vector style
                skCanvas.Save();
                skCanvas.Translate(new SKPoint(0, -16));
                // Than rotate
                var rotation = (float)Rotation + (RotateWithMap ? mapRotation : 0f);
                skCanvas.RotateDegrees(rotation);
                // Draw a random geometry
                if (Random.Next(0, 2) == 0)
                    skCanvas.DrawCircle(new SKPoint(0, 0), 5, new SKPaint() { Color = SKColors.Red, IsAntialias = true });
                else
                    skCanvas.DrawRect(new SKRect(-5, -5, 5, 5), new SKPaint() { Color = SKColors.Blue, IsAntialias = true });
                skCanvas.Restore();
                
                skCanvas.Save();
                SKRect bounds;
                using (SKPaint paint = new SKPaint())
                {
                    paint.Color = new SKColor((byte)Random.Next(0, 256), (byte)Random.Next(0, 256), (byte)Random.Next(0, 256));
                    paint.Typeface = SKTypeface.FromFamilyName(null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
                    paint.TextSize = 20;

                    using (SKPath textPath = paint.GetTextPath((string)feature["name"], 0, 0))
                    {
                        // Set transform to center and enlarge clip path to window height
                        textPath.GetTightBounds(out bounds);
                    }
                    skCanvas.Translate(-bounds.MidX, -bounds.MidY);
                    skCanvas.DrawText((string)feature["name"], new SKPoint(0, 0), paint);
                }
                skCanvas.Restore();
            }
            else
            {
                // Here goes code for the WPF renderer
            }
        }
    }

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

                var ownerDrawnStyle = CreateOwnerDrawnStyle();
                feature.Styles.Add(ownerDrawnStyle);

                return feature;
            });
        }

        private static IStyle CreateOwnerDrawnStyle()
        {
            var ownerDrawnStyle = new OwnerDrawnStyle() { RotateWithMap = true };

            return ownerDrawnStyle;
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