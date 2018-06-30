using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class InfoLayersSample
    {
        private const string InfoLayerName = "Info Layer";
        private const string PolygonLayerName = "Polygon Layer";
        private const string LineLayerName = "Line Layer";

        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateInfoLayer(map.Envelope));
            map.Layers.Add(CreatePolygonLayer());
            map.Layers.Add(CreateLineLayer());
            
            return map;
        }

        private static ILayer CreatePolygonLayer()
        {
            var features = new Features { CreatePolygonFeature(), CreateMultiPolygonFeature() };
            var provider = new MemoryProvider(features);

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
                DataSource = new MemoryProvider(CreateLineFeature()),
                Style = null,
                IsMapInfoLayer = true
            };
        }

        private static Feature CreateMultiPolygonFeature()
        {
            var feature = new Feature
            {
                Geometry = CreateMultiPolygon(),
                ["Name"] = "Multipolygon 1"
            };
            feature.Styles.Add(new VectorStyle { Fill = new Brush(Color.Gray), Outline = new Pen(Color.Black) });
            return feature;
        }

        private static Feature CreatePolygonFeature()
        {
            var feature = new Feature
            {
                Geometry = CreatePolygon(),
                ["Name"] = "Polygon 1"
            };
            feature.Styles.Add(new VectorStyle());
            return feature;
        }

        private static Feature CreateLineFeature()
        {
            return new Feature
            {
                Geometry = CreateLine(),
                ["Name"] = "Line 1",
                Styles = new List<IStyle> { new VectorStyle{ Line = new Pen(Color.Violet, 6)}}
            };
        }

        private static MultiPolygon CreateMultiPolygon()
        {
            return new MultiPolygon
            {
                Polygons = new List<Polygon>
                {
                    new Polygon(new LinearRing(new[]
                    {
                        new Point(4000000, 3000000),
                        new Point(4000000, 2000000),
                        new Point(3000000, 2000000),
                        new Point(3000000, 3000000),
                        new Point(4000000, 3000000)
                    })),

                    new Polygon(new LinearRing(new[]
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

        private static ILayer CreateInfoLayer(BoundingBox envelope)
        {
            return new Layer(InfoLayerName)
            {
                DataSource = RandomPointHelper.CreateProviderWithRandomPoints(envelope, 25, 7),
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