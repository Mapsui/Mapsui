using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class InfoLayersSample
    {
        private const string InfoLayerName = "Info Layer";
        private const string HoverLayerName = "Hover Layer";
        private const string PolygonLayerName = "Polygon Layer";

        public static Map CreateMap()
        {
            var map = new Map();

            var t = OpenStreetMap.CreateTileLayer();
            var p = CreatePolygonLayer();
            map.Layers.Add(t);
            map.Layers.Add(CreateInfoLayer(map.Envelope));
            map.Layers.Add(CreateHoverLayer(map.Envelope));
            map.Layers.Add(p);

            map.InfoLayers.Add(map.Layers.First(l => l.Name == InfoLayerName));
            map.InfoLayers.Add(map.Layers.First(l => l.Name == PolygonLayerName));
            map.HoverLayers.Add(map.Layers.First(l => l.Name == HoverLayerName));

            StartTimerToRemoveAndAddLayer(map,t, p);

            return map;
        }

        private static void StartTimerToRemoveAndAddLayer(Map map, ILayer t, ILayer p)
        {
            Task.Run(() =>
            {
                Task.Delay(5000).Wait();

                map.Layers.Clear();
                map.Layers.Add(t);
                map.Layers.Add(CreateInfoLayer(map.Envelope));
                map.Layers.Add(CreateHoverLayer(map.Envelope));
                map.Layers.Add(p);
            });
        }

        private static ILayer CreatePolygonLayer()
        {
            var layer = new MemoryLayer {Name = PolygonLayerName};
            var provider = new MemoryProvider();
            provider.Features.Add(CreatePolygonFeature());
            provider.Features.Add(CreateMultiPolygonFeature());
            layer.DataSource = provider;
            layer.Style = null;
            return layer;
        }

        private static Feature CreateMultiPolygonFeature()
        {
            var feature = new Feature
            {
                Geometry = CreateMultiPolygon(),
                ["Name"] = "Multipolygon 1"
            };
            feature.Styles.Add(new VectorStyle { Fill = new Brush(Color.Gray), Outline = new Pen(Color.Black)});
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

        private static ILayer CreateInfoLayer(BoundingBox envelope)
        {
            return new Layer(InfoLayerName)
            {
                DataSource = PointsSample.CreateProviderWithRandomPoints(envelope, 25),
                Style = CreateSymbolStyle()
            };
        }

        private static ILayer CreateHoverLayer(BoundingBox envelope)
        {
            return new Layer(HoverLayerName)
            {
                DataSource = PointsSample.CreateProviderWithRandomPoints(envelope, 25),
                Style = CreateHoverSymbolStyle()
            };
        }

        private static SymbolStyle CreateHoverSymbolStyle()
        {
            return new SymbolStyle
            {
                SymbolScale = 0.8,
                Fill = new Brush(new Color(251, 236, 215)),
                Outline = {Color = Color.Gray, Width = 1}
            };
        }

        private static SymbolStyle CreateSymbolStyle()
        {
            return new SymbolStyle
            {
                SymbolScale = 0.8,
                Fill = new Brush(new Color(213, 234, 194)),
                Outline = {Color = Color.Gray, Width = 1}
            };
        }
    }
}