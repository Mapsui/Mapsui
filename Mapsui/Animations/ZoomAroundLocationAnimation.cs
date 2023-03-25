using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.Animations;

internal class ZoomAroundLocationAnimation
{
    public static List<AnimationEntry<Viewport>> Create(Viewport viewport, double centerOfZoomX, double centerOfZoomY, double resolution, long duration, Easing easing)
    {
        var (x, y) = TransformationAlgorithms.CalculateCenterOfMap(
            centerOfZoomX, centerOfZoomY, resolution, viewport.CenterX, viewport.CenterY, viewport.Resolution);

        var destinationViewport = viewport with { CenterX = x, CenterY = y, Resolution = resolution };
        return ViewportAnimation.Create(viewport, destinationViewport, duration, easing);
    }
}
