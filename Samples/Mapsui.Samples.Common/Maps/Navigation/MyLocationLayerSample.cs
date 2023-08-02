using System;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Tiling;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Navigation;

public class MyLocationLayerSample : ISample, IDisposable
{
    private MyLocationLayer? _myLocationLayer;
    private bool _disposed;
    private (MPoint, double, double, double, bool, bool, bool)[] _points = new (MPoint, double, double, double, bool, bool, bool)[0];
    private int _count = 0;

    public string Name => "MyLocationLayer";

    public string Category => "Navigation";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        _myLocationLayer?.Dispose();
        _myLocationLayer = new MyLocationLayer(map)
        { 
            IsCentered = false,
        };

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(_myLocationLayer);

        // Get the lon lat coordinates from somewhere (Mapsui can not help you there)
        var centerOfLondonOntario = new MPoint(-81.2497, 42.9837);
        // OSM uses spherical mercator coordinates. So transform the lon lat coordinates to spherical mercator
        var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(centerOfLondonOntario.X, centerOfLondonOntario.Y).ToMPoint();
        // Set the center of the viewport to the coordinate. The UI will refresh automatically
        // Additionally you might want to set the resolution, this could depend on your specific purpose
        map.Home = n => n.CenterOnAndZoomTo(sphericalMercatorCoordinate, n.Resolutions[9]);

        _myLocationLayer.UpdateMyLocation(sphericalMercatorCoordinate, true);

        _points = CreatePoints(centerOfLondonOntario);

        map.Info += (s, e) =>
        {
            if (_count >= 20)
                _count = 0;

            _myLocationLayer.IsCentered = _points[_count].Item5;
            _myLocationLayer.IsMoving = _points[_count].Item6;
            _myLocationLayer.UpdateMyLocation(_points[_count].Item1, _points[_count].Item7);
            _myLocationLayer.UpdateMyDirection(_points[_count].Item2, map.Navigator.Viewport.Rotation, _points[_count].Item7);
            _myLocationLayer.UpdateMyViewDirection(_points[_count].Item3, map.Navigator.Viewport.Rotation, _points[_count].Item7);
            _myLocationLayer.UpdateMySpeed(_points[_count].Item4);

            _count++;
        };

        return Task.FromResult(map);
    }

    private (MPoint, double, double, double, bool, bool, bool)[] CreatePoints(MPoint center)
    {
        var result = new (MPoint, double, double, double, bool, bool, bool)[20];
        var rand = new Random();

        for (var i = 0; i < 20; i++)
        {
            result[i].Item1 = SphericalMercator.FromLonLat(center.X + rand.NextDouble() * 0.5, center.Y + rand.NextDouble() * 0.5).ToMPoint();
            result[i].Item2 = rand.NextDouble() * 360.0;
            result[i].Item3 = rand.NextDouble() * 360.0;
            result[i].Item4 = rand.NextDouble() > 0.5 ? 1.0 : 0.0;
            result[i].Item5 = rand.NextDouble() > 0.5 ? true : false;
            result[i].Item6 = rand.NextDouble() > 0.5 ? true : false;
            result[i].Item7 = rand.NextDouble() > 0.5 ? true : false;
        }

        return result;
    }

    public virtual void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _myLocationLayer?.Dispose();
    }

    protected virtual void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
