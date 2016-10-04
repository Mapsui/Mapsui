using Mapsui.Layers;

namespace Mapsui.Samples.Common
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
                "http://geoserver.nl/tiles/tilecache.aspx/1.0.0/worlddark_GM", true))
            {
                Name = "TMS"
            };
        }
    }
}