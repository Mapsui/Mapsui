using System;
using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations
{
    public class SetRotationAnimation
    {
        public static List<AnimationEntry> Create(Viewport viewport, double rotation, Easing? easing)
        {
            rotation = GetRotationForShortedDistance(viewport, rotation);

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

        private static double GetRotationForShortedDistance(Viewport viewport, double rotation)
        {
            double alternativeRotation;
            if (0 > (rotation - viewport.Rotation))
                alternativeRotation = rotation + 360;
            else
                alternativeRotation = rotation - 360;

            if (Math.Abs(rotation - viewport.Rotation) > Math.Abs(alternativeRotation - viewport.Rotation))
            {
                rotation = alternativeRotation;
            }

            return rotation;
        }
    }
}
