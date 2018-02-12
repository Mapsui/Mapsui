using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapsui.UI;
using Mapsui;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Mapsui.Sample.Forms
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
		public MapPage ()
		{
			InitializeComponent ();
		}

        public MapPage(Func<Map> call)
        {
            InitializeComponent();

            //mapView.ErrorMessageChanged += MapErrorMessageChanged;
            //mapView.FeatureInfo += MapControlFeatureInfo;
            //mapView.TouchMoved += MapControlOnMouseMove;

            mapView.SingleTap += OnSingleTapped;

            mapView.AllowPinchRotation = true;
            mapView.UnSnapRotationDegrees = 30;
            mapView.ReSnapRotationDegrees = 5;

            mapView.Map = call();

            mapView.Map.Info += OnInfo;
        }

        private void OnInfo(object sender, InfoEventArgs e)
        {
            if (e.Feature != null)
                DisplayAlert("Feature tapped", e.Feature.Geometry.AsText(), "Ok");

            e.Handled = true;
        }

        private void OnSingleTapped(object sender, TapEventArgs e)
        {
            if (mapView.Map != null)
                e.Handled = mapView.Map.InvokeInfo(e.Location, e.Location, mapView.SkiaScale, mapView.SymbolCache, null, 1);
        }
    }
}