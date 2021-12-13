using Mapsui.Layers;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps
{
    public class TmsSample : ISample
    {
        public string Name => "8 TMS openbasiskaart";
        public string Category => "Data";

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
            var url = "https://www.openbasiskaart.nl/mapcache/tms/1.0.0/osm@rd";
            var tileSource = TmsTileSourceBuilder.Build(url, true);

            var tileLayer = new TileLayer(tileSource)
            {
                Name = "openbasiskaart.nl"
            };

            tileLayer.Attribution.Text = "© OpenStreetMap contributors (via openbasiskaart.nl)";
            tileLayer.Attribution.Url = "https://www.openstreetmap.org/copyright";
            return tileLayer;
        }
    }
}