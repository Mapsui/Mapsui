using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations
{
    internal class SetCenterAndResolutionAnimation
    {
        public static List<AnimationEntry> Create(Viewport viewport, double centerX, double centerY, double resolution, long duration)
        {
            var animations = new List<AnimationEntry>();

            var entry = new AnimationEntry(
                start: (viewport.CenterX, viewport.CenterY, viewport.Resolution),
                end: (centerX, centerY, resolution),
                animationStart: 0,
                animationEnd: 1,
                easing: Easing.SinInOut,
                tick: (e, v) => CenterAndResolutionTick(viewport, e, v),
                final: (e) => CenterAndResolutionFinal(viewport, e)
            );
            animations.Add(entry);

            Animation.Start(animations, duration);

            return animations;
        }

        private static void CenterAndResolutionTick(Viewport viewport, AnimationEntry entry, double value)
        {
            var start = ((double CenterX, double CenterY, double Resolution))entry.Start;
            var end = ((double CenterX, double CenterY,double Resolution))entry.End;

            var x = start.CenterX + (end.CenterX - start.CenterX) * entry.Easing.Ease(value);
            var y = start.CenterY + (end.CenterY - start.CenterY) * entry.Easing.Ease(value);
            var r = start.Resolution + (end.Resolution - start.Resolution) * entry.Easing.Ease(value);

            viewport.CenterX = x;
            viewport.CenterY = y;
            viewport.Resolution = r;
        }

        private static void CenterAndResolutionFinal(Viewport viewport, AnimationEntry entry)
        {
            var end = ((double CenterX, double CenterY, double Resolution))entry.End;
            viewport.CenterX = end.CenterX;
            viewport.CenterY = end.CenterY;
            viewport.Resolution = end.Resolution;
        }
    }
}
