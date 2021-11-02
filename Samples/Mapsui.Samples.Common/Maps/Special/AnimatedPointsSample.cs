﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps.Special
{
    public class AnimatedPointsSample : ISample
    {
        public string Name => "Animated point movement";

        public string Category => "Special";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(new AnimatedPointsWithAutoUpdateLayer { Name = "Animated Points" });
            return map;
        }
    }

    public class AnimatedPointsWithAutoUpdateLayer : AnimatedPointLayer
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        // ReSharper disable once NotAccessedField.Local
        private readonly Timer _timer;

        public AnimatedPointsWithAutoUpdateLayer()
            : base(new DynamicMemoryProvider())
        {
            Style = new SymbolStyle { Fill = { Color = new Color(255, 215, 0, 200) }, SymbolScale = 0.9 };
            _timer = new Timer(_ => UpdateData(), this, 0, 2000);
        }

        private class DynamicMemoryProvider : GeometryMemoryProvider<IPointFeature>
        {
            private readonly Random _random = new(0);

            public override IEnumerable<IPointFeature> GetFeatures(FetchInfo fetchInfo)
            {
                var features = new List<IPointFeature>();
                var points = RandomPointHelper.GenerateRandomPoints(fetchInfo.Extent, 10, _random.Next()).ToList();
                var count = 0;
                var random = _random.Next(points.Count);

                foreach (var point in points)
                {
                    if (count != random) // skip a random element to test robustness
                    {
                        var feature = new PointFeature
                        {
                            Point = point,
                            ["ID"] = count.ToString(CultureInfo.InvariantCulture)
                        };
                        features.Add(feature);
                    }
                    count++;
                }
                return features;
            }
        }
    }
}