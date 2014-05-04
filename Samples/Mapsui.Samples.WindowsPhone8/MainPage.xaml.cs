using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using BruTile.Web;
using Mapsui.Layers;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Mapsui.Samples.WindowsPhone8.Resources;

namespace Mapsui.Samples.WindowsPhone8
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();

            Init();
        }
        private void Init()
        {
            //Openstreetmap URI: 
            var osm = new TileLayer(new OsmTileSource()) { LayerName = "OSM" };
            //add layers
            MapControl.Map.Layers.Add(osm);
            MapControl.Refresh();
        }

        private void BtnZoomOutClick(object sender, EventArgs e)
        {
            MapControl.ZoomOut();
        }

        private void BtnZoomInClick(object sender, EventArgs e)
        {
            MapControl.ZoomIn();
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}