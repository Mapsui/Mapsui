using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Layers;
using Mapsui.Layers.Tiling;
using Mapsui.Providers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;

#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.

namespace Mapsui.Samples.Common.Maps
{
    public class InfoLayersSample : ISample
    {
        private const string InfoLayerName = "Info Layer";
        private const string PolygonLayerName = "Polygon Layer";
        private const string LineLayerName = "Line Layer";

        public string Name => "2 Map Info";
        public string Category => "Demo";
        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateInfoLayer(map.Extent));
            map.Layers.Add(CreatePolygonLayer());
            map.Layers.Add(new WritableLayer());
            map.Layers.Add(CreateLineLayer());

            return map;
        }

        private static ILayer CreatePolygonLayer()
        {
            var features = new List<IFeature> { CreatePolygonFeature(), CreateMultiPolygonFeature() };
            var provider = new MemoryProvider<IFeature>(features);

            var layer = new MemoryLayer
            {
                Name = PolygonLayerName,
                DataSource = provider,
                Style = null,
                IsMapInfoLayer = true
            };

            return layer;
        }

        private static ILayer CreateLineLayer()
        {
            return new MemoryLayer
            {
                Name = LineLayerName,
                DataSource = new MemoryProvider<IFeature>(CreateLineFeature()),
                Style = null,
                IsMapInfoLayer = true
            };
        }

        private static GeometryFeature CreateMultiPolygonFeature()
        {
            var feature = new GeometryFeature
            {
                Geometry = CreateMultiPolygon(),
                ["Name"] = "Multipolygon 1"
            };
            feature.Styles.Add(new VectorStyle { Fill = new Brush(Color.Gray), Outline = new Pen(Color.Black) });
            return feature;
        }

        private static GeometryFeature CreatePolygonFeature()
        {
            var feature = new GeometryFeature
            {
                Geometry = CreatePolygon(),
                ["Name"] = "Polygon 1"
            };
            feature.Styles.Add(new VectorStyle());
            return feature;
        }

        private static GeometryFeature CreateLineFeature()
        {
            return new GeometryFeature
            {
                Geometry = CreateLine(),
                ["Name"] = "Line 1",
                Styles = new List<IStyle> { new VectorStyle { Line = new Pen(Color.Violet, 6) } }
            };
        }

        private static MultiPolygon CreateMultiPolygon()
        {
            return new MultiPolygon
            {
                Polygons = new List<Polygon>
                {
                    new (new LinearRing(new[]
                    {
                        new Point(4000000, 3000000),
                        new Point(4000000, 2000000),
                        new Point(3000000, 2000000),
                        new Point(3000000, 3000000),
                        new Point(4000000, 3000000)
                    })),

                    new (new LinearRing(new[]
                    {
                        new Point(4000000, 5000000),
                        new Point(4000000, 4000000),
                        new Point(3000000, 4000000),
                        new Point(3000000, 5000000),
                        new Point(4000000, 5000000)
                    }))
                }
            };
        }

        private static Polygon CreatePolygon()
        {
            return new Polygon(new LinearRing(new[]
            {
                new Point(1000000, 1000000),
                new Point(1000000, -1000000),
                new Point(-1000000, -1000000),
                new Point(-1000000, 1000000),
                new Point(1000000, 1000000)
            }));
        }

        private static LineString CreateLine()
        {
            var offsetX = -2000000;
            var offsetY = -2000000;
            var stepSize = -2000000;

            return new LineString(new[]
            {
                new Point(offsetX + stepSize,      offsetY + stepSize),
                new Point(offsetX + stepSize * 2,  offsetY + stepSize),
                new Point(offsetX + stepSize * 2,  offsetY + stepSize * 2),
                new Point(offsetX + stepSize * 3,  offsetY + stepSize * 2),
                new Point(offsetX + stepSize * 3,  offsetY + stepSize * 3)
            });
        }

        private static ILayer CreateInfoLayer(MRect? envelope)
        {
            return new Layer(InfoLayerName)
            {
                DataSource = RandomPointGenerator.CreateProviderWithRandomPoints(envelope, 25, 7),
                Style = CreateSymbolStyle(),
                IsMapInfoLayer = true
            };
        }

        private static SymbolStyle CreateSymbolStyle()
        {
            return new SymbolStyle
            {
                SymbolScale = 0.8,
                Fill = new Brush(new Color(213, 234, 194)),
                Outline = { Color = Color.Gray, Width = 1 }
            };
        }
    }
}