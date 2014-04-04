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
        private const string PrimaryKey = "ID";
        private readonly string _previousGeometryField = Guid.NewGuid().ToString();
        private readonly string _currentGeometryField = Guid.NewGuid().ToString();
        private long _startTime;
        private const long AnimationDuration = 1000;
        private Timer _timer;
        protected MemoryProvider Cache;
        private IProvider _dataSource;
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
            var previousCache = Cache;

            var featureList = features.ToList();
            Cache = new MemoryProvider(featureList);

            if (previousCache == null) return;

            foreach (var feature in featureList)
            {
                var previousFeature = previousCache.Find(feature[PrimaryKey], PrimaryKey);
                if (previousFeature != null)
                {
                    feature[_previousGeometryField] = previousFeature.Geometry;
                    feature[_currentGeometryField] = feature.Geometry;
                }
            }
            _startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            OnDataChanged(new DataChangedEventArgs());
        }

        public override BoundingBox Envelope
        {
            get { return null; }
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox extent, double resolution)
        {
            var features = Cache.Features;

            var currentTime = DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond;
            var elapsedTime = currentTime - _startTime;
            var portion = elapsedTime/(float)AnimationDuration;
            if (portion > 1) return features;

            foreach (var feature in features)
            {
                var previous = feature[_previousGeometryField] as Point;
                var current = feature[_currentGeometryField] as Point;
                if (previous == null || current == null) continue;
                var x = previous.X + (current.X - previous.X) * portion;
                var y = previous.Y + (current.Y - previous.Y) * portion;
                feature.Geometry = new Point(x, y);
            }

            return features;
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
