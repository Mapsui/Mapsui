using Mapsui.Extensions;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Widgets;

public class ButtonSample : ISample
{
    public string Name => "Button";
    public string Category => "Widgets";

    private int _tapCount;
    private int _doubleTapCount;

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        map.Widgets.Add(CreateButton("Tap me", VerticalAlignment.Top, HorizontalAlignment.Left, (s, a) =>
            {
                s.Text = $"Tapped {++_tapCount} times";
                map.RefreshGraphics();
                return false;
            }));
        map.Widgets.Add(CreateButtonWithImage(VerticalAlignment.Top, HorizontalAlignment.Right));
        map.Widgets.Add(CreateButton("Hello!", VerticalAlignment.Bottom, HorizontalAlignment.Right, (s, a) =>
            {
                s.Text = $"{s.Text}!";
                map.RefreshGraphics();
                return false;
            }));
        map.Widgets.Add(CreateButton("Double Tap me", VerticalAlignment.Bottom, HorizontalAlignment.Left, (s, a) =>
        {
            if (a.TapCount < 2)
                return false;
            s.Text = $"Double Tapped {++_doubleTapCount} times";
            map.RefreshGraphics();
            return false;
        }));

        return Task.FromResult(map);
    }

    private static ButtonWidget CreateButton(string text, VerticalAlignment verticalAlignment,
        HorizontalAlignment horizontalAlignment, Func<ButtonWidget, WidgetEventArgs, bool> tapped) => new()
        {
            Text = text,
            VerticalAlignment = verticalAlignment,
            HorizontalAlignment = horizontalAlignment,
            Margin = new MRect(30),
            Padding = new MRect(10, 8),
            CornerRadius = 8,
            BackColor = new Color(0, 123, 255),
            TextColor = Color.White,
            Tapped = tapped
        };

    private static IconButtonWidget CreateButtonWithImage(
        VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment) => new()
        {
            SvgImage = LoadSomeSvgAsString(),
            VerticalAlignment = verticalAlignment,
            HorizontalAlignment = horizontalAlignment,
            Margin = new MRect(30),
            Padding = new MRect(10, 8),
            CornerRadius = 8,
            Envelope = new MRect(0, 0, 64, 64)
        };

    static string LoadSomeSvgAsString()
    {
        Assembly assembly = typeof(Map).Assembly ?? throw new ArgumentNullException("assembly");
        using Stream stream = assembly.GetManifestResourceStream(assembly.GetFullName("Resources.Images.MyLocationStill.svg"))
            ?? throw new Exception("Can not find embedded resource");
        using StreamReader reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
