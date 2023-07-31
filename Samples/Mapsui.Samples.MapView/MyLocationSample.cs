using System;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;
#if __MAUI__
using Mapsui.UI.Maui;
#else
using Mapsui.UI.Forms;
#endif

namespace Mapsui.Samples.Maps.MapView;

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
