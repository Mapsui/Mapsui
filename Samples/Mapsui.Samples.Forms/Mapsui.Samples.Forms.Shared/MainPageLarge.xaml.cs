using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.Samples.CustomWidget;
using Mapsui.Styles;
using Mapsui.UI.Forms;
using Mapsui.UI.Objects;

namespace Mapsui.Samples.Forms;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MainPageLarge : ContentPage
{
    IEnumerable<ISampleBase>? allSamples;
    Func<object?, EventArgs, bool>? clicker;

    public MainPageLarge()
    {
        InitializeComponent();

        allSamples = AllSamples.GetSamples();

        var categories = allSamples.Select(s => s.Category).Distinct().OrderBy(c => c);
        picker.ItemsSource = categories.ToList<string>();
        picker.SelectedIndexChanged += PickerSelectedIndexChanged;
        picker.SelectedItem = "Forms";

        mapView.RotationLock = false;
        mapView.UnSnapRotationDegrees = 30;
        mapView.ReSnapRotationDegrees = 5;

        mapView.PinClicked += OnPinClicked;
        mapView.MapClicked += OnMapClicked;

        mapView.MyLocationLayer.UpdateMyLocation(new UI.Forms.Position());
        mapView.MyLocationLayer.CalloutText = "My location!\n";
        mapView.MyLocationLayer.Clicked += MyLocationClicked;

        mapView.Renderer.WidgetRenders[typeof(CustomWidget.CustomWidget)] = new CustomWidgetSkiaRenderer();

        StartGPS();
    }

    protected override void OnAppearing()
    {
        mapView.Refresh();
    }

    private void FillListWithSamples()
    {
        var selectedCategory = picker.SelectedItem?.ToString() ?? "";
        listView.ItemsSource = allSamples.Where(s => s.Category == selectedCategory).Select(x => x.Name);
    }

    private void PickerSelectedIndexChanged(object sender, EventArgs e)
    {
        FillListWithSamples();
    }

    private void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        e.Handled = clicker != null && (clicker?.Invoke(sender as MapView, e) ?? false);
    }

    void OnSelection(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
        {
            return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
        }

        var sampleName = e.SelectedItem.ToString();
        var sample = allSamples.Where(x => x.Name == sampleName).FirstOrDefault<ISampleBase>();

        if (sample != null)
        {
            mapView.Reset();
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

    private void OnPinClicked(object sender, PinClickedEventArgs e)
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
            if (Device.RuntimePlatform == Device.WPF)
                return;
            // Start GPS
            await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(1),
                    1,
                    true,
                    new ListenerSettings
                    {
                        ActivityType = ActivityType.Fitness,
                        AllowBackgroundUpdates = false,
                        DeferLocationUpdates = true,
                        DeferralDistanceMeters = 1,
                        DeferralTime = TimeSpan.FromSeconds(0.2),
                        ListenForSignificantChanges = false,
                        PauseLocationUpdatesAutomatically = true
                    });

            CrossGeolocator.Current.PositionChanged += MyLocationPositionChanged;
            CrossGeolocator.Current.PositionError += MyLocationPositionError;

        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }
    }

    public async void StopGPS()
    {
        try
        {
            if (Device.RuntimePlatform == Device.WPF)
                return;

            // Stop GPS
            if (CrossGeolocator.Current.IsListening)
            {
                await CrossGeolocator.Current.StopListeningAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }
    }

    /// <summary>
    /// If there was an error while getting GPS coordinates
    /// </summary>
    /// <param name="sender">Geolocator</param>
    /// <param name="e">Event arguments for position error</param>
    private void MyLocationPositionError(object sender, PositionErrorEventArgs e)
    {
    }

    /// <summary>
    /// New informations from Geolocator arrived
    /// </summary>
    /// <param name="sender">Geolocator</param>
    /// <param name="e">Event arguments for new position</param>
    private void MyLocationPositionChanged(object sender, PositionEventArgs e)
    {
        Device.BeginInvokeOnMainThread(() =>
        {
            mapView.MyLocationLayer.UpdateMyLocation(new UI.Forms.Position(e.Position.Latitude, e.Position.Longitude));
            mapView.MyLocationLayer.UpdateMyDirection(e.Position.Heading, mapView.Viewport.Rotation);
            mapView.MyLocationLayer.UpdateMySpeed(e.Position.Speed);
            mapView.MyLocationLayer.CalloutText = $"My location:\nlat={e.Position.Latitude:F6}°\nlon={e.Position.Longitude:F6}°";
        });
    }


    public void MyLocationClicked(object sender, DrawableClickedEventArgs args)
    {
        var myLocLayer = sender as MyLocationLayer;
        args.Handled = true;
        if (myLocLayer == null)
            return;
        // toggle label
        myLocLayer.ShowCallout = !myLocLayer.ShowCallout;
    }
}
