using Mapsui.Geometries;
using Mapsui.Styles;

namespace Mapsui.Tests.Common.Maps
{
    internal class EmptySample
    {
        public static Map CreateMap()
        {
            var map = new Map
            {
                BackColor = Color.Transparent,
                Viewport = {Center = new Point(0, 0), Width = 200, Height = 200, Resolution = 1}
            };
            return map;
        }
    }
}