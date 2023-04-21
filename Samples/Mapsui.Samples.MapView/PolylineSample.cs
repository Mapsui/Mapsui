using System;
using System.Linq;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;

#if __MAUI__
using Mapsui.UI.Maui;
#elif __UWP__
using Mapsui.UI.Uwp;
#elif __ANDROID__ && !HAS_UNO_WINUI
using Mapsui.UI.Android;
#elif __IOS__ && !HAS_UNO_WINUI && !__FORMS__
using Mapsui.UI.iOS;
#elif __WINUI__
using Mapsui.UI.WinUI;
#elif __FORMS__
using Mapsui.UI.Forms;
#elif __AVALONIA__
using Mapsui.UI.Avalonia;
#elif __ETO_FORMS__
using Mapsui.UI.Eto;
#elif __BLAZOR__
using Mapsui.UI.Blazor;
#elif __WPF__
using Mapsui.UI.Wpf;
#else
using Mapsui.UI;
#endif

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
