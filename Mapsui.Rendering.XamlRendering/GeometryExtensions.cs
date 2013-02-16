using System;
using System.Linq;
using Mapsui.Geometries;
#if !NETFX_CORE
using WinPoint = System.Windows.Point;
#else
using WinPoint = Windows.Foundation.Point;
#endif

namespace Mapsui.Rendering.XamlRendering
{
    static class GeometryExtensions
    {
        public static WinPoint ToWinPoint(this Point point)
        {
            return new WinPoint(point.X, point.Y);
        }

    }
}
