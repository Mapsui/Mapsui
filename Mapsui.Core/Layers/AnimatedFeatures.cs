using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private List<AnimatedFeature> _cache = new();
        private long _startTimeAnimation;
        private readonly int _millisecondsBetweenUpdates;

        /// <summary>
        /// When the distance between the current and the previous position is larger
        /// than the DistanceThreshold it will not be animated. 
        /// The default is Double.MaxValue
        /// </summary>
        public double DistanceThreshold { get; set; }

        public AnimatedFeatures(int millisecondsBetweenUpdates = 8)
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

        public event EventHandler? AnimatedPositionChanged;

        public void AddFeatures(IEnumerable<PointFeature> features)
        {
            var previousCache = _cache;

            _cache = ConvertToAnimatedFeatures(features.ToList(), previousCache, IdField);
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
            return _cache.Select(i => i.Feature);
        }

        private static bool Completed(double progress)
        {
            return progress >= 1;
        }

        protected virtual void OnAnimatedPositionChanged()
        {
            AnimatedPositionChanged?.Invoke(this, EventArgs.Empty);
        }

        private static void AnimationCallback(object state)
        {
            var animatedPointLayer = (AnimatedFeatures)state;
            animatedPointLayer.OnAnimatedPositionChanged();
        }

        private static void LogAllFeatures(IEnumerable<AnimatedFeature> animatedFeatures)
        {
            if (!_first) return;
            _first = false;
            _counter++;

            foreach (var animatedFeature in animatedFeatures)
            {
                var target = animatedFeature.Feature?.Point;
                if (animatedFeature.PreviousPoint == null || animatedFeature.CurrentPoint == null || target == null) continue;
                if (animatedFeature.PreviousPoint.Distance(animatedFeature.CurrentPoint) < 10000) continue;
            }
        }

        private static List<AnimatedFeature> ConvertToAnimatedFeatures(
            IEnumerable<PointFeature> features, List<AnimatedFeature> previousItems, string idField)
        {
            return features.Select(f => new AnimatedFeature(f, FindPreviousPoint(previousItems, f, idField))).ToList();
        }

        private static int _counter;
        private static bool _first = true;

        private static void InterpolateAnimatedPosition(IEnumerable<AnimatedFeature> items, double progress, double threshold)
        {
            foreach (var item in items)
            {
                var target = item.Feature?.Point;
                if (item.PreviousPoint == null || item.CurrentPoint == null || target == null) continue;
                if (item.PreviousPoint.Distance(item.CurrentPoint) > threshold) continue;
                target.X = item.PreviousPoint.X + (item.CurrentPoint.X - item.PreviousPoint.X) * progress;
                target.Y = item.PreviousPoint.Y + (item.CurrentPoint.Y - item.PreviousPoint.Y) * progress;
            }
        }

        private static MPoint? FindPreviousPoint(IEnumerable<AnimatedFeature>? previousItems, IFeature feature,
            string idField)
        {
            // There is no guarantee the idField is set since the features are added by the user. Things do not crash
            // right now because AnimatedPointSample a feature is created with an "ID" field. This is an unresolved
            // issue.
            return previousItems?.FirstOrDefault(f => f.Feature[idField]?.Equals(feature[idField]) ?? false)?.CurrentPoint;
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
            return ((t = (t / d) - 1) * t * t) + 1;
        }

        private class AnimatedFeature
        {
            public AnimatedFeature(PointFeature feature, MPoint? previousPoint)
            {
                Feature = feature;
                CurrentPoint = new MPoint(feature.Point);
                if (previousPoint != null)
                    PreviousPoint = new MPoint(previousPoint);
            }
            public PointFeature Feature { get; }
            public MPoint? CurrentPoint { get; set; }
            public MPoint? PreviousPoint { get; }

            public override string ToString()
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("Trackee: " + Feature?["Trackee"]);
                stringBuilder.Append(", ID: " + Feature?["ID"]);
                stringBuilder.Append(", speed: " + Feature?["Speed"]);
                stringBuilder.Append(", Bps: " + Feature?["Bps"]);
                stringBuilder.Append(", DateGps: " + Feature?["DateGps"]);
                stringBuilder.Append(", DateReceived: " + Feature?["DateReceived"]);
                stringBuilder.Append(", Longitude: " + Feature?["Longitude"]);
                stringBuilder.Append(", Latitude: " + Feature?["Latitude"]);
                stringBuilder.Append(", X: " + CurrentPoint?.X);
                stringBuilder.Append(", Y: " + CurrentPoint?.Y);
                stringBuilder.Append(", Previous X: " + PreviousPoint?.X);
                stringBuilder.Append(", Previous Y: " + PreviousPoint?.Y);
                return stringBuilder.ToString();
            }
        }
    }
}
