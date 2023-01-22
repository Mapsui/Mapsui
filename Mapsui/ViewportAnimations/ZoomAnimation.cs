using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations;

public class ZoomAnimation
{
    public static List<AnimationEntry<Viewport>> Create(Viewport viewport, double resolution, long duration, Easing? easing)
    {
        var animations = new List<AnimationEntry<Viewport>>();

        var entry = new AnimationEntry<Viewport>(
            start: viewport.Resolution,
            end: resolution,
            animationStart: 0,
            animationEnd: 1,
            easing: easing ?? Easing.SinInOut,
            tick: ResolutionTick,
            final: ResolutionFinal
        );
        animations.Add(entry);

        Animation.Start(animations, duration);
        return animations;
    }

    private static void ResolutionFinal(Viewport viewport, AnimationEntry<Viewport> entry)
    {
        viewport.Resolution = (double)entry.End;
    }

    private static void ResolutionTick(Viewport viewport, AnimationEntry<Viewport> entry, double value)
    {
        viewport.Resolution = (double)entry.Start + ((double)entry.End - (double)entry.Start) * entry.Easing.Ease(value);
    }
}
