
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Mapsui.UI.Blazor.Extensions
{
    public static class Extensions
    {    
        public static MPoint ToMapsui(this PointF point)
        {
            return new MPoint(point.X, point.Y);
        }
    }
}
