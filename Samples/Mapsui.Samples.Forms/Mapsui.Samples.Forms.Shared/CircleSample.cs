using System;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;
#if __MAUI__
using Mapsui.UI.Maui;
using Microsoft.Maui;

using Color = Microsoft.Maui.Graphics.Color;
#else
using Mapsui.UI.Forms;
using Xamarin.Forms;
#endif

#if __MAUI__
namespace Mapsui.Samples.Maui;
#else
namespace Mapsui.Samples.Forms;
#endif

public class CircleSample : IFormsSample
{
    private static Random rnd = new Random(1);

    public string Name => "Add Circle Sample";

    public string Category => "Forms";

    public bool OnClick(object? sender, EventArgs args)
    {
        var mapView = sender as MapView;
        var e = args as MapClickedEventArgs;

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
