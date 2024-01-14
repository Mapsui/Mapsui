using Mapsui.Styles;

namespace Mapsui.Extensions;
public static class BrushExtensions
{
    public static bool IsVisible(this Brush? brush)
    {
        if (brush == null)
        {
            return false;
        }

        if (brush.Color?.A == 0)
        {
            return false;
        }

        return true;
    }
}
