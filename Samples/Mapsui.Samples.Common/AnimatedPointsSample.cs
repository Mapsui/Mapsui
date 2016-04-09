using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common
{
    public static class AnimatedPointsSample
    {
        public static ILayer CreateLayer()
        {
            return new AnimatedPointsWithAutoUpdateLayer {Name = "AnimatedLayer"};
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
                var geometries = PointLayerSample.GenerateRandomPoints(box, 10).ToList();
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
                        yield return feature;
                    }
                    count++;
                }
            }
        }
    }
}
