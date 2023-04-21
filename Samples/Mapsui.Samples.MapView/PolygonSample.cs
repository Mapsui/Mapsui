using System;
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

public class PolygonSample : IMapViewSample
{
    static readonly Random random = new Random(1);

    public string Name => "Add Polygon Sample";

    public string Category => "MapView";

    public bool OnClick(object? sender, EventArgs args)
    {
        var mapView = sender as IMapView;
        var e = args as IMapClicked;

        if (e == null)
            return false;

        var center = new Position(e.Point);
        var diffX = random.Next(0, 1000) / 100.0;
        var diffY = random.Next(0, 1000) / 100.0;

        var polygon = new Polygon
        {
            StrokeColor = new Color(random.Next(0, 255) / 255.0f, random.Next(0, 255) / 255.0f, random.Next(0, 255) / 255.0f),
            FillColor = new Color(random.Next(0, 255) / 255.0f, random.Next(0, 255) / 255.0f, random.Next(0, 255) / 255.0f)
        };

        polygon.Positions.Add(new Position(center.Latitude - diffY, center.Longitude - diffX));
        polygon.Positions.Add(new Position(center.Latitude + diffY, center.Longitude - diffX));
        polygon.Positions.Add(new Position(center.Latitude + diffY, center.Longitude + diffX));
        polygon.Positions.Add(new Position(center.Latitude - diffY, center.Longitude + diffX));

        // Be carefull: holes should have other direction of Positions.
        // If Positions is clockwise, than Holes should all be counter clockwise and the other way round.
        polygon.Holes.Add(new Position[] {
            new Position(center.Latitude - diffY * 0.3, center.Longitude - diffX * 0.3),
            new Position(center.Latitude + diffY * 0.3, center.Longitude + diffX * 0.3),
            new Position(center.Latitude + diffY * 0.3, center.Longitude - diffX * 0.3),
        });

        polygon.IsClickable = true;
        polygon.Clicked += (s, a) =>
        {
            if (s is Polygon p)
            {
                p.FillColor = new Color(random.Next(0, 255) / 255.0f, random.Next(0, 255) / 255.0f, random.Next(0, 255) / 255.0f);
                a.Handled = true;
            }
        };

        mapView?.Drawables.Add(polygon);

        return true;
    }

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = OsmSample.CreateMap();
    }
}
