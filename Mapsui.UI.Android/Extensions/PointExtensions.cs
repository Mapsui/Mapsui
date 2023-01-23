namespace Mapsui.UI.Android.Extensions;

public static class PointExtensions
{
    public static MPoint ToDeviceIndependentUnits(this MPoint point, float pixelDensity)
    {
        return new MPoint(
            ToDeviceIndependentUnits(point.X, pixelDensity),
            ToDeviceIndependentUnits(point.Y, pixelDensity));
    }

    private static double ToDeviceIndependentUnits(double devicePixels, float pixelDensity)
    {
        return devicePixels / pixelDensity;
    }
}
