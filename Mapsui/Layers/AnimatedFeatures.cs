using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mapsui.Providers;

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
        private List<AnimatedItem> _cache = new();
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

        public void AddFeatures(IEnumerable<IPointFeature> features)
        {
            var previousCache = _cache;

            _cache = ConvertToAnimatedItems(features.ToList(), previousCache, IdField);
            _startTimeAnimation = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            _animationTimer.Change(_millisecondsBetweenUpdates, _millisecondsBetweenUpdates);
        }

        public IEnumerable<IFeature> GetFeatures()
        {
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
            AnimatedPositionChanged?.Invoke(this, EventArgs.Empty);
        }

        private static void AnimationCallback(object state)
        {
            var animatedPointLayer = (AnimatedFeatures)state;
            animatedPointLayer.OnAnimatedPositionChanged();
        }

        private static List<AnimatedItem> ConvertToAnimatedItems(IEnumerable<IPointFeature> features,
            List<AnimatedItem> previousItems, string idField)
        {
            var result = new List<AnimatedItem>();
            foreach (var feature in features)
            {
                var previousPoint = FindPreviousPoint(previousItems, feature, idField);
                var animatedItem = new AnimatedItem
                {
                    Feature = feature,
                    CurrentPoint = new MPoint(feature.Point),
                    PreviousPoint = previousPoint == null ? null : new MPoint(previousPoint)
                };
                result.Add(animatedItem);
            }
            return result;
        }

        private static void InterpolateAnimatedPosition(IEnumerable<AnimatedItem> items, double progress, double threshold)
        {
            foreach (var item in items)
            {
                var target = new MPoint(item.Feature.Point);
                if (item.PreviousPoint == null || item.CurrentPoint == null || target == null) continue;
                if (item.PreviousPoint.Distance(item.CurrentPoint) > threshold) continue;
                target.X = item.PreviousPoint.X + (item.CurrentPoint.X - item.PreviousPoint.X) * progress;
                target.Y = item.PreviousPoint.Y + (item.CurrentPoint.Y - item.PreviousPoint.Y) * progress;
            }
        }

        private static MPoint FindPreviousPoint(IEnumerable<AnimatedItem>? previousItems, IFeature feature,
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
            public IPointFeature? Feature { get; set; }
            public MPoint? PreviousPoint { get; set; }
            public MPoint? CurrentPoint { get; set; }
        }
    }
}