using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;
using Mapsui.UI.Maui;

namespace Mapsui.Samples.Maui;

public class SnapshotSample : IMapViewSample
{
    public string Name => "Snapshot Sample";

    public string Category => "MapView";

    public bool UpdateLocation => true;

    public bool OnTap(object? s, MapClickedEventArgs e)
    {
        var mapView = s as UI.Maui.MapView;

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
