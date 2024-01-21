using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using System;
using System.Threading.Tasks;

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
            .AddMarker(SphericalMercator.FromLonLat(-73.935242, 40.730610), DemoColor(), 1.0, "New York City");

        return map;
    }

    private static Random _rand = new(1);

    private static Color DemoColor()
    {
        return new Color(_rand.Next(128, 256), _rand.Next(128, 256), _rand.Next(128, 256));
    }
}
