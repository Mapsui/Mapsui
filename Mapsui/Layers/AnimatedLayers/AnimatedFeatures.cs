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
    private List<AnimatedPointFeature> _features = new();

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

    public void UpdateFeatures(IEnumerable<PointFeature> features)
    {
        UpdateStartAndFinish(_features, features, IdField);
    }

    public IEnumerable<IFeature> GetFeatures()
    {
        return _features;
    }

    private void UpdateStartAndFinish(
        List<AnimatedPointFeature> animatedFeatures, IEnumerable<PointFeature> incoming, string idField)
    {
        foreach (var feature in incoming)
        {
            var animatedpointFeature = FindAnimatedPointFeature(animatedFeatures, feature, idField);
            if (animatedpointFeature is null)
                animatedFeatures.Add(new AnimatedPointFeature(feature));
            else
            {
                animatedpointFeature.UpdateAnimation(feature.Point);
                foreach (var field in feature.Fields)
                    animatedpointFeature[field] = feature[field];
            }
        }
    }

    private static AnimatedPointFeature? FindAnimatedPointFeature(IEnumerable<AnimatedPointFeature>? features, IFeature feature,
        string idField)
    {
        // There is no guarantee the idField is set since the features are added by the user. Things do not crash
        // right now because AnimatedPointSample a feature is created with an "ID" field. This is an unresolved
        // issue.
        return features?.FirstOrDefault(f => f[idField]?.Equals(feature[idField]) ?? false);
    }

    public bool UpdateAnimations()
    {
        return InterpolateAnimatedPosition(_features, AnimationDuration, Function, DistanceThreshold);
    }

    private static bool InterpolateAnimatedPosition(IEnumerable<AnimatedPointFeature> items, int duration, EasingFunction function, double distanceThreshold)
    {
        var animating = false;
        foreach (var item in items)
        {
            if (item.UpdateAnimation(duration, function, distanceThreshold))
                animating = true;
        }
        return animating;
    }
}
