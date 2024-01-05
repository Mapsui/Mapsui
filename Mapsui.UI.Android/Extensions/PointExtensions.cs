namespace Mapsui.UI.Android.Extensions;

public static class PointExtensions
{
    public static MPoint ToDeviceIndependentUnits(this MPoint point, double pixelDensity)
    {
        return new MPoint(
            ToDeviceIndependentUnits(point.X, pixelDensity),
            ToDeviceIndependentUnits(point.Y, pixelDensity));
    }

    private static double ToDeviceIndependentUnits(double devicePixels, double pixelDensity)
    {
        return devicePixels / pixelDensity;
    }
}
