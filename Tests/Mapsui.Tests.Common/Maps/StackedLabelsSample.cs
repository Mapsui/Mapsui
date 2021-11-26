using System;
using System.Collections.Generic;
using System.Globalization;
using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Helpers;
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
            var provider = RandomPointGenerator.CreateProviderWithRandomPoints(new MRect(-100, -100, 100, 100), 20, 0);
            var layer = CreateLayer(provider);
            var stackedLabelLayer = CreateStackedLabelLayer(provider, LabelColumn);

            var map = new Map
            {
                BackColor = Color.FromString("WhiteSmoke"),
                Home = n => n.NavigateTo(layer.Extent?.Grow(layer.Extent.Width * 0.3))
            };

            map.Layers.Add(stackedLabelLayer);
            map.Layers.Add(layer);

            return map;
        }

        private static MemoryLayer CreateStackedLabelLayer(IProvider<IFeature> provider, string labelColumn)
        {
            return new MemoryLayer
            {
                DataSource = new StackedLabelProvider(provider, new LabelStyle { LabelColumn = labelColumn }),
                Style = null
            };
        }

        private static MemoryLayer CreateLayer(IProvider<IFeature> dataSource)
        {
            return new MemoryLayer
            {
                DataSource = dataSource,
                Style = new SymbolStyle { SymbolScale = 1, Fill = new Brush(new Color { A = 128, R = 8, G = 20, B = 192 }) }
            };
        }
    }
}