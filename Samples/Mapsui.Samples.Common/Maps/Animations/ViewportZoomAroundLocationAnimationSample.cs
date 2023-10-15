using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.Zoom;
using System.Threading.Tasks;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps.Animations;

public class ViewportZoomAroundLocationAnimationSample : ISample
{
    public string Name => "Animated Viewport - Zoom Around Location";
    public string Category => "Animations";

    public static int mode = 1;

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map { CRS = "EPSG:3857" };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(new ZoomInOutWidget { MarginX = 20, MarginY = 40 });

        map.Widgets.Add(CreateTextBox("Tap on the map to zoom in the location where you tapped. " +
            "The map will stay centered on the place where you tap."));

        map.Info += (s, a) =>
        {
            if (a.MapInfo?.WorldPosition != null)
            {
                // Zoom in while keeping centerOfZoom at the same position. If you click somewhere to zoom in the mousepointer
                // will still be above the same location in the map. This can be you used for mouse wheel zoom.
                map.Navigator.ZoomTo(a.MapInfo.Resolution * 0.5, a.MapInfo.ScreenPosition!, 500, Easing.CubicOut);
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
