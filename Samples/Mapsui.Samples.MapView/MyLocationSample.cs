using System;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;
using Mapsui.UI.Maui;

namespace Mapsui.Samples.Maui;

public class MyLocationSample : IMapViewSample
{
    public string Name => "MyLocation Sample";

    public string Category => "MapView";

    public bool UpdateLocation => true;

    public bool OnClick(object? sender, EventArgs args)
    {
        var mapView = sender as MapView;
        var e = args as MapClickedEventArgs;

        if (mapView == null)
            return false;

        mapView.MyLocationLayer.IsMoving = mapView.MyLocationEnabled;
        mapView.MyLocationEnabled = true;
        mapView.UseDoubleTap = true;

        return false;
    }

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = OsmSample.CreateMap();
    }
}
