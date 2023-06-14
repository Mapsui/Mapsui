using Microsoft.AspNetCore.Components.Web;

namespace Mapsui.UI.Blazor.Extensions;
public static  class TouchPointExtensions
{
    public static MPoint ToMPoint(this TouchPoint touchPoint)
    {
        return new MPoint(touchPoint.ClientX, touchPoint.ClientY);
    }
}
