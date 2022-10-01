
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Mapsui.UI.Blazor.Extensions
{
    public static class Extensions
    {
        public static readonly ReadOnlyDictionary<string, int> Keys = new(new Dictionary<string, int>
        {
            
        };

        public static MPoint ToMapsui(this PointF point)
        {
            return new MPoint(point.X, point.Y);
        }
    }
}
