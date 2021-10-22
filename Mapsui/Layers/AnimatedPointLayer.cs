using Mapsui.Fetcher;
using Mapsui.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Extensions;

namespace Mapsui.Layers
{
    public class AnimatedPointLayer : BaseLayer
    {
        private readonly IProvider<IGeometryFeature> _dataSource;
        private MRect _extent;
        private double _resolution;
        private readonly AnimatedFeatures _animatedFeatures = new();

        public AnimatedPointLayer(IProvider<IGeometryFeature> dataSource)
        {
            _dataSource = dataSource;
            _animatedFeatures.AnimatedPositionChanged += (_, _) => OnDataChanged(new DataChangedEventArgs());
        }

        public void UpdateData()
        {
            if (_extent == null) return;
            if (_dataSource == null) return;

            Task.Factory.StartNew(() =>
            {
                _animatedFeatures.AddFeatures(_dataSource.GetFeaturesInView(_extent.ToBoundingBox(), _resolution));
                OnDataChanged(new DataChangedEventArgs());
            });
        }

        public override MRect Envelope => _dataSource?.GetExtents().ToMRect();

        public override IEnumerable<IFeature> GetFeaturesInView(MRect extent, double resolution)
        {
            return _animatedFeatures.GetFeatures();
        }
        public override void RefreshData(MRect extent, double resolution, ChangeType changeType)
        {
            _extent = extent;
            _resolution = resolution;
        }
    }
}
