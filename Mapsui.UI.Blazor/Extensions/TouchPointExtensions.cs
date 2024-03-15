using Mapsui.Manipulations;
using Microsoft.AspNetCore.Components.Web;

namespace Mapsui.UI.Blazor.Extensions;
public static class TouchPointExtensions
{
    public static ScreenPosition ToScreenPosition(this TouchPoint touchPoint, BoundingClientRect clientRect)
    {
        return new ScreenPosition(touchPoint.ClientX - clientRect.Left, touchPoint.ClientY - clientRect.Top);
    }

    public static ReadOnlySpan<ScreenPosition> ToScreenPositions(this IEnumerable<TouchPoint> touchPoints, BoundingClientRect clientRect)
    {
        return touchPoints.Select(p => p.ToScreenPosition(clientRect)).ToArray();
    }
}
