using Mapsui.Styles;

namespace Mapsui.Extensions;
public static class PenExtensions
{
    public static bool IsVisible(this Pen? pen)
    {
        if (pen == null)
        {
            return false;
        }

        if (pen.Color.A == 0)
        {
            return false;
        }

        return true;
    }
}
