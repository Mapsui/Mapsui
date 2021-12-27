using System;
using Mapsui.Extensions;
using Mapsui.Layers.Tiling;
using Mapsui.UI;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;

namespace Mapsui.Samples.Common.Maps
{
    public class ViewportAnimationSample : ISample
    {
        public string Name => "0. Viewport animation";
        public string Category => "Demo";

        public static int animationMode = 3;

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
            mapControl.Map.Info += (s, a) => {
                if (a.MapInfo?.WorldPosition != null)
                {
                    if (animationMode == 0)
                        mapControl.Navigator?.FlyTo(a.MapInfo.WorldPosition, mapControl.Viewport.Resolution * 8, 500);
                    else if (animationMode == 1)
                        mapControl.Navigator?.RotateTo(mapControl.Viewport.Rotation + 56, 500, Easing.CubicIn);
                    else if (animationMode == 2)
                        mapControl.Navigator?.CenterOn(a.MapInfo.WorldPosition, 500, Easing.CubicOut);
                    else if (animationMode == 3)
                        mapControl.Navigator?.NavigateTo(a.MapInfo.WorldPosition, mapControl.Map.Resolutions[5], 500, Easing.CubicOut);
                    else if (animationMode == 4)
                        mapControl.Navigator?.ZoomTo(mapControl.Map.Resolutions[8], a.MapInfo.ScreenPosition!, 500, Easing.CubicOut);
                    else if (animationMode == 5)
                        mapControl.Navigator?.ZoomTo(mapControl.Map.Resolutions[8], 500, Easing.CubicOut);

                    // todo: Somehow select between modes
                    //animationMode++;
                    //if (animationMode > 2) animationMode = 0;

                }
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
