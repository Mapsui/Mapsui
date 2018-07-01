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
                Home = n => n.NavigateTo(new Point(0, 0), 1)
            };
            return map;
        }
    }
}