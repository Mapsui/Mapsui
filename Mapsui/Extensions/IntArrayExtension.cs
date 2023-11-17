namespace Mapsui.Extensions;
public static class IntArrayExtension
{
    /// <summary> True if Is Natural Axis Order </summary>
    /// <param name="axisOrder"></param>
    /// <returns></returns>
    public static bool IsNaturalOrder(this int[] axisOrder)
    {
        if (axisOrder is [0, 1])
        {
            return true;
        }

        return false;
    }
}
