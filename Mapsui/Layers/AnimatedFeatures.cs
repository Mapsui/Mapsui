using System.Diagnostics;
using Mapsui.Geometries;
using Mapsui.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Mapsui.Layers
{
    public enum EasingFunction
    {
        CubicEaseOut,
        Linear
    }

    public class AnimatedFeatures
    {
        private readonly Timer _animationTimer;
        private List<AnimatedItem> _cache = new List<AnimatedItem>();
        private long _startTimeAnimation;
        private readonly int _millisecondsBetweenUpdates;

        /// <summary>
        /// When the distance between the current and the previous position is larger
        /// than the DistanceThreshold it will not be animated. 
        /// The default is Double.MaxValue
        /// </summary>
        public double DistanceThreshold { get; set; }

        public AnimatedFeatures(int millisecondsBetweenUpdates = 16)
        {
            AnimationDuration = 1000;
            IdField = "ID";
            Function = EasingFunction.CubicEaseOut;
            DistanceThreshold = double.MaxValue;
            _millisecondsBetweenUpdates = millisecondsBetweenUpdates;
            _animationTimer = new Timer(AnimationCallback, this, Timeout.Infinite, Timeout.Infinite);
        }

        public string IdField { get; set; }
        public int AnimationDuration { get; set; }
        public EasingFunction Function { get; set; }

        public event EventHandler AnimatedPositionChanged;
        
        public void AddFeatures(IEnumerable<IFeature> features)
        {
            var previousCache = _cache;
 
           _cache = ConvertToAnimatedItems(features.ToList(), previousCache, IdField);
            _startTimeAnimation = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            _animationTimer.Change(_millisecondsBetweenUpdates, _millisecondsBetweenUpdates);
            _first = true;
        }

        public IEnumerable<IFeature> GetFeatures()
        {
            LogAllFeatures(_cache);

            var progress = CalculateProgress(_startTimeAnimation, AnimationDuration, Function);
            if (!Completed(progress)) InterpolateAnimatedPosition(_cache, progress, DistanceThreshold);
            else _animationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            return _cache.Select(f => f.Feature);
        }

        private static bool Completed(double progress)
        {
            return progress >= 1;
        }

        protected virtual void OnAnimatedPositionChanged()
        {
            AnimatedPositionChanged?.Invoke(this, new EventArgs());
        }

        private static void AnimationCallback(object state)
        {
            var animatedPointLayer = (AnimatedFeatures)state;
            animatedPointLayer.OnAnimatedPositionChanged();
        }

        private static void LogAllFeatures(IEnumerable<AnimatedItem> animatedItems)
        {
            if (!_first) return;
            _first = false;

            Debug.WriteLine("ITERATION: " + _counter + " ===========================================================================");
            _counter++;

            foreach (var animatedItem in animatedItems)
            {
                var target = animatedItem.Feature.Geometry as Point;
                if (animatedItem.PreviousPoint == null || animatedItem.CurrentPoint == null || target == null) continue;
                if (animatedItem.PreviousPoint.Distance(animatedItem.CurrentPoint) < 10000) continue;
                LogItem(animatedItem);
            }
        }

        private static List<AnimatedItem> ConvertToAnimatedItems(IEnumerable<IFeature> features,
            List<AnimatedItem> previousItems, string idField)
        {
            var result = new List<AnimatedItem>();
            foreach (var feature in features)
            {
                var animatedItem = new AnimatedItem
                {
                    Feature = feature,
                    CurrentPoint = CopyAsPoint(feature.Geometry),
                    PreviousPoint = CopyAsPoint(FindPreviousPoint(previousItems, feature, idField))
                };
                result.Add(animatedItem);
            }
            return result;
        }

        private static Point CopyAsPoint(IGeometry geometry)
        {
            var point = geometry as Point;
            return point == null ? null : new Point(point.X, point.Y);
        }

        private static int _counter;
        private static bool _first = true;

        private static void InterpolateAnimatedPosition(IEnumerable<AnimatedItem> items, double progress, double threshold)
        {
            foreach (var item in items)
            {
                var target = item.Feature.Geometry as Point;
                if (item.PreviousPoint == null || item.CurrentPoint == null || target == null) continue;
                if (item.PreviousPoint.Distance(item.CurrentPoint) > threshold) continue; 
                target.X = item.PreviousPoint.X + (item.CurrentPoint.X - item.PreviousPoint.X) * progress;
                target.Y = item.PreviousPoint.Y + (item.CurrentPoint.Y - item.PreviousPoint.Y) * progress;
            }
        }

        private static void LogItem(AnimatedItem item)
        {
            Debug.WriteLine("Trackee: " + item.Feature["Trackee"]);
            Debug.WriteLine("ID: " + item.Feature["ID"]);
            Debug.WriteLine("speed: " + item.Feature["Speed"]);
            Debug.WriteLine("Bps: " + item.Feature["Bps"]);
            Debug.WriteLine("DateGps: " + item.Feature["DateGps"]);
            Debug.WriteLine("DateReceived: " + item.Feature["DateReceived"]);
            Debug.WriteLine("Longitude: " + item.Feature["Longitude"]);
            Debug.WriteLine("Latitude: " + item.Feature["Latitude"]);

            Debug.WriteLine("X: " + item.CurrentPoint.X);
            Debug.WriteLine("Y: " + item.CurrentPoint.Y);
            Debug.WriteLine("Previous X: " + item.PreviousPoint.X);
            Debug.WriteLine("Previous Y: " + item.PreviousPoint.Y);
            Debug.WriteLine("-------------------------------------------------");
        }

        private static Point FindPreviousPoint(IEnumerable<AnimatedItem> previousItems, IFeature feature,
            string idField)
        {
            return previousItems?.FirstOrDefault(f => f.Feature[idField].Equals(feature[idField]))?.CurrentPoint;
        }

        private static double CalculateProgress(long startTime, int animationDuration, EasingFunction function)
        {
            var currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var elapsedTime = (double)currentTime - startTime;

            if (function == EasingFunction.CubicEaseOut)
                return CubicEaseOut(animationDuration, elapsedTime);
            return Linear(animationDuration, elapsedTime);
        }

        private static double Linear(double d, double t)
        {
            return t / d;
        }

        private static double CubicEaseOut(double d, double t)
        {
            return ((t = t / d - 1) * t * t + 1);
        }

        private class AnimatedItem
        {
            public IFeature Feature { get; set; }
            public Point PreviousPoint { get; set; }
            public Point CurrentPoint { get; set; }
        }
    }
}