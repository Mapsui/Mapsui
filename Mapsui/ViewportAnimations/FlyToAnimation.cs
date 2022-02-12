using System;
using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations
{
    public class FlyToAnimation
    {
        public static List<AnimationEntry<Viewport>> Create(IViewport viewport, MPoint center, double maxResolution, long duration)
        {
            var animations = new List<AnimationEntry<Viewport>>();
            AnimationEntry<Viewport> entry;

            if (!center.Equals(viewport.Center))
            {
                entry = new AnimationEntry<Viewport>(
                    start: viewport.Center,
                    end: (MReadOnlyPoint)center,
                    animationStart: 0,
                    animationEnd: 1,
                    easing: Easing.SinInOut,
                    tick: CenterTick,
                    final: CenterFinal
                );
                animations.Add(entry);
            }

            entry = new AnimationEntry<Viewport>(
                start: viewport.Resolution,
                end: Math.Min(maxResolution, viewport.Resolution * 2),
                animationStart: 0,
                animationEnd: 0.5,
                easing: Easing.SinIn,
                tick: ResolutionTick,
                final: ResolutionFinal
            );
            animations.Add(entry);

            entry = new AnimationEntry<Viewport>(
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

        private static void CenterTick(Viewport viewport, AnimationEntry<Viewport> entry, double value)
        {
            viewport.CenterX = ((MReadOnlyPoint)entry.Start).X + (((MReadOnlyPoint)entry.End).X - ((MReadOnlyPoint)entry.Start).X) * entry.Easing.Ease(value);
            viewport.CenterY = ((MReadOnlyPoint)entry.Start).Y + (((MReadOnlyPoint)entry.End).Y - ((MReadOnlyPoint)entry.Start).Y) * entry.Easing.Ease(value);
        }

        private static void CenterFinal(Viewport viewport, AnimationEntry<Viewport> entry)
        {
            viewport.CenterX = ((MReadOnlyPoint)entry.End).X;
            viewport.CenterY = ((MReadOnlyPoint)entry.End).Y;
        }

        private static void ResolutionTick(Viewport viewport, AnimationEntry<Viewport> entry, double value)
        {
            viewport.Resolution = (double)entry.Start + ((double)entry.End - (double)entry.Start) * entry.Easing.Ease(value);
        }

        private static void ResolutionFinal(Viewport viewport, AnimationEntry<Viewport> entry)
        {
            viewport.Resolution = (double)entry.End;
        }
    }
}
