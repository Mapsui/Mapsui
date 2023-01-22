namespace Mapsui.Extensions;

public static class DoubleExtensions
{
    public static bool IsNanOrInfOrZero(this double target)
    {
        if (double.IsNaN(target)) return true;
        if (double.IsInfinity(target)) return true;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return target == 0;
    }
}
