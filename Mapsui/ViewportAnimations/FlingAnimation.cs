using System;
using System.Collections.Generic;
using Mapsui.Extensions;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations;

public static class FlingAnimation
{
    public static (List<AnimationEntry<Viewport>> Entries, long Duration) Create(double velocityX, double velocityY, long maxDuration)
    {
        var animations = new List<AnimationEntry<Viewport>>();

        if (maxDuration < 16)
            return (animations, 0);

        velocityX = -velocityX; // reverse as it finger direction is opposite to map movement
        velocityY = -velocityY; // reverse as it finger direction is opposite to map movement

        var magnitudeOfV = Math.Sqrt(velocityX * velocityX + velocityY * velocityY);

        var duration = magnitudeOfV / 10;

        if (magnitudeOfV < 100 || duration < 16)
            return (animations, 0); ;

        if (duration > maxDuration)
            duration = maxDuration;

        var entry = new AnimationEntry<Viewport>(
            start: (velocityX, velocityY),
            end: (0d, 0d),
            animationStart: 0,
            animationEnd: 1,
            easing: Easing.SinIn,
            tick: FlingTick
        );
        animations.Add(entry);

        Animation.Start(animations, (long)duration);

        return (animations, (long)Math.Ceiling(duration));
    }

    private static void FlingTick(Viewport viewport, AnimationEntry<Viewport> entry, double value)
    {
        var timeAmount = 16 / 1000d; // 16 milliseconds 

        var (velocityX, velocityY) = ((double, double))entry.Start;

        var xMovement = velocityX * (1d - entry.Easing.Ease(value)) * timeAmount;
        var yMovement = velocityY * (1d - entry.Easing.Ease(value)) * timeAmount;

        if (xMovement.IsNanOrInfOrZero())
            xMovement = 0;
        if (yMovement.IsNanOrInfOrZero())
            yMovement = 0;

        if (xMovement == 0 && yMovement == 0)
            return;

        var previous = viewport.ScreenToWorld(0, 0);
        var current = viewport.ScreenToWorld(xMovement, yMovement);

        var xDiff = current.X - previous.X;
        var yDiff = current.Y - previous.Y;

        var newX = viewport.State.CenterX + xDiff;
        var newY = viewport.State.CenterY + yDiff;
        viewport.State = viewport.State with { CenterX = newX, CenterY = newY };
    }
}
