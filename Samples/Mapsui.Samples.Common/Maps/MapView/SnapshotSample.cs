using System;
using System.IO;
using Mapsui.Rendering.Skia;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps.MapView;

public class SnapshotSample : IMapViewSample
{
    public string Name => "Snapshot Sample";

    public string Category => "MapView";

    public bool UpdateLocation => true;

    public bool OnClick(object? sender, EventArgs args)
    {
        var mapView = sender as IMapControl;

        if (mapView == null)
            return false;

        var snapshot = mapView.GetSnapshot();
        using var memoryStream = new MemoryStream(snapshot);
        var test = BitmapHelper.LoadBitmap(memoryStream);

        return true;
    }

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = OsmSample.CreateMap();
    }
}
