using Mapsui.UI;

// ReSharper disable once CheckNamespace
namespace Mapsui;

public  interface IMapView : IMapControl
{
    bool MyLocationEnabled { get; set; }
    bool UseDoubleTap { get; set; }
    IMyLocationLayer MyLocationLayer { get; }
    bool MyLocationFollow { get; set; }
}
