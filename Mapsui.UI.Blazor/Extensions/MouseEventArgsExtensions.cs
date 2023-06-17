using Microsoft.AspNetCore.Components.Web;

namespace Mapsui.UI.Blazor.Extensions;
internal static class MouseEventArgsExtensions
{
    public static MPoint ToLocation(this MouseEventArgs e, BoundingClientRect clientRect)
    {
        return new MPoint(e.ClientX - clientRect.Left, e.ClientY - clientRect.Top);
    }
}
