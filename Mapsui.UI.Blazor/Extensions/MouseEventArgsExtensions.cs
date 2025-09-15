using Mapsui.Manipulations;
using Microsoft.AspNetCore.Components.Web;

namespace Mapsui.UI.Blazor.Extensions;
internal static class MouseEventArgsExtensions
{
    public static ScreenPosition ToScreenPosition(this MouseEventArgs e)
    {
        return new ScreenPosition(e.OffsetX, e.OffsetY);
    }
}
