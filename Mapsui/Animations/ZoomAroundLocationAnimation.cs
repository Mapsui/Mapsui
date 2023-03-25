using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.Animations;

internal class ZoomAroundLocationAnimation
{
    public static List<AnimationEntry<Viewport>> Create(Viewport viewport, double centerOfZoomX, double centerOfZoomY, double resolution,
        Viewport currentState, long duration, Easing easing)
    {
        var (x, y) = TransformationAlgorithms.CalculateCenterOfMap(
            centerOfZoomX, centerOfZoomY, resolution, currentState.CenterX, currentState.CenterY, currentState.Resolution);

        var newState = currentState with { CenterX = x, CenterY = y, Resolution = resolution };
        return ViewportStateAnimation.Create(viewport, newState, duration, easing);
    }
}
