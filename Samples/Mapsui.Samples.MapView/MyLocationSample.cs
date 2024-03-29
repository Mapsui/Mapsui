using System;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;

namespace Mapsui.Samples.Maui;

public class MyLocationSample : IMapViewSample
{
    public string Name => "MyLocation Sample";

    public string Category => "MapView";

    public bool UpdateLocation => true;

    public bool OnTap(object? sender, EventArgs args)
    {
        if (sender is UI.Maui.MapView mapView)
        {
            mapView.MyLocationLayer.IsMoving = mapView.MyLocationEnabled;
            mapView.MyLocationEnabled = true;
        }
        return false;
    }

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = OsmSample.CreateMap();
    }
}
