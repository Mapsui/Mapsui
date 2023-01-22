using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations;

public class CenterAnimation
{
    public static List<AnimationEntry<Viewport>> Create(IViewport viewport, double centerX, double centerY, long duration, Easing? easing)
    {
        var animations = new List<AnimationEntry<Viewport>> { new AnimationEntry<Viewport>(
            start: (viewport.CenterX, viewport.CenterY),
            end: (centerX, centerY),
            animationStart: 0,
            animationEnd: 1,
            easing: easing ?? Easing.SinOut,
            tick: CenterTick,
            final: CenterFinal
        )};

        Animation.Start(animations, duration);

        return animations;
    }

    private static void CenterTick(Viewport viewport, AnimationEntry<Viewport> entry, double value)
    {
        var (startX, startY) = ((double, double))entry.Start;
        var (endX, endY) = ((double, double))entry.End;
        viewport.CenterX = startX + (endX - startX) * entry.Easing.Ease(value);
        viewport.CenterY = startY + (endY - startY) * entry.Easing.Ease(value);
    }

    private static void CenterFinal(Viewport viewport, AnimationEntry<Viewport> entry)
    {
        var (centerX, centerY) = ((double, double))entry.End;
        viewport.CenterX = centerX;
        viewport.CenterY = centerY;
    }
}
