﻿using System;
using Mapsui.Extensions;
using Mapsui.Layers.Tiling;
using Mapsui.UI;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;

namespace Mapsui.Samples.Common.Maps
{
    public class ViewportCenterOnAnimationSample : ISample
    {
        public string Name => "Viewport Center Animation";
        public string Category => "Animations";

        public static int mode = 1;
        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();

            mapControl.Map.Info += (s, a) => {
                if (a.MapInfo?.WorldPosition != null)
                {
                    // Animate to the new center.
                    mapControl.Navigator?.CenterOn(a.MapInfo.WorldPosition, 500, Easing.CubicOut);
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
