using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI.Forms;
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

            mapView.AllowPinchRotation = true;
            mapView.UnSnapRotationDegrees = 30;
            mapView.ReSnapRotationDegrees = 5;

            mapView.PinClicked += OnPinClicked;
            mapView.MapClicked += OnMapClicked;
        }

        private void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            e.Handled = (bool)clicker?.Invoke(sender as MapView, e);
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
            }

            e.Handled = true;
        }
    }
}