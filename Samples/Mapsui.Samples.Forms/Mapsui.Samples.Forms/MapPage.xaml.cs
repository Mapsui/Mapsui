using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Mapsui.UI.Forms;

namespace Mapsui.Samples.Forms
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
        int markerNum = 1;
        Random rnd = new Random();
        Func<MapView, MapClickedEventArgs, bool> clicker;

        public MapPage ()
		{
			InitializeComponent ();
		}

        public MapPage(Func<Map> call, Func<MapView, MapClickedEventArgs, bool> c = null)
        {
            InitializeComponent();

            mapView.AllowPinchRotation = true;
            mapView.UnSnapRotationDegrees = 30;
            mapView.ReSnapRotationDegrees = 5;

            mapView.PinClicked += OnPinClicked;
            mapView.MapClicked += OnMapClicked;

            mapView.Map = call();

            clicker = c;
        }

        private void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            e.Handled = (bool)clicker?.Invoke(sender as MapView, e);
            //Samples.SetPins(mapView, e);
            //Samples.DrawPolylines(mapView, e);
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