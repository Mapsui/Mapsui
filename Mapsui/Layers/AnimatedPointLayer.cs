using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Fetcher;
using Mapsui.Providers;

namespace Mapsui.Layers
{
    public class AnimatedPointLayer : BaseLayer
    {
        private readonly IProvider<IPointFeature> _dataSource;
        private FetchInfo _fetchInfo;
        private readonly AnimatedFeatures _animatedFeatures = new();

        public AnimatedPointLayer(IProvider<IPointFeature> dataSource)
        {
            _dataSource = dataSource;
            _animatedFeatures.AnimatedPositionChanged += (_, _) => OnDataChanged(new DataChangedEventArgs());
        }

        public void UpdateData()
        {
            if (_fetchInfo == null) return;
            if (_dataSource == null) return;

            Task.Factory.StartNew(() => {
                _animatedFeatures.AddFeatures(_dataSource.GetFeatures(_fetchInfo));
                OnDataChanged(new DataChangedEventArgs());
            });
        }

        public override MRect? Envelope => _dataSource.GetExtent();

        public override IEnumerable<IFeature> GetFeatures(MRect extent, double resolution)
        {
            return _animatedFeatures.GetFeatures();
        }

        public override void RefreshData(FetchInfo fetchInfo)
        {
            _fetchInfo = fetchInfo;
        }
    }
}
