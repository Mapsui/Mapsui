using Mapsui.Samples.Common;
using Mapsui.UI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Mapsui.Sample.Forms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPageLarge : ContentPage
    {
        Dictionary<string, Func<Map>> allSamples;

        public MainPageLarge()
        {
            InitializeComponent();

            allSamples = AllSamples.CreateList();

            listView.ItemsSource = allSamples.Select(k => k.Key).ToList();

            mapView.AllowPinchRotation = true;
            mapView.UnSnapRotationDegrees = 30;
            mapView.ReSnapRotationDegrees = 5;

            mapView.PinClicked += OnPinClicked;
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
            mapView.Pins.Add(new Pin { Label = "Test1", Position = new Position(48, 9) });
            mapView.Pins.Add(new Pin { Label = "Test2", Position = new Position(50, 0) });
        }

        private void OnPinClicked(object sender, PinClickedEventArgs e)
        {
            if (e.Pin != null)
                DisplayAlert($"Pin {e.Pin.Label}", $"Is at position {e.Pin.Position}", "Ok");

            e.Handled = true;
        }
    }
}