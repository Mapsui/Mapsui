using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Mapsui.Samples.Common.Maps
{
    public class ManyVerticesSample : ISample
    {
        public string Name => "Many Vertices";
        public string Category => "Special";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap(mapControl.PixelDensity);
        }

        public static Map CreateMap(float pixelDensity)
        {
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(new RasterizingLayer(CreatePointLayer(), pixelDensity: pixelDensity));
            map.Home = n => n.NavigateTo(map.Layers[1].Envelope.Grow(map.Layers[1].Envelope.Width * 0.25));
            return map;
        }

        private static ILayer CreatePointLayer()
        {
            return new MemoryLayer
            {
                Name = "Points",
                IsMapInfoLayer = true,
                DataSource = new MemoryProvider(GetFeature())
            };
        }

        private static IFeature GetFeature()
        {
            var feature = new Feature();

            var startPoint = new Point(1623484, 7652571);

            var points = new List<Point>();

            for (int i = 0; i < 10000; i++)
            {
                points.Add(new Point(startPoint.X + i, startPoint.Y + i));
            }

            AddStyles(feature);
            feature.Geometry = new LineString(points);
            feature["Name"] = $"LineString with {points.Count()} vertices";
            return feature;
        }

        private static void AddStyles(Feature feature)
        {
            // route outline style
            VectorStyle vsout = new VectorStyle
            {
                Opacity = 0.5f,
                Line = new Pen(Color.White, 10f),
            };

            VectorStyle vs = new VectorStyle
            {
                Fill = null,
                Outline = null,
                Line = { Color = Color.Red, Width = 5f }
            };

            feature.Styles.Add(vsout);
            feature.Styles.Add(vs);
        }
    }
}