
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    }
}
