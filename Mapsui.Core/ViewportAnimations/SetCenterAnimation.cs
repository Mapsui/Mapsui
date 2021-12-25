using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations
{
    public class SetCenterAnimation
    {
        public static List<AnimationEntry> Create(IViewport viewport, double centerX, double centerY, Easing? easing)
        {
            return new List<AnimationEntry> { new AnimationEntry(
                start: (viewport.CenterX, viewport.CenterY),
                end: (centerX, centerY),
                animationStart: 0,
                animationEnd: 1,
                easing: easing ?? Easing.SinOut,
                tick: (e, v) => CenterTick(viewport, e, v),
                final: (e) => CenterFinal(viewport, e)
            )};
        }

        private static void CenterTick(IViewport viewport, AnimationEntry entry, double value)
        {
            var (startX, startY) = ((double, double))entry.Start;
            var (endX, endY) = ((double, double))entry.End;
            var centerX = startX + (endX - startX) * entry.Easing.Ease(value);
            var centerY = startY + (endY - startY) * entry.Easing.Ease(value);
            viewport.SetCenter(centerX, centerY);
        }

        private static void CenterFinal(IViewport viewport, AnimationEntry entry)
        {
            var (centerX, centerY) = ((double, double))entry.End;
            viewport.SetCenter(centerX, centerY);
        }
    }
}
