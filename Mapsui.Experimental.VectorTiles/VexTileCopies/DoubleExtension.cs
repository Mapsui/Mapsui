using System;

namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

internal static class DoubleExtension
{
    internal static bool BasicallyEqualTo(this double a, double b) =>
        BasicallyEqualTo(a, b, 0.0001);

    private static bool BasicallyEqualTo(this double a, double b, double precision) =>
        Math.Abs(a - b) <= precision;
}
