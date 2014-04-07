using Mapsui.Geometries;
using Mapsui.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Mapsui.Layers
{
    internal class TransitionAnimator
    {
        private Timer _animation;
        private List<AnimatedItem> _cache = new List<AnimatedItem>();
        private long _startTimeAnimation;

        public TransitionAnimator()
        {
            MillisecondsBetweenUpdates = 16;
            AnimationDuration = 1000;
            IdField = "ID";
        }

        public string IdField { get; set; }
        public int MillisecondsBetweenUpdates { get; set; }
        public int AnimationDuration { get; set; }

        public event EventHandler AnimatedPositionChanged;

        public void AddFeatures(IEnumerable<IFeature> features)
        {
            var previousCache = _cache;
            _cache = ConvertToAnimatedItems(previousCache, features.ToList(), IdField);
            _startTimeAnimation = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (_animation != null) _animation.Dispose();
            _animation = new Timer(AnimationCallback, this, 0, MillisecondsBetweenUpdates);
        }

        public IEnumerable<IFeature> GetFeatures()
        {
            var progress = CalculateAnimationProgress(_startTimeAnimation, AnimationDuration);
            if (NotCompleted(progress)) InterpolateAnimatedPosition(_cache, progress);
            return _cache.Select(f => f.Feature);
        }

        private static bool NotCompleted(double progress)
        {
            return progress < 1;
        }

        protected virtual void OnAnimatedPositionChanged()
        {
            var handler = AnimatedPositionChanged;
            if (handler != null) handler(this, new EventArgs());
        }

        private static void AnimationCallback(object state)
        {
            var animatedPointLayer = (TransitionAnimator)state;
            animatedPointLayer.OnAnimatedPositionChanged();
        }

        private static List<AnimatedItem> ConvertToAnimatedItems(List<AnimatedItem> previousItems,
            IEnumerable<IFeature> features, string idField)
        {
            var animatedFeatureList = new List<AnimatedItem>();
            foreach (var feature in features)
            {
                animatedFeatureList.Add(new AnimatedItem
                {
                    Feature = feature,
                    CurrentPoint = CopyToPoint(feature.Geometry),
                    PreviousPoint = FindPreviousPoint(previousItems, feature, idField)
                });
            }
            return animatedFeatureList;
        }

        private static Point CopyToPoint(IGeometry geometry)
        {
            var point = geometry as Point;
            if (point == null) return null;
            return new Point(point.X, point.Y);
        }

        private static void InterpolateAnimatedPosition(IEnumerable<AnimatedItem> items, double progress)
        {
            foreach (var feature in items)
            {
                if (feature.PreviousPoint == null || feature.CurrentPoint == null) continue;
                var x = feature.PreviousPoint.X + (feature.CurrentPoint.X - feature.PreviousPoint.X) * progress;
                var y = feature.PreviousPoint.Y + (feature.CurrentPoint.Y - feature.PreviousPoint.Y) * progress;
                var point = feature.Feature.Geometry as Point;
                if (point == null) return;
                point.X = x;
                point.Y = y;
            }
        }

        private static Point FindPreviousPoint(IEnumerable<AnimatedItem> previousItems, IFeature feature, 
            string idField)
        {
            if (previousItems == null) return null;
            var previousItem = previousItems.FirstOrDefault(f => f.Feature[idField].Equals(feature[idField]));
            if (previousItem == null) return null;
            return previousItem.Feature.Geometry as Point;
        }

        private static double CalculateAnimationProgress(long startTime, int animationDuration)
        {
            var currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var elapsedTime = currentTime - startTime;
            return elapsedTime / (double)animationDuration;
        }

        private class AnimatedItem
        {
            public IFeature Feature { get; set; }
            public Point PreviousPoint { get; set; }
            public Point CurrentPoint { get; set; }
        }
    }
}