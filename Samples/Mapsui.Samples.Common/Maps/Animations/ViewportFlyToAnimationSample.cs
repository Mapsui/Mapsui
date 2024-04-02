using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using Mapsui.Widgets.BoxWidgets;
using System.Drawing;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Animations;

public class ViewportFlyToAnimationSample : ISample
{
    public string Name => "Animated Viewport - Fly To";
    public string Category => "Animations";

    public static int mode = 1;

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map { CRS = "EPSG:3857" };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(new ZoomInOutWidget { Margin = new MRect(20, 40) });
        map.Widgets.Add(CreateTextBox("Tap on the map to fly to that location. The fly-to animation zooms out and then in."));

        map.Info += (s, a) =>
        {
            if (a.MapInfo?.WorldPosition != null)
            {
                // 'FlyTo' is a specific navigation that moves to a new center while moving in and out.
                map.Navigator.FlyTo(a.MapInfo.WorldPosition, a.MapInfo.Resolution * 1.5, 500);
            }
        };

        return map;
    }

    private static IWidget CreateTextBox(string text) => new TextBoxWidget()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalAlignment = HorizontalAlignment.Left,
        Margin = new MRect(10),
        Padding = new MRect(8),
        CornerRadius = 4,
        BackColor = Color.FromArgb(128, 108, 117, 125),
        TextColor = Color.White,
    };
}
