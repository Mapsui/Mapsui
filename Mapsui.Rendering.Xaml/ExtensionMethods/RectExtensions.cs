using System.Windows;
using Mapsui.Geometries;

namespace Mapsui.Rendering.Xaml
{
    public static class RectExtensions
    {
        public static BoundingBox ToMapsui(this Rect rect)
        {
            return new BoundingBox(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
    }
}
