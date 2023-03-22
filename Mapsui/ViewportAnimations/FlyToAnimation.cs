using System;
using System.Collections.Generic;
using Mapsui.Animations;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations;

public class FlyToAnimation
{
    public static List<AnimationEntry<ViewportState>> Create(ViewportState viewport, MPoint center, double maxResolution, long duration)
    {
        var animations = new List<AnimationEntry<ViewportState>>();
        AnimationEntry<ViewportState> entry;

        var viewportCenter = new MPoint(viewport.CenterX, viewport.CenterY);

        if (!center.Equals(viewportCenter))
        {
            entry = new AnimationEntry<ViewportState>(
                start: viewportCenter,
                end: center,
                animationStart: 0,
                animationEnd: 1,
                easing: Easing.SinInOut,
                tick: CenterTick,
                final: CenterFinal
            );
            animations.Add(entry);
        }

        entry = new AnimationEntry<ViewportState>(
            start: viewport.Resolution,
            end: Math.Min(maxResolution, viewport.Resolution * 2),
            animationStart: 0,
            animationEnd: 0.5,
            easing: Easing.SinIn,
            tick: ResolutionTick,
            final: ResolutionFinal
        );
        animations.Add(entry);

        entry = new AnimationEntry<ViewportState>(
            start: Math.Min(maxResolution, viewport.Resolution * 2),
            end: viewport.Resolution,
            animationStart: 0.5,
            animationEnd: 1,
            easing: Easing.SinIn,
            tick: ResolutionTick,
            final: ResolutionFinal
        );
        animations.Add(entry);

        Animation.Start(animations, duration);
        return animations;
    }

    private static AnimationResult<ViewportState> CenterTick(ViewportState viewport, AnimationEntry<ViewportState> entry, double value)
    {
        var newX = ((MPoint)entry.Start).X + (((MPoint)entry.End).X - ((MPoint)entry.Start).X) * entry.Easing.Ease(value);
        var newY = ((MPoint)entry.Start).Y + (((MPoint)entry.End).Y - ((MPoint)entry.Start).Y) * entry.Easing.Ease(value);
        var result = viewport with { CenterX = newX, CenterY = newY };
        return new AnimationResult<ViewportState>(result, true);
    }

    private static AnimationResult<ViewportState> CenterFinal(ViewportState viewport, AnimationEntry<ViewportState> entry)
    {
        var result = viewport with { CenterX = ((MPoint)entry.End).X, CenterY = ((MPoint)entry.End).Y };
        return new AnimationResult<ViewportState>(result, true);
    }

    private static AnimationResult<ViewportState> ResolutionTick(ViewportState viewport, AnimationEntry<ViewportState> entry, double value)
    {
        var result = viewport with { Resolution = (double)entry.Start + ((double)entry.End - (double)entry.Start) * entry.Easing.Ease(value) };
        return new AnimationResult<ViewportState>(result, true);
    }

    private static AnimationResult<ViewportState> ResolutionFinal(ViewportState viewport, AnimationEntry<ViewportState> entry)
    {
        var result = viewport with { Resolution = (double)entry.End };
        return new AnimationResult<ViewportState>(result, true);
    }
}
