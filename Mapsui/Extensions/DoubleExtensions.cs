namespace Mapsui.Extensions;

public static class DoubleExtensions
{
    public static bool IsNanOrInfOrZero(this double target)
    {
        if (double.IsNaN(target)) return true;
        if (double.IsInfinity(target)) return true;
        return target == 0;
    }
}
