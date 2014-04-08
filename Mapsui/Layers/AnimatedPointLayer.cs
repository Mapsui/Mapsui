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
        private readonly AnimatedFeatures _animatedFeatures = new AnimatedFeatures();

        public AnimatedPointLayer(IProvider dataSource)
        {
            _dataSource = dataSource;
            _animatedFeatures.AnimatedPositionChanged += (sender, args) => OnDataChanged(new DataChangedEventArgs());
        }

        public void UpdateData()
        {
            if (_extent == null) return;
            if (_dataSource == null) return;

            Task.Factory.StartNew(() =>
            {
                _animatedFeatures.AddFeatures(_dataSource.GetFeaturesInView(_extent, _resolution));
                OnDataChanged(new DataChangedEventArgs());
            });
        }

        public override BoundingBox Envelope
        {
            get { return (_dataSource == null) ? null : _dataSource.GetExtents(); }
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox extent, double resolution)
        {
            return _animatedFeatures.GetFeatures();
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
