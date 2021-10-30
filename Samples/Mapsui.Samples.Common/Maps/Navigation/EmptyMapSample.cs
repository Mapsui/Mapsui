using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps.Navigation
{
    public class EmptyMapSample : ISample
    {
        public string Name => "Empty Map";
        public string Category => "Navigation";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            return new Map
            {
                Home = n => n.NavigateTo(new MRect(-180, -90, 180, 90))
            };
        }
    }
}