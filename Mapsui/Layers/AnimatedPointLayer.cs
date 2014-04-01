using Mapsui.Geometries;
using Mapsui.Providers;
using System;
using System.Collections.Generic;

namespace Mapsui.Layers
{
    internal class FeatureWithHistory : Feature
    {
        private IGeometry PreviousGeometry { get; set; }
    }

    public class AnimatedPointLayer : Layer
    {
        private const string PrimaryKey = "id";
        private readonly string _previousGeometryField = Guid.NewGuid().ToString();
        private readonly string _currentGeometryField = Guid.NewGuid().ToString();
        private long _startTime;
        private long _animationDuration;

        protected new void DataArrived(IEnumerable<IFeature> features, object state = null)
        {
            // before DataArrived keep pointer to original cache
            var previousCache = Cache;

            // call DataArrived
            base.DataArrived(features, state);

            // assign previous geometries.
            foreach (var feature in Cache.Features)
            {
                var previousFeature = previousCache.Find(feature[PrimaryKey]);
                if (previousFeature != null)
                {
                    feature[_previousGeometryField] = previousFeature.Geometry;
                    feature[_currentGeometryField] = feature.Geometry;
                }
            }
            _startTime = DateTime.Now.Ticks;
        }
        
        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox extent, double resolution)
        {
            var features = base.GetFeaturesInView(extent, resolution);
            foreach (var feature in features)
            {
                feature.Geometry = new Point();
            }
            return features;
        }
    }
}
