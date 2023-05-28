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

    public IEnumerable<IFeature> GetFeatures()
    {
        return _features;
    }

    public void SetAnimationTarget(IEnumerable<PointFeature> targets)
    {
        foreach (var target in targets)
        {
            var animatedpointFeature = FindPrevious(_features, target, IdField);
            if (animatedpointFeature is null)
                _features.Add(new AnimatedPointFeature(target));
            else
            {
                animatedpointFeature.SetAnimationTarget(target.Point);
                foreach (var field in target.Fields)
                    animatedpointFeature[field] = target[field];
            }
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

    public bool UpdateAnimations()
    {
        var animating = false;
        foreach (var feature in _features)
        {
            if (feature.UpdateAnimation(AnimationDuration, Function, DistanceThreshold))
                animating = true;
        }
        return animating;
    }
}
