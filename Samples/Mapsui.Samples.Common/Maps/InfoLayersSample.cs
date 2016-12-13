using System.Linq;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps
{
    public static class InfoLayersSample
    {
        private const string InfoLayerName = "Info Layer";
        private const string HoverInfoLayerName = "Hover Info Layer";
        private const string PolygonLayerName = "Polygon Layer";

        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(OsmSample.CreateLayer());
            map.Layers.Add(CreateInfoLayer(map.Envelope));
            map.Layers.Add(CreateHoverInfoLayer(map.Envelope));
            map.Layers.Add(CreatePolygonLayer());

            map.InfoLayers.Add(map.Layers.First(l => l.Name == InfoLayerName));
            map.InfoLayers.Add(map.Layers.First(l => l.Name == PolygonLayerName));
            map.HoverInfoLayers.Add(map.Layers.First(l => l.Name == HoverInfoLayerName));

            return map;
        }

        private static ILayer CreatePolygonLayer()
        {
            var layer = new MemoryLayer {Name = PolygonLayerName};
            var provider = new MemoryProvider();
            var feature = new Feature
            {
                Geometry = new Polygon(new LinearRing(new[]
                {
                    new Point(1000000, 1000000),
                    new Point(1000000, -1000000),
                    new Point(-1000000, -1000000),
                    new Point(-1000000, 1000000),
                    new Point(1000000, 1000000)
                })),
                ["Name"] = "Polygon 1"
            };
            provider.Features.Add(feature);
            layer.DataSource = provider;
            return layer;
        }

        private static ILayer CreateInfoLayer(BoundingBox envelope)
        {
            return new Layer(InfoLayerName)
            {
                DataSource = PointsSample.CreateProviderWithRandomPoints(envelope, 25),
                Style = CreateSymbolStyle()
            };
        }

        private static ILayer CreateHoverInfoLayer(BoundingBox envelope)
        {
            return new Layer(HoverInfoLayerName)
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