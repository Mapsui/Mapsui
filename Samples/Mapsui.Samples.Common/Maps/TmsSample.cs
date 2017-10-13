using Mapsui.Layers;

namespace Mapsui.Samples.Common.Maps
{
    public static class TmsSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(CreateLayer());
            return map;
        }

        public static ILayer CreateLayer()
        {
            return new TileLayer(() => TmsTileSourceBuilder.Build(
                "https://geodata.nationaalgeoregister.nl/tiles/service/tms/1.0.0/opentopo@EPSG%3A28992 ", true))
            {
                Name = "Open Topo (PDOK)"
            };
        }
    }
}