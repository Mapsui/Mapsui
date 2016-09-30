namespace System
{
    static class Extensions
    {
        public static bool IsNanOrZero(this double target)
        {
            if (double.IsNaN(target)) return true;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return target == 0;
        }
    }
}
