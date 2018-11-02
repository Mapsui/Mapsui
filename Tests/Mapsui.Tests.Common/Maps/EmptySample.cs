using Mapsui.Geometries;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;

namespace Mapsui.Tests.Common.Maps
{
    public class EmptySample : ISample
    {
        public string Name => "Empty";
        public string Category => "Tests";

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