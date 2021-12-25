using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations
{
    public class SetCenterAnimation
    {
        public static List<AnimationEntry> Create(IViewport viewport, MReadOnlyPoint center, Easing? easing)
        {
            return new List<AnimationEntry> { new AnimationEntry(
                start: viewport.Center,
                end: center,
                animationStart: 0,
                animationEnd: 1,
                easing: easing ?? Easing.SinOut,
                tick: (e, v) => CenterTick(viewport, e, v),
                final: (e) => CenterFinal(viewport, e)
            )};
        }

        private static void CenterTick(IViewport viewport, AnimationEntry entry, double value)
        {
            var centerX = ((MReadOnlyPoint)entry.Start).X + (((MReadOnlyPoint)entry.End).X - ((MReadOnlyPoint)entry.Start).X) * entry.Easing.Ease(value);
            var centerY = ((MReadOnlyPoint)entry.Start).Y + (((MReadOnlyPoint)entry.End).Y - ((MReadOnlyPoint)entry.Start).Y) * entry.Easing.Ease(value);
            viewport.SetCenter(centerX, centerY);
        }

        private static void CenterFinal(IViewport viewport, AnimationEntry entry)
        {
            var centerX = ((MReadOnlyPoint)entry.End).X;
            var centerY = ((MReadOnlyPoint)entry.End).Y;
            viewport.SetCenter(centerX, centerY);
        }
    }
}
