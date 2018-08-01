using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class KeepWithinExtentsSample
    {
        public static Map CreateMap()
        {
            var map = new Map
            {
                Limits =
                {
                    ZoomMode = ZoomMode.KeepWithinResolutionsAndAlwaysFillViewport,
                    PanMode = PanMode.KeepViewportWithinExtents
                }
            };
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            return map;
        }
    }
}