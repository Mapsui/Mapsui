using Mapsui.Animations;
using System;

namespace Mapsui.Layers.AnimatedLayers;

/// <summary>
/// A point feature that smoothly interpolates its position between two geographic locations.
/// Animation settings (duration, easing, and distance threshold) are specified in the constructor,
/// allowing individual features within the same layer to have different animation behaviors.
/// </summary>
public class AnimatedPointFeature : PointFeature, IAnimatedFeature
{
    private long _startTime;
    private readonly int _animationDuration;
    private readonly Easing _easing;
    private readonly double _distanceThreshold;

    public AnimatedPointFeature(AnimatedPointFeature animatedPointFeature) : base(animatedPointFeature)
    {
        _startTime = animatedPointFeature._startTime;
        _animationDuration = animatedPointFeature._animationDuration;
        _easing = animatedPointFeature._easing;
        _distanceThreshold = animatedPointFeature._distanceThreshold;
        Start = animatedPointFeature.Start;
        End = animatedPointFeature.End;
    }

    public AnimatedPointFeature(double x, double y, int animationDuration = 1000, Easing? easing = null,
        double distanceThreshold = double.MaxValue) : base(x, y)
    {
        _animationDuration = animationDuration;
        _easing = easing ?? Easing.CubicOut;
        _distanceThreshold = distanceThreshold;
        Start = new MPoint(x, y);
        End = new MPoint(x, y);
    }

    public MPoint End { get; set; }
    public MPoint Start { get; set; }

    public void SetAnimationTarget(MPoint target)
    {
        Start = new MPoint(End); // Start where the previous animation ended
        End = new MPoint(target);

        Point.X = Start.X;
        Point.Y = Start.Y;

        _startTime = Environment.TickCount64;
    }

    /// <inheritdoc/>
    public bool UpdateAnimation() => UpdateAnimationCore(_animationDuration, _easing, _distanceThreshold);

    /// <summary>
    /// Updates the animation using explicitly supplied settings.
    /// Although still supported, prefer the parameterless <see cref="UpdateAnimation()"/>
    /// and supply animation settings through the constructor instead. When using this overload,
    /// the supplied arguments take precedence over any values passed to the constructor.
    /// </summary>
    public bool UpdateAnimation(int duration, Easing easing, double distanceThreshold)
        => UpdateAnimationCore(duration, easing, distanceThreshold);

    private bool UpdateAnimationCore(int duration, Easing easing, double distanceThreshold)
    {
        var progress = CalculateProgress(_startTime, duration, easing);
        if (progress >= 1) return false;

        // This is a solution to a situation where some vehicle was not updated for a long time 
        // and then at some point was updated again. This caused a huge jump in the animation.
        // In that case it was better to just show the vehicle on the new position.
        // Not sure how important this is, and perhaps there is a better solution, like checking
        // the time between the previous and the current update.
        if (Start.Distance(End) > distanceThreshold) return false;

        Point.X = Start.X + (End.X - Start.X) * progress;
        Point.Y = Start.Y + (End.Y - Start.Y) * progress;
        Modified();
        return true;
    }

    private static double CalculateProgress(long startTime, int animationDuration, Easing easing)
    {
        var currentTime = Environment.TickCount64;
        var elapsedTime = currentTime - startTime;
        return easing.Ease(elapsedTime / (float)animationDuration);
    }

    public override object Clone() => new AnimatedPointFeature(this);
}
