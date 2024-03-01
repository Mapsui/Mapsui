using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using Mapsui.Widgets.ScaleBar;
using System.Threading.Tasks;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps.Animations;

public class ViewportZoomToResolutionAnimationSample : ISample
{
    public string Name => "Animated Viewport - Zoom";
    public string Category => "Animations";

    public static int mode = 1;

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map { CRS = "EPSG:3857" };

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(CreateScaleBar(map));
        map.Widgets.Add(new ZoomInOutWidget { Margin = new MRect(20, 40) });

        var rotateButton = CreateButton("Zoom in", VerticalAlignment.Top);
        rotateButton.Tapped = (s, e) =>
        {
            map.Navigator.ZoomTo(map.Navigator.Viewport.Resolution * 0.5, 500, Easing.CubicOut);
            return false;
        };
        map.Widgets.Add(rotateButton);

        var rotateBackButton = CreateButton("Zoom out", VerticalAlignment.Bottom);
        rotateBackButton.Tapped = (s, e) =>
        {
            map.Navigator.ZoomTo(map.Navigator.Viewport.Resolution * 2, 500, Easing.CubicOut);
            return false;
        };
        map.Widgets.Add(rotateBackButton);

        return map;
    }

    private static ScaleBarWidget CreateScaleBar(Map map) => new ScaleBarWidget(map)
    {
        TextAlignment = Alignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Top
    };

    private static ButtonWidget CreateButton(string text, VerticalAlignment verticalAlignment) => new()
    {
        Text = text,
        Margin = new MRect(20),
        Padding = new MRect(10),
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = verticalAlignment,
        BackColor = new Color(0, 123, 255),
        TextColor = Color.White
    };
}
