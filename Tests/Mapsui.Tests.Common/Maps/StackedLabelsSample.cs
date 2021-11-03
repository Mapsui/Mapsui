using System;
using System.Collections.Generic;
using System.Globalization;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;

namespace Mapsui.Tests.Common.Maps
{
    public class StackedLabelsSample : ISample
    {
        private const string LabelColumn = "Label";
        public string Category => "Tests";

        public string Name => "Stacked Labels";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var provider = CreateRandomPointsProvider(GenerateRandomPoints(new BoundingBox(-100, -100, 100, 100), 20));
            var layer = CreateLayer(provider);
            var stackedLabelLayer = CreateStackedLabelLayer(provider, LabelColumn);

            var map = new Map
            {
                BackColor = Color.FromString("WhiteSmoke"),
                Home = n => n.NavigateTo(layer.Envelope.Grow(layer.Envelope.Width * 0.3))
            };

            map.Layers.Add(stackedLabelLayer);
            map.Layers.Add(layer);

            return map;
        }

        private static MemoryLayer CreateStackedLabelLayer(IProvider<IGeometryFeature> provider, string labelColumn)
        {
            return new MemoryLayer
            {
                DataSource = new StackedLabelProvider(provider, new LabelStyle { LabelColumn = labelColumn }),
                Style = null
            };
        }

        private static MemoryLayer CreateLayer(IProvider<IGeometryFeature> dataSource)
        {
            return new MemoryLayer
            {
                DataSource = dataSource,
                Style = new SymbolStyle { SymbolScale = 1, Fill = new Brush(new Color { A = 128, R = 8, G = 20, B = 192 }) }
            };
        }

        private static GeometryMemoryProvider<IGeometryFeature> CreateRandomPointsProvider(IEnumerable<IGeometry> randomPoints)
        {
            var features = new List<IGeometryFeature>();
            var count = 0;
            foreach (var point in randomPoints)
            {
                var feature = new GeometryFeature
                {
                    Geometry = point,
                    [LabelColumn] = count.ToString(CultureInfo.InvariantCulture)
                };
                features.Add(feature);
                count++;
            }
            return new GeometryMemoryProvider<IGeometryFeature>(features);
        }

        private static IEnumerable<IGeometry> GenerateRandomPoints(BoundingBox box, int count = 25)
        {
            var result = new List<IGeometry>();
            var random = new Random(0);

            for (var i = 0; i < count; i++)
            {
                var x = random.NextDouble() * box.Width + box.Left;
                var y = random.NextDouble() * box.Height - (box.Height - box.Top);
                result.Add(new Point(x, y));
            }

            return result;
        }
    }
}