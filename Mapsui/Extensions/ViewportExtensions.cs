namespace Mapsui.Extensions;

public static class ViewportExtensions
{
    /// <summary>
    /// True if Width and Height are not zero
    /// </summary>
    public static bool HasSize(this IReadOnlyViewport viewport) =>
        !viewport.Width.IsNanOrInfOrZero() && !viewport.Height.IsNanOrInfOrZero();

    /// <summary> World To Screen Translation of a Rect </summary>
    /// <param name="viewport">view Port</param>
    /// <param name="rect">rect</param>
    /// <returns>Transformed rect</returns>
    public static MRect WorldToScreen(this IReadOnlyViewport viewport, MRect rect)
    {
        var min = viewport.WorldToScreen(rect.Min);
        var max = viewport.WorldToScreen(rect.Max);
        return new MRect(min.X, min.Y, max.X, max.Y);
    }
}
