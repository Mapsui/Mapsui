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
using Microsoft.Phone.Controls;
using SharpMap.Layers;
using BruTile.Web;

namespace Mapsui.WindowsPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.map.Map.Layers.Add(new TileLayer(new OsmTileSource()));
            
            App.Current.Host.Content.Resized += Content_Resized;
        }

        void Content_FullScreenChanged(object sender, EventArgs e)
        {
            Width = App.Current.Host.Content.ActualWidth;
            Height = App.Current.Host.Content.ActualHeight;
        }

        void Content_Resized(object sender, EventArgs e)
        {
            Width = App.Current.Host.Content.ActualWidth;
            Height = App.Current.Host.Content.ActualHeight;
        }
    }
}