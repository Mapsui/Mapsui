using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Layers
{
    public class AnimatedPointLayer : BaseLayer
    {
        private readonly IProvider _dataSource;
        private BoundingBox _extent;
        private double _resolution;
        private readonly AnimationBuffer _animationBuffer = new AnimationBuffer();

        public AnimatedPointLayer(IProvider dataSource)
        {
            _dataSource = dataSource;
            _animationBuffer.AnimatedPositionChanged += (sender, args) => OnDataChanged(new DataChangedEventArgs());
        }

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
            _animationBuffer.AddFeatures(features);
            OnDataChanged(new DataChangedEventArgs());
        }

        public override BoundingBox Envelope
        {
            get { return (_dataSource == null) ? null : _dataSource.GetExtents(); }
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox extent, double resolution)
        {
            return _animationBuffer.GetFeatures();
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
