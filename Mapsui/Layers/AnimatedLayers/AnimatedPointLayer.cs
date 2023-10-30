using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers.AnimationLayers;
using Mapsui.Providers;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Layers.AnimatedLayers;

public class AnimatedPointLayer : BaseLayer, IAsyncDataFetcher, ILayerDataSource<IProvider>
{
    private readonly IProvider _dataSource;
    private FetchInfo? _fetchInfo;
    private List<AnimatedPointFeature> _features = new();

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

        // Todo: There should be a assignable function to find the previous feature, so the user has all flexibility
        IdField = "ID";
    }

    /// <summary>
    /// When the distance between the current and the previous position is larger
    /// than the DistanceThreshold it will not be animated. 
    /// The default is Double.MaxValue
    /// </summary>
    public double DistanceThreshold { get; set; } = double.MaxValue;
    public string IdField { get; set; }

    /// <summary>
    /// The period of which the animaton should move from the previous position to the new position.
    /// The default is 1000 milliseconds.
    /// </summary>
    public int AnimationDuration { get; set; } = 1000;

    /// <summary>
    /// The easing function to use for the animation. The default is Easing.CubicOut.
    /// </summary>
    public Easing Easing { get; set; } = Easing.CubicOut;


    public async Task UpdateDataAsync()
    {
        if (_fetchInfo is null) return;

        var features = await _dataSource.GetFeaturesAsync(_fetchInfo);
        SetAnimationTarget(features.Cast<PointFeature>());
        OnDataChanged(new DataChangedEventArgs());
    }

    public void SetAnimationTarget(IEnumerable<PointFeature> targets)
    {
        foreach (var target in targets)
        {
            var animatedpointFeature = FindPrevious(_features, target, IdField);
            if (animatedpointFeature is null)
            {
                animatedpointFeature = new AnimatedPointFeature(target.Point.X, target.Point.Y);
                _features.Add(animatedpointFeature);
            }
            else
            {
                animatedpointFeature.SetAnimationTarget(target.Point);
            }
            foreach (var field in target.Fields)
                animatedpointFeature[field] = target[field];
        }
    }

    private static AnimatedPointFeature? FindPrevious(IEnumerable<AnimatedPointFeature>? features, IFeature feature,
    string idField)
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
