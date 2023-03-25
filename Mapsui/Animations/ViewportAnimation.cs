using System.Collections.Generic;

namespace Mapsui.Animations;

internal class ViewportAnimation
{
    public static List<AnimationEntry<Viewport>> Create(Viewport viewport, Viewport destination, long duration, Easing? easing)
    {
        var animations = new List<AnimationEntry<Viewport>>();

        var entry = new AnimationEntry<Viewport>(
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

    private static AnimationResult<Viewport> Tick(Viewport viewport, AnimationEntry<Viewport> entry, double value)
    {
        var start = (Viewport)entry.Start;
        var end = (Viewport)entry.End;
        var result = start + (end - start) * entry.Easing.Ease(value);
        return new AnimationResult<Viewport>(result, true);
    }

    private static AnimationResult<Viewport> Final(Viewport viewport, AnimationEntry<Viewport> entry)
    {
        return new AnimationResult<Viewport>((Viewport)entry.End, true);
    }
}
