using System.Collections.Generic;
using Mapsui.Animations;
using Mapsui.Utilities;

namespace Mapsui.ViewportAnimations;

internal class ZoomAroundLocationAnimation
{
    public static List<AnimationEntry<ViewportState>> Create(ViewportState viewport, double centerOfZoomX, double centerOfZoomY, double resolution,
        ViewportState currentState, long duration, Easing easing)
    {
        var (x, y) = TransformationAlgorithms.CalculateCenterOfMap(
            centerOfZoomX, centerOfZoomY, resolution, currentState.CenterX, currentState.CenterY, currentState.Resolution);

        var newState = currentState with { CenterX = x, CenterY = y, Resolution = resolution };
        return ViewportStateAnimation.Create(viewport, newState, duration, easing);
    }
}
