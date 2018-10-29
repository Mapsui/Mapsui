using Mapsui.Geometries;
using Mapsui.Styles;
using Mapsui.UI;

namespace Mapsui.Tests.Common.Maps
{
    public class EmptySample : ITestSample
    {
        public string Name => "Empty";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

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