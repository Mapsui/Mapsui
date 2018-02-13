using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Mapsui.UI.Forms;

namespace Mapsui.Sample.Forms
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
        int markerNum = 1;
        Random rnd = new Random();

        public MapPage ()
		{
			InitializeComponent ();
		}

        public MapPage(Func<Map> call)
        {
            InitializeComponent();

            mapView.AllowPinchRotation = true;
            mapView.UnSnapRotationDegrees = 30;
            mapView.ReSnapRotationDegrees = 5;

            mapView.PinClicked += OnPinClicked;
            mapView.MapClicked += OnMapClicked;

            mapView.Map = call();
        }

        private void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            //var assembly = typeof(MainPageLarge).GetTypeInfo().Assembly;
            //var image = assembly.GetManifestResourceStream("").ToBytes();
            mapView.Pins.Add(new Pin { Label = $"Marker {markerNum++}", Position = e.Point, Type = PinType.Pin, Color = new Color(rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0) });
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