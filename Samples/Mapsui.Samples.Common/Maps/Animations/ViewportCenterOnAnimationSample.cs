﻿using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using System.Threading.Tasks;
using Mapsui.Styles;
using Mapsui.Widgets.BoxWidgets;

namespace Mapsui.Samples.Common.Maps.Animations;

public class ViewportCenterOnAnimationSample : ISample
{
    public string Name => "Animated Viewport - Center";
    public string Category => "Animations";

    public static int mode = 1;

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map { CRS = "EPSG:3857" };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(new ZoomInOutWidget { Margin = new MRect(20, 40) });
        map.Widgets.Add(CreateTextBox("Tap on the map to center on that location"));
        map.Tapped += (m, e) =>
        {
            // Animate to the new center.
            m.Navigator.CenterOn(e.WorldPosition, 500, Easing.CubicOut);
            return true;
        };
        return map;
    }

    private static IWidget CreateTextBox(string text) => new TextBoxWidget()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalAlignment = HorizontalAlignment.Left,
        Margin = new MRect(10),
        Padding = new MRect(8),
        CornerRadius = 4,
        BackColor = new Color(108, 117, 125, 128),
        TextColor = Color.White,
    };
}
