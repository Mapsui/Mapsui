#if NET6_0_OR_GREATER

using System;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;
#if __MAUI__
using Mapsui.UI.Maui;
#else
using Mapsui.UI.Forms;
#endif

#if __MAUI__
namespace Mapsui.Samples.Maui;
#else
namespace Mapsui.Samples.Forms;
#endif

public sealed class AnimatedMyLocationSample : IMapViewSample, IDisposable
{
    private readonly PeriodicTimer _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
    private Position _newLocation;
    private MapView? _mapView;

    public string Name => "Animated MyLocation Sample";

    public string Category => "MapView";

    public void Setup(IMapControl mapControl)
    {
        // 54°54′24″N 25°19′12″E Center of Europe
        _newLocation = new Position(54.5424, 25.1912);

        _mapView = (MapView)mapControl;
        var map = OsmSample.CreateMap();
        map.Home = n => n.CenterOnAndZoomTo(_newLocation.ToMapsui(), n.Resolutions[14]);
        mapControl.Map = map;

        _mapView.MyLocationLayer.IsMoving = true;
        _mapView.MyLocationEnabled = true;
        _mapView.MyLocationFollow = true;
        _mapView.UseDoubleTap = true;

        _mapView.MyLocationLayer.UpdateMyLocation(_newLocation);
        _mapView.Map.Navigator.CenterOn(_newLocation.ToMapsui());

        Catch.TaskRun(RunTimerAsync);
    }

    public bool UpdateLocation => false;
    public bool OnClick(object? sender, EventArgs args)
    {
        return true; 
    }

    private async Task RunTimerAsync()
    {
        while(true)
        {
            await _timer.WaitForNextTickAsync();

            _newLocation = new (_newLocation.Latitude + 0.00005, _newLocation.Longitude + 0.00005);                                

            _mapView?.MyLocationLayer.UpdateMyLocation(_newLocation, true);
            _mapView?.MyLocationLayer.UpdateMyDirection(_mapView.MyLocationLayer.Direction + 10, 0, true);
            _mapView?.MyLocationLayer.UpdateMyViewDirection(_mapView.MyLocationLayer.ViewingDirection + 10, 0, true);
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
#endif
