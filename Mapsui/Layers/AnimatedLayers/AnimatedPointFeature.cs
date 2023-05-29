using Mapsui.Animations;
using System;

namespace Mapsui.Layers.AnimationLayers;

internal class AnimatedPointFeature : PointFeature
{
    long startTime;

    public AnimatedPointFeature(double x, double y) : base(x, y)
    {
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

        startTime = Environment.TickCount;
    }

    public bool UpdateAnimation(int duration, Easing easing, double distanceThreshold)
    {
        var progress = CalculateProgress(startTime, duration, easing);
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

    private static double CalculateProgress(long startTime, int animationDuration, Easing easing)
    {
        var currentTime = Environment.TickCount;
        var elapsedTime = currentTime - startTime;
        return easing.Ease(elapsedTime / (float)animationDuration);
    }
}
