using Mapsui.Extensions;
using Mapsui.Rendering.Skia;
using Mapsui.Rendering.Skia.SkiaWidgets;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.BoxWidgets;
using Mapsui.Widgets.ButtonWidgets;
using Mapsui.Widgets.InfoWidgets;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Widgets;

public class RulerWidgetSample : ISample
{
    public string Name => "RulerWidget";
    public string Category => "Widgets";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        MapRenderer.RegisterWidgetRenderer(typeof(RulerWidget), new RulerWidgetRenderer());

        var map = new Map
        {
            CRS = "EPSG:3857"
        };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(new RulerWidget() { IsActive = true }); // Active on startup. You need to set this value from a button in our own application.
        map.Widgets.Add(CreateInstructionTextBox("Drag on the map to see the ruler widget in action."));
        map.Widgets.Add(CreateToggleButton());
        return map;
    }

    private static ButtonWidget CreateToggleButton() => new()
    {
        Text = "Toggle RulerWidget",
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalAlignment = HorizontalAlignment.Left,
        CornerRadius = 3,
        BackColor = new Color(0, 123, 255),
        TextColor = Color.White,
        Margin = new MRect(10),
        Padding = new MRect(8),
        TextSize = 16,
        WithTappedEvent = (s, e) =>
        {
            var rulerWidget = e.Map.Widgets.OfType<RulerWidget>().Single();
            rulerWidget.IsActive = !rulerWidget.IsActive;
        }
    };

    private static TextBoxWidget CreateInstructionTextBox(string text) => new()
    {
        Text = text,
        TextSize = 16,
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new MRect(10),
        Padding = new MRect(8),
        CornerRadius = 3,
        BackColor = new Color(108, 117, 125, 128),
        TextColor = Color.White,
    };
}
