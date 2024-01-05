using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using System.Threading.Tasks;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps.Animations;

public class ViewportRotateAnimationSample : ISample
{
    public string Name => "Animated Viewport - Rotate";
    public string Category => "Animations";

    public static int mode = 1;

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map { CRS = "EPSG:3857" };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        var rotateButton = CreateButton("Click to rotate clockwise", VerticalAlignment.Top);
        rotateButton.Touched += (s, e) => map.Navigator.RotateTo(map.Navigator.Viewport.Rotation + 45, 500, Easing.CubicIn);
        map.Widgets.Add(rotateButton);

        var rotateBackButton = CreateButton("Click to rotate counterclockwise", VerticalAlignment.Bottom);
        rotateBackButton.Touched += (s, e) => map.Navigator.RotateTo(map.Navigator.Viewport.Rotation - 45, 500, Easing.CubicIn);
        map.Widgets.Add(rotateBackButton);

        return map;
    }

    private static TextButtonWidget CreateButton(string text, VerticalAlignment verticalAlignment) => new TextButtonWidget
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
