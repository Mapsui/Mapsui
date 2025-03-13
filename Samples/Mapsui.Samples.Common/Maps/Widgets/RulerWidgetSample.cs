using Mapsui.Extensions;
using Mapsui.Rendering.Skia;
using Mapsui.Rendering.Skia.SkiaWidgets;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.BoxWidgets;
using Mapsui.Widgets.InfoWidgets;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Widgets;

public class RulerWidgetSample : ISample
{
    public string Name => "RulerWidget";
    public string Category => "Widgets";

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
    }

    public static Map CreateMap()
    {
        MapRenderer.RegisterWidgetRenderer(typeof(RulerWidget), new RulerWidgetRenderer());

        var map = new Map
        {
            CRS = "EPSG:3857"
        };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(CreateTextBox("Drag on the map to see the ruler widget in action."));
        map.Widgets.Add(new RulerWidget() { IsActive = true }); // Active on startup. You need to set this value from a button in our own application.
        return map;
    }

    private static TextBoxWidget CreateTextBox(string text) => new()
    {
        Text = text,
        TextSize = 16,
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new MRect(10),
        Padding = new MRect(8),
        CornerRadius = 4,
        BackColor = new Color(108, 117, 125, 128),
        TextColor = Color.White,
    };
}
