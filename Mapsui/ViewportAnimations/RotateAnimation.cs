using System;
using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations;

public class RotateAnimation
{
    public static List<AnimationEntry<Viewport>> Create(Viewport viewport, double rotation, long duration, Easing? easing)
    {
        rotation = GetNearestRotation(viewport, rotation);

        var animations = new List<AnimationEntry<Viewport>> { new AnimationEntry<Viewport>(
            start: viewport.Rotation,
            end: rotation,
            animationStart: 0,
            animationEnd: 1,
            easing: easing ?? Easing.SinInOut,
            tick: (viewport,e, v) => viewport.Rotation = (double)e.Start + (((double)e.End - (double)e.Start) * e.Easing.Ease(v)),
            final: (viewport, e) => viewport.Rotation = (double)e.End
        ) };

        Animation.Start(animations, duration);

        return animations;
    }

    private static double GetNearestRotation(Viewport viewport, double rotation)
    {
        var rotationInTheOtherDirection = rotation + (0 > (rotation - viewport.Rotation) ? 360 : -360);

        // Which rotation is closest to the current rotation?
        return Math.Abs(rotation - viewport.Rotation) > Math.Abs(rotationInTheOtherDirection - viewport.Rotation)
            ? rotationInTheOtherDirection
            : rotation;
    }
}
