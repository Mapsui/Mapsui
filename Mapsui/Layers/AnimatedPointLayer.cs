using BruTile;
using BruTile.Predefined;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Timers;

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
        //private readonly string _previousGeometryField = Guid.NewGuid().ToString();
        //private readonly string _currentGeometryField = Guid.NewGuid().ToString();
        private long _startTime;
        private const long AnimationDuration = 1000;
        private List<AnimatedPointFeature> _cache;
        private readonly IProvider _dataSource;
        private Timer _timer;
        private Timer _animation;

        public AnimatedPointLayer(IProvider dataSource)
        {
            _dataSource = dataSource;
            _timer = new Timer(Callback, this, 0, 2000);
            _animation = new Timer(AnimationCallback, this, 0, 9);
        }

        private static void Callback(object state)
        {
            var animatedPointLayer = (AnimatedPointLayer)state;
            var extent = new SphericalMercatorInvertedWorldSchema().Extent.ToBoundingBox();
            var features = animatedPointLayer._dataSource.GetFeaturesInView(extent, 0);
            animatedPointLayer.DataArrived(features);
        }

        private static void AnimationCallback(object state)
        {
            var animatedPointLayer = (AnimatedPointLayer)state;
            animatedPointLayer.OnDataChanged(new DataChangedEventArgs());
        }

        protected void DataArrived(IEnumerable<IFeature> features, object state = null)
        {
            var previousCache = _cache;

            _cache = ConvertToAnimatedFeatures(previousCache, features.ToList());
            
            if (previousCache == null) return;

            _startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            OnDataChanged(new DataChangedEventArgs());
        }

        private List<AnimatedPointFeature> ConvertToAnimatedFeatures(List<AnimatedPointFeature> previousFeatures, IEnumerable<IFeature> features)
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
            if (previousFeature != null)
            {
                animatedPointFeature.PreviousPoint = previousFeature.Geometry as Point;
                animatedPointFeature.CurrentPoint = animatedPointFeature.Geometry as Point;
            }
        }

        public override BoundingBox Envelope
        {
            get { return null; }
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox extent, double resolution)
        {
            var progression = CalculateProgressionAnimation(_startTime);
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

        private static double CalculateProgressionAnimation(long startTime)
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

        }

        public override void ClearCache()
        {
        }
    }
}
