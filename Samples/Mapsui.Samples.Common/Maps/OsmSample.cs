using Mapsui.Projection;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class OsmSample
    {
        public static Map CreateMap()
        {
            var map = new Map
            {
                CRS = "EPSG:3857",
                Transformation = new MinimalTransformation()
            };
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Widgets.Add(new Widgets.ScaleBar.ScaleBarWidget(map) { TextAlignment=Widgets.Alignment.Center, HorizontalAlignment = Widgets.HorizontalAlignment.Center, VerticalAlignment = Widgets.VerticalAlignment.Top });
            map.Widgets.Add(new Widgets.Zoom.ZoomInOutWidget(map) { MarginX = 20, MarginY = 40 });
            return map;
        }
    }
}