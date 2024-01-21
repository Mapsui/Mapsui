using System.Diagnostics.CodeAnalysis;
using Mapsui.Styles;

namespace Mapsui.Extensions;
public static class PenExtensions
{
    public static bool IsVisible([NotNullWhen(true)] this Pen? pen)
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
