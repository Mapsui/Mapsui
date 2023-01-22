using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations;

internal class ZoomOnCenterAnimation
{
    public static List<AnimationEntry<Viewport>> Create(IViewport viewport, double centerX, double centerY, double resolution, long duration)
    {
        var animations = new List<AnimationEntry<Viewport>>();

        var entry = new AnimationEntry<Viewport>(
            start: (viewport.CenterX, viewport.CenterY, viewport.Resolution),
            end: (centerX, centerY, resolution),
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
        var start = ((double CenterX, double CenterY, double Resolution))entry.Start;
        var end = ((double CenterX, double CenterY, double Resolution))entry.End;

        viewport.CenterX = start.CenterX + (end.CenterX - start.CenterX) * entry.Easing.Ease(value);
        viewport.CenterY = start.CenterY + (end.CenterY - start.CenterY) * entry.Easing.Ease(value);
        viewport.Resolution = start.Resolution + (end.Resolution - start.Resolution) * entry.Easing.Ease(value);
    }

    private static void CenterAndResolutionFinal(Viewport viewport, AnimationEntry<Viewport> entry)
    {
        var end = ((double CenterX, double CenterY, double Resolution))entry.End;
        viewport.CenterX = end.CenterX;
        viewport.CenterY = end.CenterY;
        viewport.Resolution = end.Resolution;
    }
}
