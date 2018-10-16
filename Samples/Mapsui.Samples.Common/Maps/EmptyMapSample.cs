using Mapsui.Geometries;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps
{
    public class EmptyMapSample : IDemoSample
    {
        public string Name => "4.3 Empty Map";

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