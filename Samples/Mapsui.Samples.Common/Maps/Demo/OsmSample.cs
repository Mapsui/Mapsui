using Mapsui.Extensions;
using Mapsui.Layers.Tiling;
using Mapsui.UI;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;

namespace Mapsui.Samples.Common.Maps.Demo
{
    public class OsmSample : ISample
    {
        public string Name => "1 OpenStreetMap";
        public string Category => "Demo";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map
            {
                CRS = "EPSG:3857"
            };
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Widgets.Add(new ScaleBarWidget(map) { TextAlignment = Alignment.Center, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top });
            map.Widgets.Add(new ZoomInOutWidget { MarginX = 20, MarginY = 40 });
            return map;
        }
    }
}