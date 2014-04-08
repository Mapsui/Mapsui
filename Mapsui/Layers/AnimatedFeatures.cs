using System.Threading.Timers;
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

    internal class AnimatedFeatures
    {
        private Timer _animation;
        private List<AnimatedItem> _cache = new List<AnimatedItem>();
        private long _startTimeAnimation;
        
        public AnimatedFeatures()
        {
            MillisecondsBetweenUpdates = 16;
            AnimationDuration = 1000;
            IdField = "ID";
            Function = EasingFunction.CubicEaseOut;
        }

        public string IdField { get; set; }
        public int MillisecondsBetweenUpdates { get; set; }
        public int AnimationDuration { get; set; }
        public EasingFunction Function { get; set; }

        public event EventHandler AnimatedPositionChanged;

        public void AddFeatures(IEnumerable<IFeature> features)
        {
            var previousCache = _cache;
            _cache = ConvertToAnimatedItems(previousCache, features.ToList(), IdField);
            _startTimeAnimation = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (_animation != null) StopAnimation();
            _animation = new Timer(AnimationCallback, this, 0, MillisecondsBetweenUpdates);
        }

        private void StopAnimation()
        {
            _animation.Stop();
            _animation.Dispose();
            _animation = null;
        }

        public IEnumerable<IFeature> GetFeatures()
        {
            var progress = CalculateProgress(_startTimeAnimation, AnimationDuration, Function);
            if (!Completed(progress)) InterpolateAnimatedPosition(_cache, progress);
            else if (_animation != null) StopAnimation();
            return _cache.Select(f => f.Feature);
        }

        private static bool Completed(double progress)
        {
            return progress >= 1;
        }

        protected virtual void OnAnimatedPositionChanged()
        {
            var handler = AnimatedPositionChanged;
            if (handler != null) handler(this, new EventArgs());
        }

        private static void AnimationCallback(object state)
        {
            var animatedPointLayer = (AnimatedFeatures)state;
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
                    CurrentPoint = CopyAsPoint(feature.Geometry),
                    PreviousPoint = CopyAsPoint(FindPreviousGeometry(previousItems, feature, idField))
                });
            }
            return animatedFeatureList;
        }

        private static Point CopyAsPoint(IGeometry geometry)
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

        private static Geometry FindPreviousGeometry(IEnumerable<AnimatedItem> previousItems, IFeature feature,
            string idField)
        {
            if (previousItems == null) return null;
            var previousItem = previousItems.FirstOrDefault(f => f.Feature[idField].Equals(feature[idField]));
            if (previousItem == null) return null;
            return previousItem.CurrentPoint;
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