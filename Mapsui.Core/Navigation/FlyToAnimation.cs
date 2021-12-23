using System;
using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Utilities;

namespace Mapsui.Navigation
{
    public class FlyToAnimation
    {
        public static List<AnimationEntry> Create(IViewport viewport, MPoint center, double maxResolution, long duration)
        {
            var animations = new List<AnimationEntry>();
            AnimationEntry entry;

            if (!center.Equals(viewport.Center))
            {
                entry = new AnimationEntry(
                    start: viewport.Center,
                    end: (MReadOnlyPoint)center,
                    animationStart: 0,
                    animationEnd: 1,
                    easing: Easing.SinInOut,
                    tick: (e, v) => CenterTick(viewport, e, v),
                    final: (e) => CenterFinal(viewport, e)
                );
                animations.Add(entry);
            }

            entry = new AnimationEntry(
                start: viewport.Resolution,
                end: Math.Min(maxResolution, viewport.Resolution * 2),
                animationStart: 0,
                animationEnd: 0.5,
                easing: Easing.SinIn,
                tick: (e, v) => ResolutionTick(viewport, e, v),
                final: (e) => ResolutionFinal(viewport, e)
            );
            animations.Add(entry);

            entry = new AnimationEntry(
                start: Math.Min(maxResolution, viewport.Resolution * 2),
                end: viewport.Resolution,
                animationStart: 0.5,
                animationEnd: 1,
                easing: Easing.SinIn,
                tick: (e, v) => ResolutionTick(viewport, e, v),
                final: (e) => ResolutionFinal(viewport, e)
            );
            animations.Add(entry);

            Animation.Start(animations, duration);
            return animations;
        }

        private static void CenterTick(IViewport viewport, AnimationEntry entry, double value)
        {
            var x = ((MReadOnlyPoint)entry.Start).X + (((MReadOnlyPoint)entry.End).X - ((MReadOnlyPoint)entry.Start).X) * entry.Easing.Ease(value);
            var y = ((MReadOnlyPoint)entry.Start).Y + (((MReadOnlyPoint)entry.End).Y - ((MReadOnlyPoint)entry.Start).Y) * entry.Easing.Ease(value);

            viewport.SetCenter(x, y);
        }

        private static void CenterFinal(IViewport viewport, AnimationEntry entry)
        {
            viewport.SetCenter((MReadOnlyPoint)entry.End);
        }

        private static void ResolutionTick(IViewport viewport, AnimationEntry entry, double value)
        {
            var r = (double)entry.Start + ((double)entry.End - (double)entry.Start) * entry.Easing.Ease(value);

            viewport.SetResolution(r);
        }

        private static void ResolutionFinal(IViewport viewport, AnimationEntry entry)
        {
            viewport.SetResolution((double)entry.End);
        }
    }
}
