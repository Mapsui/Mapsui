using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using System;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Extensions;

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

        // Add a OSM map
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        // Add a scalebar
        map.Widgets.Add(new ScaleBarWidget(map) { TextAlignment = Alignment.Center, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top });
        // Create layer for markers
        using var markerLayer = map.AddMarkerLayer("Marker")
            // Create marker for NYC
            .AddMarker(SphericalMercator.FromLonLat(-73.935242, 40.730610), Color.Red, 1.0, "New York City")
            // Create marker for Boston
            .AddMarker(SphericalMercator.FromLonLat(-71.057083, 42.361145), Color.LightGreen, 0.8, "Boston", "MA")
            // Create marker for Washington DC
            .AddMarker(SphericalMercator.FromLonLat(-77.03637, 38.89511),
                color: DemoColor(),
                scale: 1.5,
                title: "Washington DC",
                touched: MarkerTouched);

        // Zoom map, so that all markers are visible
        map.Navigator.ZoomToBox(markerLayer.Extent);

        return map;
    }

    private static Random _rand = new(1);

    /// <summary>
    /// Create a random color
    /// </summary>
    /// <returns>Color created</returns>
    private static Color DemoColor()
    {
        return new Color(_rand.Next(128, 256), _rand.Next(128, 256), _rand.Next(128, 256));
    }

    /// <summary>
    /// Function called when marker is touched/tapped/clicked
    /// </summary>
    /// <param name="layer">Layer feature belonging too</param>
    /// <param name="feature">Feature that is hit</param>
    /// <param name="args">Parameters from touch event</param>
    private static void MarkerTouched(ILayer layer, IFeature feature, MapInfoEventArgs args)
    {
        if (feature is not PointFeature marker || layer is not MemoryLayer)
            return;

        // Change color of marker
        marker.SetColor(DemoColor())
            .SetSubtitle(String.IsNullOrEmpty(marker.GetSubtitle()) ? "0" : (int.Parse(marker.GetSubtitle()) + 1).ToString())
            .ShowCallout(layer);

        // We handled this event, so there isn't the default handling (show callout) needed
        args.Handled = true;
    }
}
