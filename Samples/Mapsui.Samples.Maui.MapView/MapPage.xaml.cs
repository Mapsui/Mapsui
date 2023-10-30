using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.Samples.CustomWidget;
using Mapsui.Styles;
using Mapsui.UI.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Compass = Microsoft.Maui.Devices.Sensors.Compass;
using Microsoft.Maui.Dispatching;

namespace Mapsui.Samples.Maui;

public sealed partial class MapPage : ContentPage, IDisposable
{
    private CancellationTokenSource? gpsCancelation;
    public Func<MapView?, MapClickedEventArgs, bool>? Clicker { get; set; }

    public MapPage()
    {
        InitializeComponent();

        // nullable warning workaround
        var test = mapView ?? throw new InvalidOperationException();
        var test1 = info ?? throw new InvalidOperationException();
    }

    public MapPage(ISampleBase sample, Func<MapView?, MapClickedEventArgs, bool>? c = null)
    {
        InitializeComponent();

        // nullable warning workaround
        var test = mapView ?? throw new InvalidOperationException();
        var test1 = info ?? throw new InvalidOperationException();

        mapView!.RotationLock = false;
        mapView.UnSnapRotationDegrees = 20;
        mapView.ReSnapRotationDegrees = 5;

        mapView.PinClicked += OnPinClicked;
        mapView.MapClicked += OnMapClicked;

        Compass.ReadingChanged += Compass_ReadingChanged;

        mapView.MyLocationLayer.UpdateMyLocation(new Mapsui.UI.Maui.Position());

        mapView.Info += MapView_Info;
        mapView.Renderer.WidgetRenders[typeof(CustomWidget.CustomWidget)] = new CustomWidgetSkiaRenderer();

        Catch.TaskRun(StartGPS);

        try
        {
            if (!Compass.IsMonitoring)
                Compass.Start(SensorSpeed.Default);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warning, ex.Message, ex);
        }

        Catch.Exceptions(async () =>
        {
            await sample.SetupAsync(mapView);
        });

        Clicker = c;
    }

    protected override void OnAppearing()
    {
        mapView.IsVisible = true;
        mapView.Refresh();
    }

    private void MapView_Info(object? sender, Mapsui.MapInfoEventArgs? e)
    {
        if (e?.MapInfo?.Feature != null)
        {
            foreach (var style in e.MapInfo.Feature.Styles)
            {
                if (style is CalloutStyle)
                {
                    style.Enabled = !style.Enabled;
                    e.Handled = true;
                }
            }

            mapView.RefreshGraphics();
        }
    }

    private void OnMapClicked(object? sender, MapClickedEventArgs e)
    {
        e.Handled = Clicker?.Invoke(sender as MapView, e) ?? false;
        //Samples.SetPins(mapView, e);
        //Samples.DrawPolylines(mapView, e);
    }

    private void OnPinClicked(object? sender, PinClickedEventArgs e)
    {
        if (e.Pin != null)
        {
            if (e.NumOfTaps == 2)
            {
                // Hide Pin when double click
                //DisplayAlert($"Pin {e.Pin.Label}", $"Is at position {e.Pin.Position}", "Ok");
                e.Pin.IsVisible = false;
            }
            if (e.NumOfTaps == 1)
                if (e.Pin.Callout.IsVisible)
                    e.Pin.HideCallout();
                else
                    e.Pin.ShowCallout();
        }

        e.Handled = true;
    }

    public async void StartGPS()
    {
        try
        {
            gpsCancelation?.Dispose();
            gpsCancelation = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                while (!gpsCancelation.IsCancellationRequested)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                    Application.Current?.Dispatcher.DispatchAsync(async () =>
                    {
                        var location = await Geolocation.GetLocationAsync(request, gpsCancelation.Token).ConfigureAwait(false);
                        if (location != null)
                        {
                            MyLocationPositionChanged(location);
                        }
                    }).ConfigureAwait(false);

                    await Task.Delay(200).ConfigureAwait(false);
                }
            }, gpsCancelation.Token).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Logging.Logger.Log(LogLevel.Error, e.Message, e);
        }
    }

    public void StopGPS()
    {
        gpsCancelation?.Cancel();
    }

    /// <summary>
    /// New informations from Geolocator arrived
    /// </summary>        
    /// <param name="e">Event arguments for new position</param>
    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
    private async void MyLocationPositionChanged(Location e)
    {
        try
        {
            await Application.Current?.Dispatcher?.DispatchAsync(() =>
            {
                mapView?.MyLocationLayer.UpdateMyLocation(new Position(e.Latitude, e.Longitude));
                if (e.Course != null)
                {
                    mapView?.MyLocationLayer.UpdateMyDirection(e.Course.Value, mapView?.Map.Navigator.Viewport.Rotation ?? 0);
                }

                if (e.Speed != null)
                {
                    mapView?.MyLocationLayer.UpdateMySpeed(e.Speed.Value);
                }
            })!;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }
    }

    private void Compass_ReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        mapView.MyLocationLayer.UpdateMyViewDirection(e.Reading.HeadingMagneticNorth, mapView.Map.Navigator.Viewport.Rotation, false);
    }

    public void Dispose()
    {
        ((IDisposable?)gpsCancelation)?.Dispose();
    }
}
