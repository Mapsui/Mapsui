using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.UI;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;

namespace Mapsui.Samples.Common.Maps.Animations;

public class ViewportRotateAnimationSample : IMapControlSample
{
    public string Name => "Animated Viewport - Rotate";
    public string Category => "Animations";

    public static int mode = 1;
    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();

        mapControl.Map.Info += (s, a) =>
        {
            if (a.MapInfo?.WorldPosition != null)
            {
                // Animate towards a new rotation, choosing the most adjacent angle.
                mapControl.Navigator?.RotateTo(mapControl.Viewport.Rotation + 45, 500, Easing.CubicIn);
            }
        };
    }

    public static Map CreateMap()
    {
        var map = new Map
        {
            CRS = "EPSG:3857"
        };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(new ScaleBarWidget(map)
        {
            TextAlignment = Alignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top
        });
        map.Widgets.Add(new ZoomInOutWidget { MarginX = 20, MarginY = 40 });
        return map;
    }
}
