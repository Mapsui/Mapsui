using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class OsmSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Widgets.Add(new Widgets.Zoom.ZoomInOutWidget(map.Viewport) { MarginX = 20, MarginY = 40 });
            return map;
        }
    }
}