using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;

#pragma warning disable IDISP001 // Dispose Created

#if NET6_0_OR_GREATER

namespace Mapsui.Samples.Common.Maps.Animations;

internal sealed class BusPointProvider : MemoryProvider, IDynamic, IDisposable
{
    public event DataChangedEventHandler? DataChanged;
        
    private readonly PeriodicTimer _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

    public BusPointProvider()
    {            
        Catch.TaskRun(RunTimerAsync);
    }

    private (double Lon, double Lat) _prevCoords = (24.945831, 60.192059);
    private async Task RunTimerAsync()
    {
        while(true)
        {
            await _timer.WaitForNextTickAsync();
                
            _prevCoords = (_prevCoords.Lon + 0.00005, _prevCoords.Lat + 0.00005);                                

            OnDataChanged();
        }
    }

    void IDynamic.DataHasChanged()
    {
        OnDataChanged();
    }

    private void OnDataChanged()
    {
        DataChanged?.Invoke(this, new DataChangedEventArgs(null, false, null));
    }
        
    public override Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        var busFeature = new PointFeature(SphericalMercator.FromLonLat(_prevCoords.Lon, _prevCoords.Lat).ToMPoint());
        busFeature["ID"] = "bus";
        return Task.FromResult((IEnumerable<IFeature>)new[] { busFeature });
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
#endif
