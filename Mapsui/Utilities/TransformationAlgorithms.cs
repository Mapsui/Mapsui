namespace Mapsui.Utilities;

public static class TransformationAlgorithms
{
    /// <summary>
    /// Calculates the new CenterOfMap based on the CenterOfZoom and the new resolution.
    /// The CenterOfZoom is not the same as the CenterOfMap. CenterOfZoom is the one place in
    /// the map that stays on the same location when zooming. In Mapsui is can be equal to the 
    /// CenterOfMap, for instance when using the +/- buttons. When using mouse wheel zoom the
    /// CenterOfZoom is the location of the mouse. 
    /// </summary>
    /// <param name="centerOfZoomX"></param>
    /// <param name="centerOfZoomY"></param>
    /// <param name="newResolution"></param>
    /// <param name="currentCenterX"></param>
    /// <param name="currentCenterY"></param>
    /// <param name="currentResolution"></param>
    /// <returns>The x and y of the center of the map.</returns>
    public static (double x, double y) CalculateCenterOfMap(
        double centerOfZoomX, double centerOfZoomY, double newResolution,
        double currentCenterX, double currentCenterY, double currentResolution)
    {
        var ratio = newResolution / currentResolution;

        return
            (centerOfZoomX - (centerOfZoomX - currentCenterX) * ratio,
            centerOfZoomY - (centerOfZoomY - currentCenterY) * ratio);
    }
}
