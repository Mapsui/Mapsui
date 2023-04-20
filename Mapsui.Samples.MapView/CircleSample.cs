using System;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;
using Mapsui.UI.Objects;
using Mapsui.Utilities;

// ReSharper disable once CheckNamespace
namespace Mapsui.Samples;

public class CircleSample : IMapViewSample
{
    private static Random rnd = new Random(1);

    public string Name => "Add Circle Sample";

    public string Category => "MapView";

    public bool OnClick(object? sender, EventArgs args)
    {
        var mapView = sender as IMapView;
        var e = args as IMapClicked;

        if (e == null)
            return false;

        if (mapView == null)
            return false;

        var circle = new Circle
        {
            Center = e.Point,
            Radius = Distance.FromMeters(rnd.Next(100000, 1000000)),
            Quality = rnd.Next(0, 60),
            StrokeColor = new Color(rnd.Next(0, 255) / 255.0f, rnd.Next(0, 255) / 255.0f, rnd.Next(0, 255) / 255.0f),
            StrokeWidth = rnd.Next(1, 5),
            FillColor = new Color(rnd.Next(0, 255) / 255.0f, rnd.Next(0, 255) / 255.0f, rnd.Next(0, 255) / 255.0f, rnd.Next(0, 255) / 255.0f)
        };

        mapView.Drawables.Add(circle);

        return true;
    }

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = OsmSample.CreateMap();
    }
}
