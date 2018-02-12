using Mapsui.Samples.Common;
using Mapsui.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            mapView.SingleTap += OnSingleTapped;

            mapView.AllowPinchRotation = true;
            mapView.UnSnapRotationDegrees = 30;
            mapView.ReSnapRotationDegrees = 5;
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