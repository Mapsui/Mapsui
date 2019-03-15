using System;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public class RasterizingLayerSample : ISample
    {
        public string Name => "Rasterizing Layer";
        public string Category => "Special";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(new RasterizingLayer(CreateRandomPointLayer()));
            return map;
        }

        private static MemoryLayer CreateRandomPointLayer()
        {
            var rnd = new Random();
            var features = new Features();
            for (var i = 0; i < 100; i++)
            {
                var feature = new Feature
                {
                    Geometry = new Geometries.Point(rnd.Next(0, 5000000), rnd.Next(0, 5000000))
                };
                features.Add(feature);
            }
            var provider = new MemoryProvider(features);

            return new MemoryLayer
            {
                DataSource = provider,
                Style = new SymbolStyle
                {
                    SymbolType = SymbolType.Triangle,
                    Fill = new Brush(Color.Red)
                }
            };
        }
    }
}