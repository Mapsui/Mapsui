using System;

namespace Mapsui.Projections;

public class Mercator
{
    private const double Radius = 6378137;
    private const double E = 0.0818191908426;
    private const double D2R = Math.PI / 180;
    private const double HalfPi = Math.PI / 2;
    private const double PiDiv4 = Math.PI / 4;

    private const double C1 = 0.00335655146887969;
    private const double C2 = 0.00000657187271079536;
    private const double C3 = 0.00000001764564338702;
    private const double C4 = 0.00000000005328478445;

    public static MPoint FromLonLat(double lon, double lat)
    {
        var lonRadians = D2R * lon;
        var latRadians = D2R * lat;

        var x = Radius * lonRadians;
        //y=a×ln[tan(π/4+φ/2)×((1-e×sinφ)/(1+e×sinφ))^(e/2)]
        var y = Radius * Math.Log(Math.Tan(PiDiv4 + latRadians * 0.5) / Math.Pow(Math.Tan(PiDiv4 + Math.Asin(E * Math.Sin(latRadians)) / 2), E));

        return new MPoint((float)x, (float)y);
    }

    public static MPoint ToLonLat(double x, double y)
    {
        var g = HalfPi - 2 * Math.Atan(1 / Math.Exp(y / Radius));
        var latRadians = g + C1 * Math.Sin(2 * g) + C2 * Math.Sin(4 * g) + C3 * Math.Sin(6 * g) + C4 * Math.Sin(8 * g);

        var lonRadians = x / Radius;

        var lon = lonRadians / D2R;
        var lat = latRadians / D2R;

        return new MPoint((float)lon, (float)lat);
    }
}
