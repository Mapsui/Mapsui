using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers.AnimationLayers;
using Mapsui.Providers;

namespace Mapsui.Layers.AnimatedLayers;

public class AnimatedPointLayer : BaseLayer, IAsyncDataFetcher, ILayerDataSource<IProvider>
{
    private readonly IProvider _dataSource;
    private FetchInfo? _fetchInfo;
    private readonly List<AnimatedPointFeature> _features = [];

    public AnimatedPointLayer(IProvider dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        if (_dataSource is IDynamic dynamic)
            dynamic.DataChanged += Dynamic_DataChanged;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_dataSource is IDynamic dynamic)
            {
                dynamic.DataChanged -= Dynamic_DataChanged;
            }
            _disposed = true;
        }
    }
    private void Dynamic_DataChanged(object? sender, EventArgs e)
    {
        if (_fetchInfo is FetchInfo fetchInfo)
        {
            UpdateFeatures(fetchInfo);
        }
    }

    private void UpdateFeatures(FetchInfo fetchInfo)
    {
        Catch.Exceptions(async () =>
        {
            await UpdateFeaturesAsync(fetchInfo);
            DataHasChanged();
        });
    }


    /// <summary>
    /// When the distance between the current and the previous position is larger
    /// than the DistanceThreshold it will not be animated. 
    /// The default is Double.MaxValue
    /// </summary>
    public double DistanceThreshold { get; set; } = double.MaxValue;
    public string IdField { get; set; } = "ID";

    /// <summary>
    /// The period of which the animation should move from the previous position to the new position.
    /// The default is 1000 milliseconds.
    /// </summary>
    public int AnimationDuration { get; set; } = 1000;

    /// <summary>
    /// The easing function to use for the animation. The default is Easing.CubicOut.
    /// </summary>
    public Easing Easing { get; set; } = Easing.CubicOut;

    private async Task UpdateFeaturesAsync(FetchInfo fetchInfo)
    {
        var features = await _dataSource.GetFeaturesAsync(fetchInfo);
        SetAnimationTarget(features.Cast<PointFeature>());
        OnDataChanged(new DataChangedEventArgs(Name));
    }

    public void SetAnimationTarget(IEnumerable<PointFeature> targets)
    {
        foreach (var target in targets)
        {
            var animatedPointFeature = FindPrevious(_features, target, IdField);
            if (animatedPointFeature is null)
            {
                animatedPointFeature = new AnimatedPointFeature(target.Point.X, target.Point.Y);
                _features.Add(animatedPointFeature);
            }
            else
            {
                animatedPointFeature.SetAnimationTarget(target.Point);
            }
            foreach (var field in target.Fields)
                animatedPointFeature[field] = target[field];
        }
    }

    private static AnimatedPointFeature? FindPrevious(IEnumerable<AnimatedPointFeature>? features,
        PointFeature feature, string idField)
    {
        // There is no guarantee the idField is set since the features are added by the user. Things do not crash
        // right now because AnimatedPointSample a feature is created with an "ID" field. This is an unresolved
        // issue.
        return features?.FirstOrDefault(f => f[idField]?.Equals(feature[idField]) ?? false);
    }

    public override MRect? Extent => _dataSource.GetExtent();

    public override IEnumerable<IFeature> GetFeatures(MRect extent, double resolution)
    {
        return _features;
    }

    public void RefreshData(FetchInfo fetchInfo)
    {
        if (_fetchInfo is null) // On the first call _fetchInfo is null and we should initialize the features.
            UpdateFeatures(fetchInfo);

        _fetchInfo = fetchInfo;
    }

    public override bool UpdateAnimations()
    {
        var animating = false;
        foreach (var feature in _features)
        {
            if (feature.UpdateAnimation(AnimationDuration, Easing, DistanceThreshold))
                animating = true;
        }
        return animating;
    }

    public void AbortFetch()
    {
    }

    public void ClearCache()
    {
    }

    public IProvider? DataSource => _dataSource;
}
