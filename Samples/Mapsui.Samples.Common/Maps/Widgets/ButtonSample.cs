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

    private int clickCount;

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        var clickMeButton = CreateButton("Click me", VerticalAlignment.Top, HorizontalAlignment.Left);
        clickMeButton.Touched += (s, a) =>
            {
                ((TextButtonWidget?)s!).Text = $"Clicked {++clickCount} times";
                map.RefreshGraphics();
            };
        map.Widgets.Add(clickMeButton);

        map.Widgets.Add(CreateButtonWithImage(VerticalAlignment.Top, HorizontalAlignment.Right));
        map.Widgets.Add(CreateButton("Button with text", VerticalAlignment.Bottom, HorizontalAlignment.Right));
        map.Widgets.Add(CreateButtonWithImage(VerticalAlignment.Bottom, HorizontalAlignment.Left));

        return Task.FromResult(map);
    }

    private static TextButtonWidget CreateButton(string text,
        VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment)
    {
        return new TextButtonWidget()
        {
            Text = text,
            VerticalAlignment = verticalAlignment,
            HorizontalAlignment = horizontalAlignment,
            Margin = new MRect(30),
            Padding = new MRect(10, 8),
            CornerRadius = 8,
            BackColor = new Color(0, 123, 255),
            TextColor = Color.White,
        };
    }

    private static IconButtonWidget CreateButtonWithImage(
        VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment)
    {
        return new IconButtonWidget()
        {
            SvgImage = LoadSomeSvgAsString(),
            VerticalAlignment = verticalAlignment,
            HorizontalAlignment = horizontalAlignment,
            Margin = new MRect(30),
            Padding = new MRect(10, 8),
            CornerRadius = 8,
            Envelope = new MRect(0, 0, 64, 64)
        };
    }

    static string LoadSomeSvgAsString()
    {
        Assembly assembly = typeof(Map).Assembly ?? throw new ArgumentNullException("assembly");
        using (Stream stream = assembly.GetManifestResourceStream(assembly.GetFullName("Resources.Images.MyLocationStill.svg"))
            ?? throw new Exception("Can not find embedded resource"))
        using (StreamReader reader = new StreamReader(stream))
            return reader.ReadToEnd();
    }
}
