using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations;

internal class ViewportStateAnimation
{
    public static List<AnimationEntry<Viewport>> Create(IViewport viewport, ViewportState viewportState, long duration, Easing? easing)
    {
        var animations = new List<AnimationEntry<Viewport>>();

        var entry = new AnimationEntry<Viewport>(
            start: viewport.State,
            end: viewportState,
            animationStart: 0,
            animationEnd: 1,
            easing: easing ?? Easing.SinInOut,
            tick: Tick,
            final: Final
        );
        animations.Add(entry);

        Animation.Start(animations, duration);

        return animations;
    }

    private static void Tick(Viewport viewport, AnimationEntry<Viewport> entry, double value)
    {
        var start = (ViewportState)entry.Start;
        var end = (ViewportState)entry.End;
        viewport.State = start + (end - start) * entry.Easing.Ease(value);
    }

    private static void Final(Viewport viewport, AnimationEntry<Viewport> entry)
    {
        viewport.State = (ViewportState)entry.End;
    }
}
