﻿using System;
using System.Collections.Generic;
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
            mapControl.Map = CreateMap(mapControl.PixelDensity);
        }

        public static Map CreateMap(float pixelDensity)
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(new RasterizingLayer(CreateRandomPointLayer(), pixelDensity: pixelDensity));
            map.Home = n => n.NavigateTo(map.Layers[1].Envelope.Grow(map.Layers[1].Envelope.Width * 0.1));
            return map;
        }

        private static MemoryLayer CreateRandomPointLayer()
        {
            var rnd = new Random(3462); // Fix the random seed so the features don't move after a refresh
            var features = new List<IGeometryFeature>();
            for (var i = 0; i < 100; i++)
            {
                var feature = new Feature
                {
                    Geometry = new Geometries.Point(rnd.Next(0, 5000000), rnd.Next(0, 5000000))
                };
                features.Add(feature);
            }
            var provider = new MemoryProvider<IGeometryFeature>(features);

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