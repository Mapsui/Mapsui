using System;
using System.Linq;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;
#if __MAUI__
using Mapsui.UI.Maui;
using Microsoft.Maui.Graphics;
using KnownColor = Mapsui.UI.Maui.KnownColor;
#else
using Mapsui.UI.Forms;
using Xamarin.Forms;
using KnownColor = Xamarin.Forms.Color;
#endif

#if __MAUI__
namespace Mapsui.Samples.Maui;
#else
namespace Mapsui.Samples.Forms;
#endif

public class PolylineSample : IFormsSample
{
    public string Name => "Add Polyline Sample";

    public string Category => "Forms";

    public bool OnClick(object? sender, EventArgs args)
    {
        var mapView = sender as MapView;
        var e = args as MapClickedEventArgs;

        if (mapView == null)
            return false;

        if (e == null)
            return false;

        UI.Objects.Drawable f;

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

        ((MapView)mapControl).UseDoubleTap = false;
    }
}
