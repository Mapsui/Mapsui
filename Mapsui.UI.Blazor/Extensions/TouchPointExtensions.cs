using Microsoft.AspNetCore.Components.Web;

namespace Mapsui.UI.Blazor.Extensions;
public static  class TouchPointExtensions
{
    public static MPoint ToMPoint(this TouchPoint touchPoint, BoundingClientRect boundingClientRect)
    {
        return new MPoint(touchPoint.ClientX - boundingClientRect.Left, touchPoint.ClientY - boundingClientRect.Top);
    }

    public static List<MPoint> ToMPoints(this IEnumerable<TouchPoint> touchPoints, BoundingClientRect boundingClientRect)
    {
        return touchPoints.Select(p => p.ToMPoint(boundingClientRect)).ToList();
    }
}
