using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.ButtonWidgets;
using System.Threading.Tasks;
using Mapsui.Widgets.InfoWidgets;

namespace Mapsui.Samples.Common.Maps.Demo;

public class OsmSample : ISample
{
    public string Name => "OpenStreetMap";
    public string Category => "Demo";

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
    }

    public static Map CreateMap()
    {
        var map = new Map
        {
            CRS = "EPSG:3857"
        };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(new ScaleBarWidget(map) { TextAlignment = Alignment.Center, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top });
        map.Widgets.Add(new ZoomInOutWidget { Margin = new MRect(20, 40) });
        map.Widgets.Add(new MouseCoordinatesWidget());
        return map;
    }
}
