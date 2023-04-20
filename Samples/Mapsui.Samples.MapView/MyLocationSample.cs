using System;
using Mapsui;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;

namespace Mapsui.Samples.MapView;

public class MyLocationSample : IMapViewSample
{
    public string Name => "MyLocation Sample";

    public string Category => "MapView";

    public bool OnClick(object? sender, EventArgs args)
    {
        var mapView = sender as IMapView;

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
