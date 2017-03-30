
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class MbTilesSample
    {

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            return map;
        }

    }
}