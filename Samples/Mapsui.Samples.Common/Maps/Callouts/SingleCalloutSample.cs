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

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable once ClassNeverInstantiated.Local

namespace Mapsui.Samples.Common.Maps.Callouts
{
    public class SingleCalloutSample : ISample
    {
        public string Name => "1 Single Callout";
        public string Category => "Info";

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
            map.Info += MapOnInfo;

            return map;
        }

        private static void MapOnInfo(object sender, MapInfoEventArgs e)
        {
            var calloutStyle = e.MapInfo.Feature?.Styles.Where(s => s is CalloutStyle).Cast<CalloutStyle>().FirstOrDefault();
            if (calloutStyle != null)
            {
                calloutStyle.Enabled = !calloutStyle.Enabled;
                e.MapInfo.Layer.DataHasChanged(); // To trigger a refresh of graphics.
            }
        }

        private static MemoryLayer CreatePointLayer()
        {
            return new MemoryLayer
            {
                Name = "Points",
                IsMapInfoLayer = true,
                DataSource = new MemoryProvider<IGeometryFeature>(GetCitiesFromEmbeddedResource()),
                Style = new VectorStyle()
            };
        }

        private static IEnumerable<IGeometryFeature> GetCitiesFromEmbeddedResource()
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
                var calloutStyle = CreateCalloutStyle(c.Name);
                feature.Styles.Add(calloutStyle);
                return feature;
            });
        }

        private static CalloutStyle CreateCalloutStyle(string name)
        {
            return new CalloutStyle
            {
                Title = name,
                TitleFont = { FontFamily = null, Size = 12, Italic = false, Bold = true },
                TitleFontColor = Color.Gray,
                MaxWidth = 120,
                RectRadius = 10,
                ShadowWidth = 4,
                Enabled = false,
                SymbolOffset = new Offset(0, SymbolStyle.DefaultHeight * 0.3f)
            };
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