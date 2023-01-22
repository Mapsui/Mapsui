using System;
using System.IO;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;
#if __MAUI__
using Mapsui.UI.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
#else
using Mapsui.UI.Forms;
using Xamarin.Forms;
#endif

#if __MAUI__
namespace Mapsui.Samples.Maui;
#else
namespace Mapsui.Samples.Forms;
#endif

public class SnapshotSample : IFormsSample
{
    public string Name => "Snapshot Sample";

    public string Category => "Forms";

    public bool OnClick(object? sender, EventArgs args)
    {
        var mapView = sender as MapView;
        var e = args as MapClickedEventArgs;

        if (mapView == null)
            return false;

        var snapshot = mapView.GetSnapshot();
        var test = ImageSource.FromStream(() => new MemoryStream(snapshot));

        return true;
    }

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = OsmSample.CreateMap();
    }
}
