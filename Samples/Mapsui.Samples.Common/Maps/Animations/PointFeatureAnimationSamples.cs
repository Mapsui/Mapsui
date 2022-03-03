using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Mapsui.Layers;
using Mapsui.Layers.AnimatedLayers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI;

#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.

namespace Mapsui.Samples.Common.Maps.Special
{
    public class PointFeatureAnimationSamples : ISample, ISampleTest
    {
        private static AnimatedPointsWithAutoUpdateLayer _animatedPointsWithAutoUpdateLayer;
        public string Name => "Point Feature Animation";

        public string Category => "Animations";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            _animatedPointsWithAutoUpdateLayer = new AnimatedPointsWithAutoUpdateLayer { Name = "Animated Points" };
            map.Layers.Add(_animatedPointsWithAutoUpdateLayer);
            return map;
        }

        public void InitializeTest()
        {
            _animatedPointsWithAutoUpdateLayer.Stop();
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _timer.Dispose();
            }
        }

        public void Stop()
        {
            _timer.Dispose();
        }
    }

    internal class DynamicMemoryProvider : MemoryProvider<PointFeature>
    {
        private readonly Random _random = new(0);

        public override IEnumerable<PointFeature> GetFeatures(FetchInfo fetchInfo)
        {
            var features = new List<PointFeature>();
            var points = RandomPointGenerator.GenerateRandomPoints(fetchInfo.Extent, 10, _random.Next()).ToList();
            var count = 0;
            var random = _random.Next(points.Count);

            foreach (var point in points)
            {
                if (count != random) // skip a random element to test robustness
                {
                    var feature = new PointFeature(point)
                    {
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