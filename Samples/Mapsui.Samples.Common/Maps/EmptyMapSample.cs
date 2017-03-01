using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class EmptyMapSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            ZoomHelper.ZoomToBoudingbox(map.Viewport, -180, -90, 180, 90, 1000, 800);
            return map;
        }
    }
}