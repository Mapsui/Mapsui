using Mapsui.Layers.AnimatedLayers;
using System;

namespace Mapsui.Layers.AnimationLayers;

internal class AnimatedPointFeature : PointFeature
{
    public AnimatedPointFeature(PointFeature pointFeature) : base(pointFeature)
    {
        Start = new MPoint(pointFeature.Point);
        End = new MPoint(pointFeature.Point);
        foreach (var field in pointFeature.Fields)
            this[field] = pointFeature[field];
    }

    public void SetAnimationTarget(MPoint target)
    {
        Start = new MPoint(End); // Start where the previous animation ended
        End = new MPoint(target);

        Point.X = Start.X;
        Point.Y = Start.Y;

        StartTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }

    public bool UpdateAnimation(int duration, EasingFunction function, double distanceThreshold)
    {
        var progress = CalculateProgress(StartTime, duration, function);
        if (progress >= 1) return false;

        // This is a solution to a situator where some vehicle was not updated for a long time 
        // and then at some point was updated again. This caused a huge jump in the animation.
        // In that case it was better to just show the vehicle on the new position.
        // Not sure how important this is, and perhaps there is a better solution, like checking
        // the time between the previous and the current update.
        if (Start.Distance(End) > distanceThreshold) return false;

        Point.X = Start.X + (End.X - Start.X) * progress;
        Point.Y = Start.Y + (End.Y - Start.Y) * progress;
        return true;
    }

    private static double CalculateProgress(long startTime, int animationDuration, EasingFunction function)
    {
        var currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        var elapsedTime = currentTime - startTime;

        if (function == EasingFunction.CubicEaseOut)
            return Math.Min(CubicEaseOut(animationDuration, elapsedTime), 1);
        return Math.Min(Linear(animationDuration, elapsedTime), 1);
    }

    private static double Linear(double d, double t)
    {
        return t / d;
    }

    private static double CubicEaseOut(double d, double t)
    {
        return (t = t / d - 1) * t * t + 1;
    }

    public MPoint End { get; set; }
    public MPoint Start { get; set; }
    public long StartTime { get; set; }
}
