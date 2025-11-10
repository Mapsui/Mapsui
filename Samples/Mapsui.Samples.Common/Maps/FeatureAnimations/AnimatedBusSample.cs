using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Layers.AnimatedLayers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Tiling;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.FeatureAnimations;

public class AnimatedBusSample : ISample
{
    public string Name => "AnimatedBus";

    public string Category => "FeatureAnimations";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map!.Layers.Add(OpenStreetMap.CreateTileLayer("AnimatedBusSamplesUserAgent"));
        map.Layers.Add(new AnimatedPointLayer(new BusPointProvider())
        {
            Name = "Buses",
            Style = new LabelStyle
            {
                BackColor = new Brush(Color.Black),
                ForeColor = Color.White,
                Text = "Bus",
            }
        });

        map.CRS = "EPSG:3857";
        map.Navigator.CenterOnAndZoomTo(new MPoint(2776952, 8442653), map.Navigator.Resolutions[18]);

        return Task.FromResult(map);
    }

    internal sealed class BusPointProvider : MemoryProvider, IDynamic, IDisposable
    {
        public event EventHandler? DataChanged;

        private readonly PeriodicTimer _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        public BusPointProvider()
        {
            Catch.TaskRun(RunTimerAsync);
        }

        private (double Lon, double Lat) _previousCoordinates = (24.945831, 60.192059);
        private async Task RunTimerAsync()
        {
            while (true)
            {
                await _timer.WaitForNextTickAsync();

                _previousCoordinates = (_previousCoordinates.Lon + 0.00005, _previousCoordinates.Lat + 0.00005);

                OnDataChanged();
            }
        }

        void IDynamic.DataHasChanged()
        {
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, new EventArgs());
        }

        public override Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
        {
            var busFeature = new PointFeature(SphericalMercator.FromLonLat(_previousCoordinates.Lon, _previousCoordinates.Lat).ToMPoint());
            busFeature["ID"] = "bus";
            return Task.FromResult((IEnumerable<IFeature>)[busFeature]);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
