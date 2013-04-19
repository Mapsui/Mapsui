using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using BruTile.Web;
using Mapsui.Layers;
using Microsoft.Phone.Controls;

namespace Mapsui.Samples.WP7
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

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
    }
}