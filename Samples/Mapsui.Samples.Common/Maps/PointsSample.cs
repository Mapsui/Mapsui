using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;
using Newtonsoft.Json;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Mapsui.Samples.Common.Maps
{
    public static class PointsSample
    {
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
                DataSource = new MemoryProvider(GetCitiesFromEmbeddedResource()),
                Style = CreateBitmapStyle()
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
                return feature;
            });
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

        private static SymbolStyle CreateBitmapStyle()
        {
            // For this sample we get the bitmap from an embedded resouce
            // but you could get the data stream from the web or anywhere
            // else.
            var path = "Mapsui.Samples.Common.Images.home.png"; // Designed by Freepik http://www.freepik.com
            var bitmapId = GetBitmapIdForEmbeddedResource(path);
            var bitmapHeight = 176; // To set the offset correct we need to know the bitmap height
            return new SymbolStyle { BitmapId = bitmapId, SymbolScale = 0.20, SymbolOffset = new Offset(0, bitmapHeight * 0.5) };
        }

        private static int GetBitmapIdForEmbeddedResource(string imagePath)
        {
            var assembly = typeof(PointsSample).GetTypeInfo().Assembly;
            var image = assembly.GetManifestResourceStream(imagePath);
            return BitmapRegistry.Instance.Register(image);
        }
    }
}