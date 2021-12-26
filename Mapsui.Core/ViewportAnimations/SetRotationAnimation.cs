using System;
using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations
{
    public class SetRotationAnimation
    {
        public static List<AnimationEntry> Create(Viewport viewport, double rotation, Easing? easing)
        {
            rotation = GetNearestRotation(viewport, rotation);

            return new List<AnimationEntry> { new AnimationEntry(
                start: viewport.Rotation,
                end: rotation,
                animationStart: 0,
                animationEnd: 1,
                easing: easing ?? Easing.SinInOut,
                tick: (e, v) => viewport.Rotation = (double)e.Start + (((double)e.End - (double)e.Start) * e.Easing.Ease(v)),
                final: (e) => viewport.Rotation = (double)e.End
            ) };
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
}
