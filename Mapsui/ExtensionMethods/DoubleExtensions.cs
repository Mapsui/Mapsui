 // ReSharper disable once CheckNamespace
namespace System
{
    public static class DoubleExtensions
    {
        public static bool IsNanOrZero(this double target)
        {
            if (double.IsNaN(target)) return true;
            if (double.IsInfinity(target)) return true;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return target == 0;
        }
    }
}
