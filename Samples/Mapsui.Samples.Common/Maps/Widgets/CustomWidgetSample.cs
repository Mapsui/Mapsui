using Mapsui.Extensions;
using Mapsui.Manipulations;
using Mapsui.Rendering.Skia.SkiaWidgets;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets;
using SkiaSharp;
using System;
using System.Threading.Tasks;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.Cache;

namespace Mapsui.Samples.Common.Maps.Widgets;

public class CustomWidgetSample : ISample
{
    public string Name => "Custom Widget";

    public string Category => "Widgets";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(new CustomWidget
        {
            Margin = new MRect(20),
            Width = 100,
            Height = 20,
            Color = new Color(218, 165, 32, 127)
        });

        return Task.FromResult(map);
    }
}

public class CustomWidget : BaseWidget
{
    private static readonly Random _random = new();

    public CustomWidget()
    {
        Margin = new(20);
    }

    public Color? Color { get; set; }

    public override bool OnTapped(Navigator navigator, WidgetEventArgs e)
    {
        base.OnTapped(navigator, e);

        if (e.TapType == TapType.Single)
            Color = GenerateRandomColor();
        else
            Color = Mapsui.Styles.Color.Transparent;
        return false;
    }

    public static Color GenerateRandomColor()
    {
        byte[] rgb = new byte[3];
        _random.NextBytes(rgb);
        return new Color(rgb[0], rgb[1], rgb[2]);
    }
}

public class CustomWidgetSkiaRenderer : ISkiaWidgetRenderer
{
    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, RenderService renderService, float layerOpacity)
    {
        // Cast to custom widget to be able to access the specific CustomWidget fields
        var customWidget = (CustomWidget)widget;

        // Update the envelope so the MapControl can do hit detection
        widget.Envelope = ToEnvelope(customWidget);

        // Use the envelope to draw
        using var skPaint = new SKPaint { Color = customWidget.Color.ToSkia(layerOpacity) };
        canvas.DrawRect(widget.Envelope.ToSkia(), skPaint);
    }

    private static MRect ToEnvelope(CustomWidget customWidget)
    {
        // A better implementation would take into account widget alignment
        return new MRect(customWidget.Margin.Left, customWidget.Margin.Top,
            customWidget.Margin.Left + customWidget.Width,
            customWidget.Margin.Top + customWidget.Height);
    }
}
