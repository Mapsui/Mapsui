using Mapsui.Layers;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps
{
    public class TmsSample // disabled because service is down
                           //  todo: Replace with another.
    {
        public string Name => "TMS";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(CreateLayer());
            return map;
        }

        public static ILayer CreateLayer()
        {
            var url = "https://geodata.nationaalgeoregister.nl/tiles/service/tms/1.0.0/opentopo@EPSG%3A28992 ";
            var tileSource = TmsTileSourceBuilder.Build(url, true);

            return new TileLayer(tileSource)
            {
                Name = "Open Topo (PDOK)"
            };
        }
    }
}