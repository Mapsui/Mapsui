using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.ButtonWidgets;
using System.Threading.Tasks;
using Mapsui.Projections;

namespace Mapsui.Samples.Common.Maps.Demo;

public class MarkerSample : ISample
{
    public string Name => "Marker";
    public string Category => "Extensions";

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

        map.AddMarkerLayer("Marker")
            .AddMarker(SphericalMercator.FromLonLat(-73.935242, 40.730610), DemoColor(), 1.0, "New York", "City");

        return map;
    }
}
