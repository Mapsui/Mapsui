using Mapsui.Manipulations;

namespace Mapsui.UI.Android.Extensions;

public static class PointExtensions
{
    public static ScreenPosition ToDeviceIndependentUnits(this ScreenPosition point, double pixelDensity)
    {
        return new ScreenPosition(
            ToDeviceIndependentUnits(point.X, pixelDensity),
            ToDeviceIndependentUnits(point.Y, pixelDensity));
    }

    private static double ToDeviceIndependentUnits(double devicePixels, double pixelDensity)
    {
        return devicePixels / pixelDensity;
    }
}
