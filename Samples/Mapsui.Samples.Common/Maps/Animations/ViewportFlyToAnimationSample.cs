using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.UI;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;

namespace Mapsui.Samples.Common.Maps.Animations;

public class ViewportFlyToAnimationSample : IMapControlSample
{
    public string Name => "Animated Viewport - Fly To";
    public string Category => "Animations";

    public static int mode = 1;
    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();

        mapControl.Map.Info += (s, a) =>
        {
            if (a.MapInfo?.WorldPosition != null)
            {
                // 'FlyTo' is a specific navigation that moves to a new center while moving in and out.
                mapControl.Navigator?.FlyTo(a.MapInfo.WorldPosition, a.MapInfo.Resolution * 1.5, 500);
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
