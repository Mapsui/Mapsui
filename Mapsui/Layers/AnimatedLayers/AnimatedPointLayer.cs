using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Fetcher;
using Mapsui.Providers;

namespace Mapsui.Layers.AnimatedLayers;

public class AnimatedPointLayer : BaseLayer, ILayerDataSource<IProvider>
{
    private readonly IProvider _dataSource;
    private FetchInfo? _fetchInfo;
    private readonly AnimatedFeatures _animatedFeatures = new();

    public AnimatedPointLayer(IProvider dataSource)
    {
        _dataSource = dataSource;
        if (_dataSource is IDynamic dynamic)
            dynamic.DataChanged += async (s, e) =>
            {
                await UpdateDataAsync();
                DataHasChanged();
            };
    }

    public async Task UpdateDataAsync()
    {
        if (_fetchInfo is null) return;
        if (_dataSource is null) return;
        var features = await _dataSource.GetFeaturesAsync(_fetchInfo).ToListAsync();
        _animatedFeatures.AddFeatures(features.Cast<PointFeature>());
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

    public IProvider? DataSource => _dataSource;
}