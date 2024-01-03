using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Demo;

public class PinSample : ISample
{
    public string Name => "6 Pin Sample";
    public string Category => "Demo";

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
    }

    public static Map CreateMap()
    {
        const string markerLayerName = "Markers";

        var map = new Map
        {
            CRS = "EPSG:3857"
        };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(new ScaleBarWidget(map) { TextAlignment = Alignment.Center, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top });

        using var layer = map.AddMarkerLayer(markerLayerName);

        if (layer == null)
            return map;

        // Read demo SVG
        var tiger = GetSvgFromResources("Images.Ghostscript_Tiger.svg");

        // Read demo Icon
        var icon = GetIconFromResources("Images.icon.png");

        // Add markers
        layer.AddMarker(SphericalMercator.FromLonLat(9.0, 48.0), title: "New York")
            .AddMarker(SphericalMercator.FromLonLat(9.1, 48.1), color: Color.Green, scale: 0.75, title: "Amsterdam")
            .AddMarker(SphericalMercator.FromLonLat(9.0, 48.1), color: Color.Blue, scale: 0.5, title: "Berlin")
            .AddMarker(SphericalMercator.FromLonLat(9.1, 48.0), title: "Madrid", svg: tiger, scale: 0.1, anchor: new Offset(0.3, -0.8, true)) // Set center point to 30 % in x and -80 % in y direction
            .AddMarker(SphericalMercator.FromLonLat(9.05, 48.05), title: "San Fransisco", icon: icon, anchor: new Offset(0.5, 0.5, true));

        // Zoom and center map
        var center = layer.Extent?.Centroid ?? new MPoint(SphericalMercator.FromLonLat(9.05, 48.05));
        var extent = layer.Extent?.Grow(2000) ?? new MRect(SphericalMercator.FromLonLat(8.95, 47.95), SphericalMercator.FromLonLat(9.15, 48.15));

        map.Navigator.CenterOn(center);
        map.Navigator.ZoomToBox(extent);

        return map;
    }

    private static string GetSvgFromResources(string name)
    {
        using var stream = typeof(PinSample).Assembly.GetManifestResourceStream(typeof(PinSample).Assembly.GetFullName(name));
        if (stream == null) return string.Empty;
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    private static byte[]? GetIconFromResources(string name)
    {
        using var stream2 = EmbeddedResourceLoader.Load(name, typeof(PinSample));
        if (stream2 == null) return null;
        using var reader2 = new MemoryStream();
        stream2.CopyTo(reader2);

        return reader2.ToArray();
    }
}
