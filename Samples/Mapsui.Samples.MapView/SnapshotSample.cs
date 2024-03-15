using System;
using System.IO;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;
using Mapsui.UI.Maui;
using Microsoft.Maui.Controls;

namespace Mapsui.Samples.Maui;

public class SnapshotSample : IMapViewSample
{
    public string Name => "Snapshot Sample";

    public string Category => "MapView";

    public bool UpdateLocation => true;

    public bool OnTap(object? sender, EventArgs args)
    {
        var mapView = sender as UI.Maui.MapView;
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
