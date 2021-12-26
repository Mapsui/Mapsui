using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations
{
    public class SetResolutionAnimation
    {
        public static List<AnimationEntry> Create(Viewport viewport, double resolution, long duration, Easing? easing)
        {
            var animations = new List<AnimationEntry>();

            var entry = new AnimationEntry(
                start: viewport.Resolution,
                end: resolution,
                animationStart: 0,
                animationEnd: 1,
                easing: easing ?? Easing.SinInOut,
                tick: (e, v) => ResolutionTick(viewport, e, v),
                final: (e) => ResolutionFinal(viewport, e)
            );
            animations.Add(entry);

            Animation.Start(animations, duration);
            return animations;
        }

        private static void ResolutionFinal(Viewport viewport, AnimationEntry entry)
        {
            viewport.Resolution = (double)entry.End;
        }

        private static void ResolutionTick(Viewport viewport, AnimationEntry entry, double value)
        {
            viewport.Resolution = (double)entry.Start + ((double)entry.End - (double)entry.Start) * entry.Easing.Ease(value);
        }
    }
}
