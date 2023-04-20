using System;
using System.IO;
using Mapsui.Rendering.Skia;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;

// ReSharper disable once CheckNamespace
namespace Mapsui.Samples;

public class SnapshotSample : IMapViewSample
{
    public string Name => "Snapshot Sample";

    public string Category => "MapView";

    public bool OnClick(object? sender, EventArgs args)
    {
        var mapView = sender as IMapView;        

        if (mapView == null)
            return false;

        var snapshot = mapView.GetSnapshot();
        using var bitmapStream = new MemoryStream(snapshot);
        var test = BitmapHelper.LoadBitmap(bitmapStream);

        return true;
    }

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = OsmSample.CreateMap();
    }
}
