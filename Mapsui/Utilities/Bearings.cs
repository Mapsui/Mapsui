using System;

namespace Mapsui.Utilities;

public class Bearings
{
    public static double CalculateBearing(double fromLat, double fromLon, double toLat, double toLon)
    {
        // Convert latitude and longitude from degrees to radians
        var radLan1 = ToRadians(fromLat);
        var radLan2 = ToRadians(toLat);
        var deltaLon = ToRadians(toLon - fromLon);

        // Calculate bearing
        var y = Math.Sin(deltaLon) * Math.Cos(radLan2);
        var x = Math.Cos(radLan1) * Math.Sin(radLan2) - Math.Sin(radLan1) * Math.Cos(radLan2) * Math.Cos(deltaLon);
        var theta = Math.Atan2(y, x);

        // Convert bearing from radians to degrees
        var bearing = ToDegrees(theta);

        // Normalize the bearing to be within the range [0, 360)
        return (bearing + 360) % 360;
    }

    private static double ToRadians(double degrees) => degrees * (Math.PI / 180.0);

    private static double ToDegrees(double radians) => radians * (180.0 / Math.PI);
}
