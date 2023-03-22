using System.Collections.Generic;
using Mapsui.Animations;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations;

internal class ViewportStateAnimation
{
    public static List<AnimationEntry<Viewport>> Create(Viewport viewport, ViewportState newViewportState, long duration, Easing? easing)
    {
        var animations = new List<AnimationEntry<Viewport>>();

        var entry = new AnimationEntry<Viewport>(
            start: viewport.State,
            end: newViewportState,
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

    private static AnimationResult<Viewport> Tick(Viewport viewport, AnimationEntry<Viewport> entry, double value)
    {
        var start = (ViewportState)entry.Start;
        var end = (ViewportState)entry.End;
        var result = viewport.SetViewportStateWithLimit(start + (end - start) * entry.Easing.Ease(value));
        return new AnimationResult<Viewport>(viewport, !result.Limited);
    }

    private static AnimationResult<Viewport> Final(Viewport viewport, AnimationEntry<Viewport> entry)
    {
        viewport.SetViewportStateWithLimit((ViewportState)entry.End); //!!! We should  not always call final.
        return new AnimationResult<Viewport>(viewport, true);
    }
}
