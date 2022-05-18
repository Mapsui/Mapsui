using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Animations;
using Mapsui.Layers.AnimationLayers;

namespace Mapsui.Layers.AnimatedLayers;

public enum EasingFunction
{
    CubicEaseOut,
    Linear
}

public class AnimatedFeatures : IAnimatable
{
    private List<AnimatedFeature> _cache = new();
    private long _startTimeAnimation;
    private bool _animating = false;

    /// <summary>
    /// When the distance between the current and the previous position is larger
    /// than the DistanceThreshold it will not be animated. 
    /// The default is Double.MaxValue
    /// </summary>
    public double DistanceThreshold { get; set; }

    public AnimatedFeatures()
    {
        AnimationDuration = 1000;
        IdField = "ID";
        Function = EasingFunction.CubicEaseOut;
        DistanceThreshold = double.MaxValue;
    }

    public string IdField { get; set; }
    public int AnimationDuration { get; set; }
    public EasingFunction Function { get; set; }

    public void AddFeatures(IEnumerable<PointFeature> features)
    {
        var previousCache = _cache;

        _cache = ConvertToAnimatedFeatures(features.ToList(), previousCache, IdField);
        _startTimeAnimation = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        _animating = true;
        _first = true;
    }

    public IEnumerable<IFeature> GetFeatures()
    {
        LogAllFeatures(_cache);
        return _cache.Select(i => i.Feature);
    }

    private static bool Completed(double progress)
    {
        return progress >= 1;
    }

    private static void LogAllFeatures(IEnumerable<AnimatedFeature> animatedFeatures)
    {
        if (!_first) return;
        _first = false;

        foreach (var animatedFeature in animatedFeatures)
        {
            var target = animatedFeature.Feature?.Point;
            if (animatedFeature.PreviousPoint == null || animatedFeature.CurrentPoint == null || target == null) continue;
            if (animatedFeature.PreviousPoint.Distance(animatedFeature.CurrentPoint) < 10000) continue;
        }
    }

    private static List<AnimatedFeature> ConvertToAnimatedFeatures(
        IEnumerable<PointFeature> features, List<AnimatedFeature> previousItems, string idField)
    {
        return features.Select(f => new AnimatedFeature(f, FindPreviousPoint(previousItems, f, idField))).ToList();
    }

    private static bool _first = true;

    private static void InterpolateAnimatedPosition(IEnumerable<AnimatedFeature> items, double progress, double threshold)
    {
        foreach (var item in items)
        {
            var target = item.Feature?.Point;
            if (item.PreviousPoint == null || item.CurrentPoint == null || target == null) continue;
            if (item.PreviousPoint.Distance(item.CurrentPoint) > threshold) continue;
            target.X = item.PreviousPoint.X + (item.CurrentPoint.X - item.PreviousPoint.X) * progress;
            target.Y = item.PreviousPoint.Y + (item.CurrentPoint.Y - item.PreviousPoint.Y) * progress;
        }
    }

    private static MPoint? FindPreviousPoint(IEnumerable<AnimatedFeature>? previousItems, IFeature feature,
        string idField)
    {
        // There is no guarantee the idField is set since the features are added by the user. Things do not crash
        // right now because AnimatedPointSample a feature is created with an "ID" field. This is an unresolved
        // issue.
        return previousItems?.FirstOrDefault(f => f.Feature[idField]?.Equals(feature[idField]) ?? false)?.CurrentPoint;
    }

    private static double CalculateProgress(long startTime, int animationDuration, EasingFunction function)
    {
        var currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        var elapsedTime = (double)currentTime - startTime;

        if (function == EasingFunction.CubicEaseOut)
            return CubicEaseOut(animationDuration, elapsedTime);
        return Linear(animationDuration, elapsedTime);
    }

    private static double Linear(double d, double t)
    {
        return t / d;
    }

    private static double CubicEaseOut(double d, double t)
    {
        return (t = t / d - 1) * t * t + 1;
    }

    public bool UpdateAnimations()
    {
        var progress = CalculateProgress(_startTimeAnimation, AnimationDuration, Function);
        if (!Completed(progress)) InterpolateAnimatedPosition(_cache, progress, DistanceThreshold);
        else _animating = false;

        return _animating;
    }
}
