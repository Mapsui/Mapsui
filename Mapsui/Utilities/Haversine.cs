using System;

namespace Mapsui.Utilities;

public static class Haversine
{
    /// <summary>
    /// Returns the distance in kilometers of any two
    /// latitude / longitude points.
    /// </summary>
    public static double Distance(double lon1, double lat1, double lon2, double lat2)
    {
        double R = 6371; // Mean radius of the earth

        var dLat = ToRadian(lat2 - lat1);
        var dLon = ToRadian(lon2 - lon1);

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadian(lat1)) * Math.Cos(ToRadian(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
        var d = R * c;

        return d; // Distance in kilometers
    }

    private static double ToRadian(double val)
    {
        return Math.PI / 180 * val;
    }
}
