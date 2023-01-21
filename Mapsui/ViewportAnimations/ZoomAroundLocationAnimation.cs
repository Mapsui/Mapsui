using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations;

internal class ZoomAroundLocationAnimation : ZoomOnCenterAnimation
{
    public static List<AnimationEntry<Viewport>> Create(IViewport viewport, double centerOfZoomX, double centerOfZoomY, double newResolution,
        double currentCenterOfMapX, double currentCenterOfMapY, double currentResolution, long duration)
    {
        // todo: Remove the inherited overload somehow.
        var (worldCenterOfMapX, worldCenterOfMapY) = TransformationAlgorithms.CalculateCenterOfMap(
            centerOfZoomX, centerOfZoomY, newResolution, currentCenterOfMapX, currentCenterOfMapY, currentResolution);

        return Create(viewport, worldCenterOfMapX, worldCenterOfMapY, newResolution, duration);
    }
}
