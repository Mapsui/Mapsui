using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Layers;
using Mapsui.Layers.Tiling;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI;

// ReSharper disable UnusedAutoPropertyAccessor.Local
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.

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
            map.Home = n => n.NavigateTo(map.Layers[1].Extent?.Grow(map.Layers[1].Extent!.Width * 0.25));
            return map;
        }

        private static ILayer CreatePointLayer()
        {
            return new MemoryLayer
            {
                Name = "Points",
                IsMapInfoLayer = true,
                DataSource = new MemoryProvider<IFeature>(GetFeature())
            };
        }

        private static IFeature GetFeature()
        {
            var lineString = CreateLineStringWithManyVertices();
            var feature = new GeometryFeature();
            AddStyles(feature);
            feature.Geometry = lineString;
            feature["Name"] = $"LineString with {lineString.Vertices.Count()} vertices";
            return feature;
        }

        private static LineString CreateLineStringWithManyVertices()
        {
            var startPoint = new Point(1623484, 7652571);

            var points = new List<Point>();

            for (var i = 0; i < 10000; i++)
            {
                points.Add(new Point(startPoint.X + i, startPoint.Y + i));
            }

            return new LineString(points);
        }

        private static void AddStyles(IFeature feature)
        {
            // route outline style
            var vsout = new VectorStyle
            {
                Opacity = 0.5f,
                Line = new Pen(Color.White, 10f),
            };

            var vs = new VectorStyle
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