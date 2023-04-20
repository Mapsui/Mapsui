using System;
using System.Linq;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;
using Mapsui.UI.Objects;

// ReSharper disable once CheckNamespace
namespace Mapsui.Samples;

public class PolylineSample : IMapViewSample
{
    public string Name => "Add Polyline Sample";

    public string Category => "MapView";

    public bool OnClick(object? sender, EventArgs args)
    {
        var mapView = sender as IMapView;
        var e = args as IMapClicked;

        if (mapView == null)
            return false;

        if (e == null)
            return false;

        IDrawable f;

        lock (mapView.Drawables)
        {
            if (mapView.Drawables.Count == 0)
            {
                f = new Polyline { StrokeWidth = 4, StrokeColor = KnownColor.Red, IsClickable = true };
                mapView.Drawables.Add(f);
            }
            else
            {
                f = mapView.Drawables.First();
            }

            if (f is Polyline polyline)
            {
                polyline.Positions.Add(e.Point);
            }
        }

        return true;
    }

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = OsmSample.CreateMap();

        ((IMapView)mapControl).UseDoubleTap = false;
    }
}
