using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations
{
    internal class SetCenterAndResolutionAnimation
    {
        public static List<AnimationEntry> Create(Viewport viewport, double resolution, MReadOnlyPoint centerOfZoom, long duration)
        {
            var animations = new List<AnimationEntry>();

            var entry = new AnimationEntry(
                start: (viewport.Center, viewport.Resolution),
                end: (centerOfZoom, resolution),
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
            var start = ((MReadOnlyPoint Center, double Resolution))entry.Start;
            var end = ((MReadOnlyPoint Center, double Resolution))entry.End;

            var x = start.Center.X + (end.Center.X - start.Center.X) * entry.Easing.Ease(value);
            var y = start.Center.Y + (end.Center.Y - start.Center.Y) * entry.Easing.Ease(value);
            var r = start.Resolution + (end.Resolution - start.Resolution) * entry.Easing.Ease(value);

            viewport.CenterX = x;
            viewport.CenterY = y;
            viewport.Resolution = r;
        }

        private static void CenterAndResolutionFinal(Viewport viewport, AnimationEntry entry)
        {
            var end = ((MReadOnlyPoint Center, double Resolution))entry.End;
            viewport.CenterX = end.Center.X;
            viewport.CenterY = end.Center.Y;
            viewport.Resolution = end.Resolution;
        }
    }
}
