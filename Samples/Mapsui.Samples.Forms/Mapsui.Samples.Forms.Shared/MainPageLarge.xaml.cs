using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI.Forms;
using Mapsui.UI.Objects;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Mapsui.Samples.Forms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPageLarge : ContentPage
    {
        Dictionary<string, Func<Map>> allSamples;
        Func<MapView, MapClickedEventArgs, bool> clicker;

        public MainPageLarge()
        {
            InitializeComponent();

            allSamples = Samples.CreateList();

            listView.ItemsSource = allSamples.Select(k => k.Key).ToList();

            mapView.RotationLock = false;
            mapView.UnSnapRotationDegrees = 30;
            mapView.ReSnapRotationDegrees = 5;

            mapView.PinClicked += OnPinClicked;
            mapView.MapClicked += OnMapClicked;

            mapView.MyLocationLayer.UpdateMyLocation(new UI.Forms.Position());

            StartGPS();
        }

        private void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            e.Handled = clicker == null ? false : (bool)clicker?.Invoke(sender as MapView, e);
            //Samples.SetPins(mapView, e);
            //Samples.DrawPolylines(mapView, e);
        }

        void OnSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
            }

            var sample = e.SelectedItem.ToString();
            var call = allSamples[sample];

            mapView.Map = call();

            clicker = Samples.GetClicker(sample);

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
                    e.Pin.IsCalloutVisible = !e.Pin.IsCalloutVisible;
            }

            e.Handled = true;
        }

        public async void StartGPS()
        {
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

        public async void StopGPS()
        {
            // Stop GPS
            if (CrossGeolocator.Current.IsListening)
            {
                await CrossGeolocator.Current.StopListeningAsync();
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
                mapView.MyLocationLayer.UpdateMyDirection(e.Position.Heading, mapView.Map.Viewport.Rotation);
                mapView.MyLocationLayer.UpdateMySpeed(e.Position.Speed);
            });
        }

    }
}