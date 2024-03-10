using Mapsui.Manipulations;
using Mapsui.Styles;
using Mapsui.Widgets;
using System;

namespace Mapsui.Samples.CustomWidget;

public class CustomWidget : Widget
{
    private static readonly Random _random = new();

    public CustomWidget()
    {
        Margin = new(20);
    }

    public Color? Color { get; set; }

    public override bool OnTapped(Navigator navigator, ScreenPosition position, WidgetEventArgs e)
    {
        base.OnTapped(navigator, position, e);

        if (e.TapType == TapType.Single)
            Color = GenerateRandomColor();
        else
            Color = Color.Transparent;
        return false;
    }

    public static Color GenerateRandomColor()
    {
        byte[] rgb = new byte[3];
        _random.NextBytes(rgb);
        return new Color(rgb[0], rgb[1], rgb[2]);
    }
}
