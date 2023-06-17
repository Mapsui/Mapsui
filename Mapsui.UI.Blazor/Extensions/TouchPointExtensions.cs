using Microsoft.AspNetCore.Components.Web;

namespace Mapsui.UI.Blazor.Extensions;
public static  class TouchPointExtensions
{
    public static MPoint ToLocation(this TouchPoint touchPoint, BoundingClientRect clientRect)
    {
        return new MPoint(touchPoint.ClientX - clientRect.Left, touchPoint.ClientY - clientRect.Top);
    }

    public static List<MPoint> ToLocations(this IEnumerable<TouchPoint> touchPoints, BoundingClientRect clientRect)
    {
        return touchPoints.Select(p => p.ToLocation(clientRect)).ToList();
    }
}
