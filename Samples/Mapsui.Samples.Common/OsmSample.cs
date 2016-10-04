using BruTile.Predefined;
using Mapsui.Layers;

namespace Mapsui.Samples.Common
{
    public static class OsmSample
    {
        public static ILayer CreateLayer()
        {
            return new TileLayer(KnownTileSources.Create()) {Name = "OSM"};
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(CreateLayer());
            return map;
        }
    }
}