using System;
using System.Collections.Generic;

#if __MAUI__
namespace Mapsui.UI.Maui.Utils;
#else
namespace Mapsui.UI.Forms.Utils;
#endif

/// <summary>
/// Polyline helper class
/// </summary>
public static class PolylineConverter
{
    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="encodedPolyline"></param>
    /// <returns></returns>
    public static List<Position>? DecodePolyline(string encodedPolyline)
    {
        if (string.IsNullOrWhiteSpace(encodedPolyline))
            return null;

        var index = 0;
        var polylineChars = encodedPolyline.ToCharArray();
        var poly = new List<Position>();
        var currentLat = 0;
        var currentLng = 0;
        int next5Bits;

        while (index < polylineChars.Length)
        {
            var sum = 0;
            var shifter = 0;

            do
            {
                next5Bits = polylineChars[index++] - 63;
                sum |= (next5Bits & 31) << shifter;
                shifter += 5;
            }
            while (next5Bits >= 32 && index < polylineChars.Length);

            if (index >= polylineChars.Length)
                break;

            currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);
            sum = 0;
            shifter = 0;

            do
            {
                next5Bits = polylineChars[index++] - 63;
                sum |= (next5Bits & 31) << shifter;
                shifter += 5;
            }
            while (next5Bits >= 32 && index < polylineChars.Length);

            if (index >= polylineChars.Length && next5Bits >= 32)
            {
                break;
            }

            currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);
            var mLatLng = new Position(Convert.ToDouble(currentLat) / 100000.0, Convert.ToDouble(currentLng) / 100000.0);
            poly.Add(mLatLng);
        }

        return poly;
    }
}
