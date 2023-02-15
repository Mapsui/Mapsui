using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations;

internal class ZoomOnCenterAnimation
{
    public static List<AnimationEntry<Viewport>> Create(IViewport viewport, double centerX, double centerY, double resolution, long duration)
    {
        var animations = new List<AnimationEntry<Viewport>>();

        var entry = new AnimationEntry<Viewport>(
            start: viewport.State,
            end: viewport.State with { CenterX = centerX, CenterY = centerY, Resolution = resolution },
            animationStart: 0,
            animationEnd: 1,
            easing: Easing.SinInOut,
            tick: CenterAndResolutionTick,
            final: CenterAndResolutionFinal
        );
        animations.Add(entry);

        Animation.Start(animations, duration);

        return animations;
    }

    private static void CenterAndResolutionTick(Viewport viewport, AnimationEntry<Viewport> entry, double value)
    {
        var start = (ViewportState)entry.Start;
        var end = (ViewportState)entry.End;
        viewport.State = start + (end - start) * entry.Easing.Ease(value);
    }

    private static void CenterAndResolutionFinal(Viewport viewport, AnimationEntry<Viewport> entry)
    {
        viewport.State = (ViewportState)entry.End;
    }
}
