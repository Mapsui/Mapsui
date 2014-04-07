using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Layers
{
    public class AnimatedPointLayer : BaseLayer
    {
        private class AnimatedPointFeature : Feature
        {
            public AnimatedPointFeature(Feature feature) : base(feature)
            {
            }

            public Point PreviousPoint { get; set; }
            public Point CurrentPoint { get; set; }
        }
        
        private const string PrimaryKey = "ID";
        private long _startTimeAnimation;
        private const long AnimationDuration = 1000;
        private List<AnimatedPointFeature> _cache = new List<AnimatedPointFeature>();
        private readonly IProvider _dataSource;
        private Timer _animation;
        private BoundingBox _extent;
        private double _resolution;

        public AnimatedPointLayer(IProvider dataSource)
        {
            _dataSource = dataSource;
            MilisecondsBetweenUpdates = 16;
        }

        public int MilisecondsBetweenUpdates { get; set; }

        public void UpdateData()
        {
            if (_extent == null) return;
            if (_dataSource == null) return;

            Task.Factory.StartNew(() =>
            {
                var features = _dataSource.GetFeaturesInView(_extent, _resolution);
                DataArrived(features);
            });
        }

        protected void DataArrived(IEnumerable<IFeature> features, object state = null)
        {
            var previousCache = _cache;

            _cache = ConvertToAnimatedFeatures(previousCache, features.ToList());
            _startTimeAnimation = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (_animation != null) _animation.Dispose();
            _animation = new Timer(AnimationCallback, this, 0, MilisecondsBetweenUpdates);

            OnDataChanged(new DataChangedEventArgs());
        }

        private static void AnimationCallback(object state)
        {
            var animatedPointLayer = (AnimatedPointLayer)state;
            animatedPointLayer.OnDataChanged(new DataChangedEventArgs());
        }

        private static List<AnimatedPointFeature> ConvertToAnimatedFeatures(List<AnimatedPointFeature> previousFeatures, IEnumerable<IFeature> features)
        {
            var animatedFeatureList = new List<AnimatedPointFeature>();
            foreach (var feature in features)
            {
                var animatedPointFeature = new AnimatedPointFeature((Feature)feature);
                if (previousFeatures != null)
                {
                    LookupPreviousState(previousFeatures, feature, animatedPointFeature);
                }
                animatedFeatureList.Add(animatedPointFeature);
            }
            return animatedFeatureList;
        }

        private static void LookupPreviousState(IEnumerable<AnimatedPointFeature> previousFeatures, IFeature feature,
            AnimatedPointFeature animatedPointFeature)
        {
            var previousFeature = previousFeatures.FirstOrDefault(f => f[PrimaryKey].Equals(feature[PrimaryKey]));
            if (previousFeature == null) return;
            animatedPointFeature.PreviousPoint = previousFeature.Geometry as Point;
            animatedPointFeature.CurrentPoint = animatedPointFeature.Geometry as Point;
        }

        public override BoundingBox Envelope
        {
            get { return (_dataSource == null) ? null : _dataSource.GetExtents(); }
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox extent, double resolution)
        {
            var progression = CalculateAnimationProgress(_startTimeAnimation);
            if (progression < 1) InterpolateAnimatedPosition(_cache, progression);
            return _cache;
        }

        private static void InterpolateAnimatedPosition(IEnumerable<AnimatedPointFeature> features, double progression)
        {
            foreach (var feature in features)
            {
                if (feature.PreviousPoint == null || feature.CurrentPoint == null) continue;
                var x = feature.PreviousPoint.X + (feature.CurrentPoint.X - feature.PreviousPoint.X) * progression;
                var y = feature.PreviousPoint.Y + (feature.CurrentPoint.Y - feature.PreviousPoint.Y) * progression;
                feature.Geometry = new Point(x, y);
            }
        }

        private static double CalculateAnimationProgress(long startTime)
        {
            var currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var elapsedTime = currentTime - startTime;
            return elapsedTime / (double)AnimationDuration;
        }

        public override void AbortFetch()
        {
        }

        public override void ViewChanged(bool changeEnd, BoundingBox extent, double resolution)
        {
            _extent = extent;
            _resolution = resolution;
        }

        public override void ClearCache()
        {
        }
    }
}
