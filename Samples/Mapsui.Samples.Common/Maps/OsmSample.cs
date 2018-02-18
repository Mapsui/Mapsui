using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class OsmSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Widgets.Add(new Widgets.ScaleBar.ScaleBarWidget(map) { TextAlignment=Widgets.Alignment.Center, HorizontalAlignment = Widgets.HorizontalAlignment.Left, VerticalAlignment = Widgets.VerticalAlignment.Bottom });
            //map.Widgets.Add(new Widgets.Zoom.ZoomInOutWidget(map) { MarginX = 20, MarginY = 40 });
            return map;
        }
    }
}