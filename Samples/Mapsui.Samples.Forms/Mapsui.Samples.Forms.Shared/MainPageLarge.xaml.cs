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
        IEnumerable<ISample> allSamples;
        Func<object, EventArgs, bool> clicker;

        public MainPageLarge()
        {
            InitializeComponent();

            allSamples = AllSamples.GetSamples();

            var categories = allSamples.Select(s => s.Category).Distinct().OrderBy(c => c);
            foreach (var category in categories)
            {
                picker.Items?.Add(category);
            }
            picker.SelectedIndexChanged += PickerSelectedIndexChanged;
            picker.SelectedItem = "Forms";

            mapView.RotationLock = false;
            mapView.UnSnapRotationDegrees = 30;
            mapView.ReSnapRotationDegrees = 5;

            mapView.PinClicked += OnPinClicked;
            mapView.MapClicked += OnMapClicked;

            mapView.MyLocationLayer.UpdateMyLocation(new UI.Forms.Position());

            mapView.IsZoomButtonVisible = true;
            mapView.IsMyLocationButtonVisible = true;
            mapView.IsNorthingButtonVisible = true;

            StartGPS();
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
            e.Handled = clicker == null ? false : (bool)clicker?.Invoke(sender as MapView, e);
        }

        async void OnSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
            }

            var sampleName = e.SelectedItem.ToString();
            var sample = allSamples.Where(x => x.Name == sampleName).FirstOrDefault<ISample>();

            if (sample != null)
            {
                sample.Setup(mapView);
            }

            clicker = null;
            if (sample is IFormsSample)
                clicker = ((IFormsSample)sample).OnClick;

            await CenterOnLocationAsync();

            // deselect the item so that re-clicking re-loads
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

        public async void StopGPS()
        {
            if (Device.RuntimePlatform == Device.WPF)
                return;

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
            System.Diagnostics.Debug.WriteLine($"{nameof(MyLocationPositionChanged)}()---  args: [{e.Position.Latitude}, {e.Position.Longitude}]");
                
            Device.BeginInvokeOnMainThread(() =>
            {
                mapView.MyLocationLayer.UpdateMyLocation(new UI.Forms.Position(e.Position.Latitude, e.Position.Longitude));
                mapView.MyLocationLayer.UpdateMyDirection(e.Position.Heading, mapView.Viewport.Rotation);
                mapView.MyLocationLayer.UpdateMySpeed(e.Position.Speed);
            });
        }


        private async System.Threading.Tasks.Task CenterOnLocationAsync()
        {
            Plugin.Geolocator.Abstractions.Position p = await GetCurrentLocationAsnyc();

            if (p != null)
            {
                System.Diagnostics.Debug.WriteLine($"{nameof(OnSelection)}()---  location: [{p.Latitude}, {p.Longitude}]");

                CenterOnLocation(p);
            }
        }

        private static async System.Threading.Tasks.Task<Plugin.Geolocator.Abstractions.Position> GetCurrentLocationAsnyc()
        {
            var p = await CrossGeolocator.Current.GetLastKnownLocationAsync();

            return p ?? await CrossGeolocator.Current.GetPositionAsync();
        }

        private void CenterOnLocation(Plugin.Geolocator.Abstractions.Position geoLoc)
        {
            if (geoLoc.Latitude != 0 || geoLoc.Latitude != 0) // uninitialized
            {
                mapView.Navigator.CenterOn(Mapsui.Projection.SphericalMercator.FromLonLat(geoLoc.Longitude, geoLoc.Latitude));
            }
        }

        private async void OnCreatePinBtnClicked(object sender, EventArgs e)
        {
            //test deadlock on UI thread...

            var geo = await GetCurrentLocationAsnyc();

            var p = new Mapsui.UI.Forms.Position(geo.Latitude, geo.Longitude);

            var pin = new Pin(mapView)
            {
                Label = $"UI thread created @ your location",
                Address = p.ToString(),
                Position = p,
                Type = PinType.Pin,
                Color = Color.DarkRed,
                Transparency = .05f,
                Scale = .91f,
            };

            pin.CalloutAnchor = new Point(0, pin.Height * pin.Scale + 0);
            //await System.Threading.Tasks.Task.Run(() =>
            //{
                pin.Callout.RectRadius = 20;
                pin.Callout.ArrowHeight = 20;
                pin.Callout.ArrowWidth = 20;
                pin.Callout.ArrowAlignment = ArrowAlignment.Bottom;
                pin.Callout.ArrowPosition = 0.5f;
                pin.Callout.SubtitleLabel.LineBreakMode = LineBreakMode.NoWrap;
            //});

            mapView.Pins.Add(pin);

        }
    }
}