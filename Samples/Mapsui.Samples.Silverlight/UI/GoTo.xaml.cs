using System;
using System.Windows;
using System.Windows.Controls;
using Mapsui.Projection;
using Mapsui.Samples.Silverlight;
using System.Net;
using System.Globalization;
using Mapsui.Samples.Silverlight.UI;

namespace Mapsui.Samples.Silverlight
{
	public partial class GoTo : UserControl
	{
        GUIOverlay gui;

		public GoTo()
		{
			InitializeComponent();
            HideGoTo.Completed += HideGoTo_Completed;
		}

        public void SetGui(GUIOverlay gui)
        {
            this.gui = gui;
        }

        void HideGoTo_Completed(object sender, EventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            HideGoTo.Begin();
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            String city = cityBox.Text;
            String country = countryBox.Text;
            String street = streetBox.Text;
            String requestString = "http://tinygeocoder.com/create-api.php?q=";

            if (!street.Equals(""))
                requestString += street;
            if (!street.Equals("") && !city.Equals(""))
                requestString += ", ";
            if (!city.Equals(""))
                requestString += city;
            if (!city.Equals("") && !country.Equals(""))
                requestString += ", ";
            if (!country.Equals(""))
                requestString += country;

            var uri = new Uri(requestString);

            var client = new WebClient();
            client.DownloadStringCompleted += client_DownloadStringCompleted;
            client.DownloadStringAsync(uri);
        }

        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            String result = e.Result;
            if (result.Equals(""))
            {
                MessageBox.Show("Could not find location");
            }
            else
            {
                String[] lonLat = result.Split(',');

                if (lonLat.Length == 2)
                {
                    if (!streetBox.Text.Equals(""))
                        gui._mapControl.Viewport.Resolution = 0.597164283;
                    else if (!cityBox.Text.Equals(""))
                        gui._mapControl.Viewport.Resolution = 9.554628534;
                    else if (!countryBox.Equals(""))
                        gui._mapControl.Viewport.Resolution = 611.496226172;

                    Mapsui.Geometries.Point sphericalLocation = SphericalMercator.FromLonLat(Double.Parse(lonLat[1], CultureInfo.InvariantCulture), Double.Parse(lonLat[0], CultureInfo.InvariantCulture));
                    gui._mapControl.Viewport.Center = sphericalLocation;
                    //Toresolution has to be set somehow
                    gui._mapControl.OnViewChanged();
                    HideGoTo.Begin();
                }
                else
                {
                    MessageBox.Show("Can not use the returned values");
                }
            }
        }
	}
}