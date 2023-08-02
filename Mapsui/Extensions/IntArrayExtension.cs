namespace Mapsui.Extensions;
public static class IntArrayExtension
{
    /// <summary> True if Is Natural Axis Order </summary>
    /// <param name="axisOrder"></param>
    /// <returns></returns>
    public static bool IsNaturalOrder(this int[] axisOrder)
    {
#if NETSTANDARD2_0
        if (axisOrder.Length == 2 && axisOrder[0] == 1 && axisOrder[1] == 1)
#else
        if (axisOrder is [0, 1])
#endif
        {
            return true;
        }

        return false;
    }
}
