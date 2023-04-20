using Mapsui.UI;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Mapsui;

public  interface IMapView : IMapControl
{
    bool MyLocationEnabled { get; set; }
    bool UseDoubleTap { get; set; }
    bool UniqueCallout { get; set; }
    IMyLocationLayer MyLocationLayer { get; }
    bool MyLocationFollow { get; set; }
    IList<IPin> Pins { get; }
    IList<IDrawable> Drawables { get; }
    void RemoveCallout(ICallout callout);
    void AddCallout(ICallout callout);
    bool IsCalloutVisible(ICallout callout);
}
