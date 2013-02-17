using System;
using System.Linq;
using Mapsui.Geometries;
#if !NETFX_CORE
using XamlPoint = System.Windows.Point;
#else
using XamlPoint = Windows.Foundation.Point;
#endif

namespace Mapsui.Rendering.XamlRendering
{
    static class GeometryExtensions
    {
        public static XamlPoint ToXaml(this Point point)
        {
            return new XamlPoint(point.X, point.Y);
        }

    }
}
