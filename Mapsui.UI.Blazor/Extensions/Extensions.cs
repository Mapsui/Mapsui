
using System.Drawing;
using Microsoft.AspNetCore.Components.Web;

namespace Mapsui.UI.Blazor.Extensions
{
    public static class Extensions
    {    
        public static MPoint ToMapsui(this PointF point)
        {
            return new MPoint(point.X, point.Y);
        }
        
        public static MPoint ToMapsui(this MPoint point)
        {
            return point;
        }

        public static MPoint Location(this MouseEventArgs e)
        {
            return new MPoint(e.PageX, e.PageY);
        }

        public static MPoint Min(MPoint x, MPoint y)
        {
            return new MPoint();
        }
        
        public static MPoint Max(MPoint x, MPoint y)
        {
            return new MPoint(Math.Max(x.X, y.X), Math.Max(x.Y, y.Y));
        }
    }
}
