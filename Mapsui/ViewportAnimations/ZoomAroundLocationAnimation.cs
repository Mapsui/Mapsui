using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations;

internal class ZoomAroundLocationAnimation
{
    public static List<AnimationEntry<Viewport>> Create(IViewport viewport, double centerOfZoomX, double centerOfZoomY, double resolution,
        double currentCenterOfMapX, double currentCenterOfMapY, double currentResolution, long duration, Easing easing)
    {
        var (x, y) = TransformationAlgorithms.CalculateCenterOfMap(
            centerOfZoomX, centerOfZoomY, resolution, currentCenterOfMapX, currentCenterOfMapY, currentResolution);

        var newState = viewport.State with { CenterX = x, CenterY = y, Resolution = resolution };
        return ViewportStateAnimation.Create(viewport, newState, duration, easing);
    }
}
