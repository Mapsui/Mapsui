using System;
using System.Windows;
using System.Windows.Controls;
using Mapsui.Silverlight;
using System.Net;
using System.Globalization;

namespace Mapsui.Silverlight
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
                        gui.mapControl.Viewport.Resolution = 0.597164283;
                    else if (!cityBox.Text.Equals(""))
                        gui.mapControl.Viewport.Resolution = 9.554628534;
                    else if (!countryBox.Equals(""))
                        gui.mapControl.Viewport.Resolution = 611.496226172;

                    Mapsui.Geometries.Point sphericalLocation = SphericalMercator.FromLonLat(Double.Parse(lonLat[1], CultureInfo.InvariantCulture), Double.Parse(lonLat[0], CultureInfo.InvariantCulture));
                    gui.mapControl.Viewport.Center = sphericalLocation;
                    //Toresolution has to be set somehow
                    gui.mapControl.OnViewChanged(true);
                    HideGoTo.Begin();
                }
                else
                {
                    MessageBox.Show("Can not use the returned values");
                }
            }
        }
	}

    public class SphericalMercator
    {
        private readonly static double radius = 6378137;
        private static double D2R = Math.PI / 180;
        private static double HALF_PI = Math.PI / 2;

        public static Mapsui.Geometries.Point FromLonLat(double lon, double lat)
        {
            double lonRadians = (D2R * lon);
            double latRadians = (D2R * lat);

            double x = radius * lonRadians;
            double y = radius * Math.Log(Math.Tan(Math.PI * 0.25 + latRadians * 0.5));

            return new Mapsui.Geometries.Point((float)x, (float)y);
        }

        public static Mapsui.Geometries.Point ToLonLat(double x, double y)
        {
            double ts;
            ts = Math.Exp(-y / (radius));
            double latRadians = HALF_PI - 2 * Math.Atan(ts);

            double lonRadians = x / (radius);

            double lon = (lonRadians / D2R);
            double lat = (latRadians / D2R);

            return new Mapsui.Geometries.Point((float)lon, (float)lat);
        }
    }
}