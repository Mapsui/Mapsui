using Mapsui.Rendering.Skia;
using Mapsui.Samples.Common;
using Mapsui.Extensions;
using Mapsui.Samples.CustomWidget;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Logging;
using Mapsui.Samples.Common.Extensions;
using Mapsui.Styles;
using Mapsui.UI.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Devices.Sensors;

namespace Mapsui.Samples.Maui;

public sealed partial class MainPageLarge : ContentPage, IDisposable
{
    static MainPageLarge()
    {
        // todo: find proper way to load assembly
        Mapsui.Tests.Common.Utilities.LoadAssembly();
    }

    IEnumerable<ISampleBase> allSamples;
    Func<object?, EventArgs, bool>? clicker;
    private CancellationTokenSource? gpsCancelation;

    public MainPageLarge()
    {
        InitializeComponent();

        // nullable warning workaround
        var test = this.listView ?? throw new InvalidOperationException();
        var test2 = this.featureInfo ?? throw new InvalidOperationException();

        allSamples = AllSamples.GetSamples() ?? new List<ISampleBase>();

        var categories = allSamples.Select(s => s.Category).Distinct().OrderBy(c => c);
        picker!.ItemsSource = categories.ToList<string>();
        picker.SelectedIndexChanged += PickerSelectedIndexChanged;
        picker.SelectedItem = "Forms";

        mapView!.RotationLock = false;
        mapView.UnSnapRotationDegrees = 30;
        mapView.ReSnapRotationDegrees = 5;

        mapView.PinClicked += OnPinClicked;
        mapView.MapClicked += OnMapClicked;

        mapView.MyLocationLayer.UpdateMyLocation(new UI.Maui.Position());

        mapView.IsZoomButtonVisible = true;
        mapView.IsMyLocationButtonVisible = true;
        mapView.IsNorthingButtonVisible = true;

        mapView.Info += MapView_Info;
        mapView.Renderer.WidgetRenders[typeof(CustomWidget.CustomWidget)] = new CustomWidgetSkiaRenderer();

        StartGPS();
    }

    protected override void OnAppearing()
    {
        mapView.Refresh();
    }

    private void MapView_Info(object? sender, UI.MapInfoEventArgs? e)
    {
        featureInfo.Text = $"Click Info:";

        if (e?.MapInfo?.Feature != null)
        {
            featureInfo.Text = $"Click Info:{Environment.NewLine}{e.MapInfo.Feature.ToDisplayText()}";

            foreach (var style in e.MapInfo.Feature.Styles)
            {
                if (style is CalloutStyle)
                {
                    style.Enabled = !style.Enabled;
                    e.Handled = true;
                }
            }

            mapView.Refresh();
        }
    }

    private void FillListWithSamples()
    {
        var selectedCategory = picker.SelectedItem?.ToString() ?? "";
        listView.ItemsSource = allSamples.Where(s => s.Category == selectedCategory).Select(x => x.Name);
    }

    private void PickerSelectedIndexChanged(object? sender, EventArgs e)
    {
        FillListWithSamples();
    }

    private void OnMapClicked(object? sender, MapClickedEventArgs e)
    {
        e.Handled = clicker?.Invoke(sender as MapView, e) ?? false;
    }

    void OnSelection(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
        {
            return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
        }

        var sampleName = e.SelectedItem.ToString();
        var sample = allSamples.FirstOrDefault(x => x.Name == sampleName);

        if (sample != null)
        {
            Catch.Exceptions(async () =>
            {
                await sample.SetupAsync(mapView);
            });

        }

        clicker = null;
        if (sample is IFormsSample formsSample)
            clicker = formsSample.OnClick;

        listView.SelectedItem = null;
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
            this.gpsCancelation?.Dispose();
            this.gpsCancelation = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                while (!gpsCancelation.IsCancellationRequested)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
#if __MAUI__ // WORKAROUND for Preview 11 will be fixed in Preview 13 https://github.com/dotnet/maui/issues/3597
                    if (Application.Current == null)
                        return;

                    await Application.Current.Dispatcher.DispatchAsync(async () =>
                    {
#else
                    await Device.InvokeOnMainThreadAsync(async () => {
#endif
                        var location = await Geolocation.GetLocationAsync(request, this.gpsCancelation.Token)
                            .ConfigureAwait(false);
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
        this.gpsCancelation?.Cancel();
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
                mapView?.MyLocationLayer.UpdateMyLocation(new UI.Maui.Position(e.Latitude, e.Longitude));
                if (e.Course != null)
                {
                    mapView?.MyLocationLayer.UpdateMyDirection(e.Course.Value, mapView?.Viewport.Rotation ?? 0);
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

    public void Dispose()
    {
        ((IDisposable?)gpsCancelation)?.Dispose();
    }
}
