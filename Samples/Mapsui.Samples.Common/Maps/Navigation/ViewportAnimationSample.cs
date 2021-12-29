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

        public static int mode = 1;
        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
            var actions = CreateListOfActions(mapControl);
            var button = CreateButton("Next Mode");
            button.Touched += (s, e) => Button_Touched(s, e, actions);
            mapControl.Map.Widgets.Add(button);

            mapControl.Map.Info += (s, a) => {
                if (a.MapInfo?.WorldPosition != null)
                {
                    actions[mode](a.MapInfo, mapControl.Viewport);
                }
            };
        }

        private static List<Action<MapInfo, IReadOnlyViewport>> CreateListOfActions(IMapControl mapControl)
        {
            return new List<Action<MapInfo, IReadOnlyViewport>> {
                (mapInfo, viewport) => mapControl.Navigator?.FlyTo(mapInfo.WorldPosition, viewport.Resolution * 8, 500),
                (mapInfo, viewport) => mapControl.Navigator?.RotateTo(viewport.Rotation + 56, 500, Easing.CubicIn),
                (mapInfo, viewport) => mapControl.Navigator?.CenterOn(mapInfo.WorldPosition, 500, Easing.CubicOut),
                (mapInfo, viewport) => mapControl.Navigator?.NavigateTo(mapInfo.WorldPosition, 4891.9698102512211, 500, Easing.CubicOut),
                (mapInfo, viewport) => mapControl.Navigator?.ZoomTo(611.49622628140264, mapInfo.ScreenPosition!, 500, Easing.CubicOut),
                (mapInfo, viewport) => mapControl.Navigator?.ZoomTo(611.49622628140264, 500, Easing.CubicOut)
            };
        }

        private void Button_Touched(object _, HyperlinkWidgetArguments e, List<Action<MapInfo, IReadOnlyViewport>> actions)
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
