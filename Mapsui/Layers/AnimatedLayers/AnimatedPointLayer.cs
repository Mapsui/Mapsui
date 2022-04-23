using System.Collections.Generic;
using Mapsui.Fetcher;
using Mapsui.Providers;

namespace Mapsui.Layers.AnimatedLayers;

public class AnimatedPointLayer : BaseLayer, ILayerDataSource<IProvider<IFeature>>
{
    private readonly IProvider<PointFeature> _dataSource;
    private FetchInfo? _fetchInfo;
    private readonly AnimatedFeatures _animatedFeatures = new();

    public AnimatedPointLayer(IProvider<PointFeature> dataSource)
    {
        _dataSource = dataSource;
        if (_dataSource is IDynamic dynamic)
            dynamic.DataChanged += (s, e) => {
                UpdateData();
                DataHasChanged();
            };
    }

    public void UpdateData()
    {
        if (_fetchInfo == null) return;

        _animatedFeatures.AddFeatures(_dataSource.GetFeatures(_fetchInfo) ?? new List<PointFeature>());
        OnDataChanged(new DataChangedEventArgs());
    }

    public override MRect? Extent => _dataSource.GetExtent();

    public override IEnumerable<IFeature> GetFeatures(MRect extent, double resolution)
    {
        return _animatedFeatures.GetFeatures();
    }

    public override void RefreshData(FetchInfo fetchInfo)
    {
        _fetchInfo = fetchInfo;
    }

    public override bool UpdateAnimations()
    {
        return _animatedFeatures.UpdateAnimations();
    }

    public IProvider<IFeature>? DataSource => _dataSource;
}
