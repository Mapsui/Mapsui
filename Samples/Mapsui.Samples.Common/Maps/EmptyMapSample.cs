using Mapsui.Geometries;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps
{
    public class EmptyMapSample : ISample
    {
        public string Name => "Empty Map";
        public string Category => "Special";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            return new Map
            {
                Home = n => n.NavigateTo(new BoundingBox(-180, -90, 180, 90))
            };
        }
    }
}