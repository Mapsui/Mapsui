using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.Zoom;
using System.Threading.Tasks;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps.Animations;

public class ViewportCenterOnAnimationSample : ISample
{
    public string Name => "Animated Viewport - Center";
    public string Category => "Animations";

    public static int mode = 1;

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        string instructions = "Tap on the map to center on that location";

        var map = new Map { CRS = "EPSG:3857" };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(new ZoomInOutWidget { MarginX = 20, MarginY = 40 });

        map.Widgets.Add(CreateTextBox(instructions));

        map.Info += (s, a) =>
        {
            if (a.MapInfo?.WorldPosition != null)
            {
                // Animate to the new center.
                map.Navigator.CenterOn(a.MapInfo.WorldPosition, 500, Easing.CubicOut);
            }
        };
        return map;
    }

    private static IWidget CreateTextBox(string text) => new TextBox()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalAlignment = HorizontalAlignment.Left,
        MarginX = 10,
        MarginY = 10,
        PaddingX = 8,
        PaddingY = 8,
        CornerRadius = 4,
        BackColor = new Color(108, 117, 125, 128),
        TextColor = Color.White,
    };
}
