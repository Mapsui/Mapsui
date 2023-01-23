using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Providers;

namespace Mapsui.Layers.AnimatedLayers;

public class AnimatedPointLayer : BaseLayer, IAsyncDataFetcher, ILayerDataSource<IProvider>
{
    private readonly IProvider _dataSource;
    private FetchInfo? _fetchInfo;
    private readonly AnimatedFeatures _animatedFeatures = new();

    [SuppressMessage("Usage", "VSTHRD101:Avoid unsupported async delegates")]
    public AnimatedPointLayer(IProvider dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentException(nameof(dataSource));
        if (_dataSource is IDynamic dynamic)
            dynamic.DataChanged += (s, e) =>
            {
                Catch.Exceptions(async () =>
                {
                    await UpdateDataAsync();
                    DataHasChanged();
                });
            };
    }

    public async Task UpdateDataAsync()
    {
        if (_fetchInfo is null) return;
        var features = await _dataSource.GetFeaturesAsync(_fetchInfo);
        _animatedFeatures.AddFeatures(features.Cast<PointFeature>());
        OnDataChanged(new DataChangedEventArgs());
    }

    public override MRect? Extent => _dataSource.GetExtent();

    public override IEnumerable<IFeature> GetFeatures(MRect extent, double resolution)
    {
        return _animatedFeatures.GetFeatures();
    }

    public void RefreshData(FetchInfo fetchInfo)
    {
        _fetchInfo = fetchInfo;
    }

    public override bool UpdateAnimations()
    {
        return _animatedFeatures.UpdateAnimations();
    }

    public void AbortFetch()
    {
    }

    public void ClearCache()
    {
    }

    public IProvider? DataSource => _dataSource;
}
