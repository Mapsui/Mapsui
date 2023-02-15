using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations;

internal class ZoomAroundLocationAnimation
{
    public static List<AnimationEntry<Viewport>> Create(IViewport viewport, double centerOfZoomX, double centerOfZoomY, double newResolution,
        double currentCenterOfMapX, double currentCenterOfMapY, double currentResolution, long duration)
    {
        var (worldCenterOfMapX, worldCenterOfMapY) = TransformationAlgorithms.CalculateCenterOfMap(
            centerOfZoomX, centerOfZoomY, newResolution, currentCenterOfMapX, currentCenterOfMapY, currentResolution);

        return ZoomOnCenterAnimation.Create(viewport, worldCenterOfMapX, worldCenterOfMapY, newResolution, duration);
    }
}
