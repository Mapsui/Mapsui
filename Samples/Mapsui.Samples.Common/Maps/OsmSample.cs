using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class OsmSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(CreateLayer());
            return map;
        }

        public static ILayer CreateLayer()
        {
            return new TileLayer(KnownTileSources.Create()) {Name = "OSM"};
        }
    }
}