using Mapsui.Rendering.Skia;
using Mapsui.Samples.Common;
using Mapsui.Extensions;
using Mapsui.Samples.CustomWidget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Styles;
using Mapsui.UI.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

namespace Mapsui.Samples.Maui
{
    public partial class MainPageLarge : ContentPage
    {
        IEnumerable<ISample> allSamples;
        Func<object?, EventArgs, bool>? clicker;
        private CancellationTokenSource? gpsCancelation;

        public MainPageLarge()
        {
            InitializeComponent();

            // nullable warning workaround
            var test = this.listView ?? throw new InvalidOperationException();
            var test2 = this.featureInfo ?? throw new InvalidOperationException();

            allSamples = AllSamples.GetSamples() ?? new List<ISample>();

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
                sample.Setup(mapView);
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
            this.gpsCancelation = new CancellationTokenSource();

            await Task.Run(async () => {
                while (!gpsCancelation.IsCancellationRequested)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                    await Device.InvokeOnMainThreadAsync(async () => {
                        var location = await Geolocation.GetLocationAsync(request, this.gpsCancelation.Token).ConfigureAwait(false);
                        if (location != null)
                        {
                            MyLocationPositionChanged(location);
                        }
                    }).ConfigureAwait(false);

                    await Task.Delay(200).ConfigureAwait(false);
                }
            }, gpsCancelation.Token).ConfigureAwait(false);
        }

        public void StopGPS()
        {
            this.gpsCancelation?.Cancel();
        }

        /// <summary>
        /// New informations from Geolocator arrived
        /// </summary>
        /// <param name="sender">Geolocator</param>
        /// <param name="e">Event arguments for new position</param>
        private void MyLocationPositionChanged(Location e)
        {
            Device.BeginInvokeOnMainThread(() => {
                mapView.MyLocationLayer.UpdateMyLocation(new UI.Maui.Position(e.Latitude, e.Longitude));
                if (e.Course != null)
                {
                    mapView.MyLocationLayer.UpdateMyDirection(e.Course.Value, mapView.Viewport.Rotation);
                }

                if (e.Speed != null)
                {
                    mapView.MyLocationLayer.UpdateMySpeed(e.Speed.Value);
                }
            });
        }

    }
}