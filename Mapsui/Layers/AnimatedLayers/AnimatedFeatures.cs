using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// When the distance between the current and the previous position is larger
    /// than the DistanceThreshold it will not be animated. 
    /// The default is Double.MaxValue
    /// </summary>
    public double DistanceThreshold { get; set; }

    public AnimatedFeatures()
    {
        AnimationDuration = 1000;
        // Todo: There should be a assignable function to find the previous feature, so the user has all flexibility
        IdField = "ID";
        // Todo: Use the animation functions from Mapsui.Animations
        Function = EasingFunction.CubicEaseOut;
        DistanceThreshold = double.MaxValue;
    }

    public string IdField { get; set; }
    public int AnimationDuration { get; set; }
    public EasingFunction Function { get; set; }

    public void AddFeatures(IEnumerable<PointFeature> features)
    {
        _cache = ConvertToAnimatedFeatures(features.ToList(), _cache, IdField);
    }

    public IEnumerable<IFeature> GetFeatures()
    {
        return _cache.Select(i => i.Feature).ToList();
    }

    private static List<AnimatedFeature> ConvertToAnimatedFeatures(
        IEnumerable<PointFeature> features, List<AnimatedFeature> previousFeatures, string idField)
    {
        return features.Select(f => new AnimatedFeature(f, FindPreviousFeature(previousFeatures, f, idField)?.Destination)).ToList();
    }

    private static AnimatedFeature? FindPreviousFeature(IEnumerable<AnimatedFeature>? previousItems, IFeature feature,
        string idField)
    {
        // There is no guarantee the idField is set since the features are added by the user. Things do not crash
        // right now because AnimatedPointSample a feature is created with an "ID" field. This is an unresolved
        // issue.
        return previousItems?.FirstOrDefault(f => f.Feature[idField]?.Equals(feature[idField]) ?? false);
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
        return InterpolateAnimatedPosition(_cache, AnimationDuration, Function, DistanceThreshold);
    }

    private static bool InterpolateAnimatedPosition(IEnumerable<AnimatedFeature> items, int duration, EasingFunction function, double distanceThreshold)
    {
        var animating = false;
        foreach (var item in items)
        {
            var progress = CalculateProgress(item.StartTimeInTicks, duration, function);
            if (progress < 1) animating = true;
            var target = item.Feature?.Point;
            if (item.Origin == null || item.Destination == null || target == null) continue;
            if (item.Origin.Distance(item.Destination) > distanceThreshold) continue;
            target.X = item.Origin.X + (item.Destination.X - item.Origin.X) * progress;
            target.Y = item.Origin.Y + (item.Destination.Y - item.Origin.Y) * progress;
        }
        return animating;
    }

    private static double CalculateProgress(long startTime, int animationDuration, EasingFunction function)
    {
        var currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        var elapsedTime = currentTime - startTime;

        if (function == EasingFunction.CubicEaseOut)
            return Math.Min(CubicEaseOut(animationDuration, elapsedTime), 1);
        return Math.Min(Linear(animationDuration, elapsedTime), 1);
    }
}
