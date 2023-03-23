using System.Collections.Generic;
using Mapsui.Animations;

namespace Mapsui.ViewportAnimations;

internal class ViewportStateAnimation
{
    public static List<AnimationEntry<ViewportState>> Create(ViewportState viewport, ViewportState destination, long duration, Easing? easing)
    {
        var animations = new List<AnimationEntry<ViewportState>>();

        var entry = new AnimationEntry<ViewportState>(
            start: viewport,
            end: destination,
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

    private static AnimationResult<ViewportState> Tick(ViewportState viewport, AnimationEntry<ViewportState> entry, double value)
    {
        var start = (ViewportState)entry.Start;
        var end = (ViewportState)entry.End;
        var result = start + (end - start) * entry.Easing.Ease(value);
        return new AnimationResult<ViewportState>(result, true);
    }

    private static AnimationResult<ViewportState> Final(ViewportState viewport, AnimationEntry<ViewportState> entry)
    {
        return new AnimationResult<ViewportState>((ViewportState)entry.End, true);
    }
}
