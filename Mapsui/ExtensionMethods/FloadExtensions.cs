// ReSharper disable once CheckNamespace
namespace System
{
    public static class FloatExtensions
    {
        public static bool IsNanOrZero(this float target)
        {
            if (float.IsNaN(target)) return true;
            if (float.IsInfinity(target)) return true;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return target == 0;
        }
    }
}
