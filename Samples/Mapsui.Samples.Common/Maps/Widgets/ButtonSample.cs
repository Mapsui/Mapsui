using Mapsui.Extensions;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidget;
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
        clickMeButton.WidgetTouched += (s, a) =>
            {
                ((ButtonWidget?)s!).Text = $"Clicked {++clickCount} times";
                map.RefreshGraphics();
            };
        map.Widgets.Add(clickMeButton);

        map.Widgets.Add(CreateButtonWithImage(VerticalAlignment.Top, HorizontalAlignment.Right));        
        map.Widgets.Add(CreateButton("Button with text", VerticalAlignment.Bottom, HorizontalAlignment.Right));
        map.Widgets.Add(CreateButtonWithImage(VerticalAlignment.Bottom, HorizontalAlignment.Left));

        return Task.FromResult(map);
    }

    private static ButtonWidget CreateButton(string text, 
        VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment)
    {
        return new ButtonWidget()
        {
            Text = text,
            VerticalAlignment = verticalAlignment,
            HorizontalAlignment = horizontalAlignment,
            MarginX = 30,
            MarginY = 30,
            PaddingX = 10,
            PaddingY = 8,
            CornerRadius = 8,
            BackColor = new Color(0, 123, 255),
            TextColor = Color.White,
        };
    }

    private static ButtonWidget CreateButtonWithImage(
        VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment)
    {
        return new ButtonWidget()
        {
            Text = "hi", // This text is apparently needed to update to position of the button
            SvgImage = LoadSomeSvgAsString(),
            VerticalAlignment = verticalAlignment,
            HorizontalAlignment = horizontalAlignment,
            MarginX = 30,
            MarginY = 30,
            PaddingX = 10,
            PaddingY = 8,
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
