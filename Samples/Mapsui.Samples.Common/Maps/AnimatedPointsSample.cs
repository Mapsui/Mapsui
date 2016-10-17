using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BruTile.Predefined;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class AnimatedPointsSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(new TileLayer(KnownTileSources.Create()) {Name = "OSM"});
            map.Layers.Add(new AnimatedPointsWithAutoUpdateLayer {Name = "Animated Points"});
            return map;
        }
    }

    public class AnimatedPointsWithAutoUpdateLayer : AnimatedPointLayer
    {
        private Timer _timer;

        public AnimatedPointsWithAutoUpdateLayer()
            : base(new DynamicMemoryProvider())
        {
            _timer = new Timer(arg => UpdateData(), this, 0, 2000);
        }

        private class DynamicMemoryProvider : MemoryProvider
        {
            readonly Random _random = new Random(0);

            public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
            {
                var features = new List<IFeature>();
                var geometries = PointsSample.GenerateRandomPoints(box, 10).ToList();
                var count = 0;
                var random = _random.Next(geometries.Count);
                
                foreach (var geometry in geometries)
                {
                    if (count != random) // skip a random element to test robustness
                    {
                        var feature = new Feature
                        {
                            Geometry = geometry,
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