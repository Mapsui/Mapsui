using Mapsui.Manipulations;
using Microsoft.AspNetCore.Components.Web;

namespace Mapsui.UI.Blazor.Extensions;
internal static class MouseEventArgsExtensions
{
    public static ScreenPosition ToScreenPosition(this MouseEventArgs e, BoundingClientRect clientRect)
    {
        return new ScreenPosition(e.ClientX - clientRect.Left, e.ClientY - clientRect.Top);
    }
}
