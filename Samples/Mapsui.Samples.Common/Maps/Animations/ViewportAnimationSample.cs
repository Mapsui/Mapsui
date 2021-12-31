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
        public string Name => "FlyTo Viewport Animation";
        public string Category => "Animations";

        public static int mode = 1;
        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();

            mapControl.Map.Info += (s, a) => {
                if (a.MapInfo?.WorldPosition != null)
                {
                    mapControl.Navigator?.FlyTo(a.MapInfo.WorldPosition, a.MapInfo.Resolution * 1.5, 500);
                }
            };
        }

        //        (mapInfo, viewport) => mapControl.Navigator?.RotateTo(viewport.Rotation + 56, 500, Easing.CubicIn),
        //        (mapInfo, viewport) => mapControl.Navigator?.CenterOn(mapInfo.WorldPosition, 500, Easing.CubicOut),
        //        (mapInfo, viewport) => mapControl.Navigator?.NavigateTo(mapInfo.WorldPosition, 4891.9698102512211, 500, Easing.CubicOut),
        //        (mapInfo, viewport) => mapControl.Navigator?.ZoomTo(611.49622628140264, mapInfo.ScreenPosition!, 500, Easing.CubicOut),
        //        (mapInfo, viewport) => mapControl.Navigator?.ZoomTo(611.49622628140264, 500, Easing.CubicOut)

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
