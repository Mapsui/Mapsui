using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Layers.AnimatedLayers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using Mapsui.UI;
using Mapsui.Utilities;

#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.

namespace Mapsui.Samples.Common.Maps.Special
{
    public class PointFeatureAnimationSamples : ISample
    {
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
            Style = CreatePointStyle();
            _timer = new Timer(_ => UpdateData(), this, 0, 2000);
        }

        private static IStyle CreatePointStyle()
        {
            return new ThemeStyle(f => {
                return CreateSvgArrowStyle("Images.arrow.svg", 0.5, f);
            });
        }

        private static IStyle CreateSvgArrowStyle(string embeddedResourcePath, double scale, IFeature feature)
        {
            var bitmapId = typeof(SvgSample).LoadSvgId(embeddedResourcePath);
            return new SymbolStyle
            {
                BitmapId = bitmapId,
                SymbolScale = scale,
                SymbolOffset = new Offset(0.0, 0.5, true),
                Opacity = 0.5f,
                SymbolRotation = (double)feature["rotation"]!
            };
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _timer.Dispose();
            }
        }
    }

    internal class DynamicMemoryProvider : MemoryProvider<PointFeature>, IDynamic
    {
        private readonly Random _random = new(0);
        private IEnumerable<PointFeature> _previousFeatures = new List<PointFeature>();

        public event DataChangedEventHandler? DataChanged;

        public override IEnumerable<PointFeature> GetFeatures(FetchInfo fetchInfo)
        {
            var features = new List<PointFeature>();
            var points = RandomPointGenerator.GenerateRandomPoints(fetchInfo.Extent, 10, _random).ToList();
            var count = 0;
            var random = _random.Next(points.Count);

            foreach (var point in points)
            {
                count++;
                if (count == random) continue; // skip a random element to test robustness

                var countAsString = count.ToString(CultureInfo.InvariantCulture);
                features.Add(new PointFeature(point)
                {
                    ["ID"] = countAsString,
                    ["rotation"] = AngleOf(point, FindPreviousPosition(countAsString)) - 90
                });
            }

            _previousFeatures = features;
            return features;
        }

        private MPoint? FindPreviousPosition(string countAsString)
        {
            return _previousFeatures.FirstOrDefault(f => f["ID"]?.ToString() == countAsString)?.Point;
        }

        public static double AngleOf(MPoint point1, MPoint? point2)
        {
            if (point2 == null) return 0;
            double result = Algorithms.RadiansToDegrees(Math.Atan2(point1.Y - point2.Y, point2.X - point1.X));
            return (result < 0) ? (360.0 + result) : result;
        }

        public void DataHasChanged()
        {
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, new DataChangedEventArgs(null, false, null));
        }
    }
}