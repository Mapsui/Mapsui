using System;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;
using Mapsui.Utilities;

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
