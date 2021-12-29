using System;
using System.Collections.Generic;
using Mapsui.Extensions;
using Mapsui.Layers.Tiling;
using Mapsui.Styles;
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

        public static int mode = 3;
        public static List<Action<MapInfo>> actions;
        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
            actions = CreateListOfActions(mapControl);

            var button = CreateButton("Next Mode");
            button.Touched += Button_Touched;
            mapControl.Map.Widgets.Add(button);

            mapControl.Map.Info += (s, a) => {
                if (a.MapInfo?.WorldPosition != null)
                {
                    actions[mode](a.MapInfo);
                }
            };
        }

        private List<Action<MapInfo>> CreateListOfActions(IMapControl mapControl)
        {
            return new List<Action<MapInfo>> {
                (mapInfo) => mapControl.Navigator?.FlyTo(mapInfo.WorldPosition, mapControl.Viewport.Resolution * 8, 500),
                (mapInfo) => mapControl.Navigator?.RotateTo(mapControl.Viewport.Rotation + 56, 500, Easing.CubicIn),
                (mapInfo) => mapControl.Navigator?.CenterOn(mapInfo.WorldPosition, 500, Easing.CubicOut),
                (mapInfo) => mapControl.Navigator?.NavigateTo(mapInfo.WorldPosition, mapControl.Map.Resolutions[5], 500, Easing.CubicOut),
                (mapInfo) => mapControl.Navigator?.ZoomTo(mapControl.Map.Resolutions[8], mapInfo.ScreenPosition!, 500, Easing.CubicOut),
                (mapInfo) => mapControl.Navigator?.ZoomTo(mapControl.Map.Resolutions[8], 500, Easing.CubicOut)
            };
        }

        private void Button_Touched(object sender, HyperlinkWidgetArguments e)
        {
            mode++;
            if (mode >= actions.Count) mode = 0;
            e.Handled = true;
        }

        private static Hyperlink CreateButton(string text)
        {
            // Todo: Replace this with a TextButton which diplays the current mode
            return new Hyperlink()
            {
                Text = text,
                CornerRadius = 2,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                MarginX = 10,
                MarginY = 10,
                PaddingX = 4,
                PaddingY = 4,
                BackColor = new Color(192, 192, 192),
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
