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
            var map = new Map
            {
                BackColor = Color.Transparent,
                Home = n => n.ZoomTo(0.5)
            };

            var provider = CreateRandomPointsProvider(GenerateRandomPoints(new BoundingBox(-100, -100, 100, 100), 20));
            map.Layers.Add(CreateStackedLabelLayer(provider, LabelColumn));
            map.Layers.Add(CreateLayer(provider));

            return map;
        }

        private static ILayer CreateStackedLabelLayer(IProvider provider, string labelColumn)
        {
            return new MemoryLayer
            {
                DataSource = new StackedLabelProvider(provider, new LabelStyle {LabelColumn = labelColumn}),
                Style = null
            };
        }

        private static ILayer CreateLayer(IProvider dataSource)
        {
            return new MemoryLayer
            {
                DataSource = dataSource,
                Style = new SymbolStyle {SymbolScale = 1, Fill = new Brush(new Color {A = 128, R = 8, G = 20, B = 192})}
            };
        }

        private static MemoryProvider CreateRandomPointsProvider(IEnumerable<IGeometry> randomPoints)
        {
            var features = new Features();
            var count = 0;
            foreach (var point in randomPoints)
            {
                var feature = new Feature
                {
                    Geometry = point,
                    [LabelColumn] = count.ToString(CultureInfo.InvariantCulture)
                };
                features.Add(feature);
                count++;
            }
            return new MemoryProvider(features);
        }

        private static IEnumerable<IGeometry> GenerateRandomPoints(BoundingBox box, int count = 25)
        {
            var result = new List<IGeometry>();
            var random = new Random(0);

            for (var i = 0; i < count; i++)
            {
                var x = random.NextDouble()*box.Width + box.Left;
                var y = random.NextDouble()*box.Height - (box.Height - box.Top);
                result.Add(new Point(x, y));
            }

            return result;
        }
    }
}