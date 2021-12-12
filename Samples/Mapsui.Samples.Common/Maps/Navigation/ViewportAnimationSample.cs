using System;
using Mapsui.Extensions;
using Mapsui.Layers.Tiling;
using Mapsui.UI;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;

namespace Mapsui.Samples.Common.Maps
{
    public class ViewportAnimationSample : ISample
    {
        public string Name => "Viewport animation";
        public string Category => "Navigation";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
            mapControl.Map.Info += (s, a) => {
                if (a.MapInfo?.WorldPosition != null)
                    mapControl.Navigator?.FlyTo(a.MapInfo.WorldPosition, mapControl.Viewport.Resolution * 8, 5000);
            };
        }

        public static Map CreateMap()
        {
            var map = new Map
            {
                CRS = "EPSG:3857"
            };
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Widgets.Add(new ScaleBarWidget(map)
            {
                TextAlignment = Alignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            });
            map.Widgets.Add(new ZoomInOutWidget { MarginX = 20, MarginY = 40 });
            return map;
        }
    }
}
