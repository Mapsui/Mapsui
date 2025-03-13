using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using System.Threading.Tasks;
using Mapsui.Styles;
using Mapsui.Widgets.BoxWidgets;

namespace Mapsui.Samples.Common.Maps.Animations;

public class ViewportCenterAndZoomAnimationSample : ISample
{
    public string Name => "Animated Viewport - Zoom On Center";
    public string Category => "Animations";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map { CRS = "EPSG:3857" };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(new ZoomInOutWidget { Margin = new MRect(20, 40) });
        map.Widgets.Add(CreateTextBox("Tap on the map to center on that location and zoom in on it"));
        map.Tapped += (m, e) =>
        {
            // Animate to the new center and new resolution
            m.Navigator.CenterOnAndZoomTo(e.WorldPosition, e.Map.Navigator.Viewport.Resolution * 0.5, 500, Easing.CubicOut);
            return true;
        };
        return map;
    }

    private static TextBoxWidget CreateTextBox(string text) => new()
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
