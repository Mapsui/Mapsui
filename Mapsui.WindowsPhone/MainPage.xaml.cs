using System;
using Microsoft.Phone.Controls;
using BruTile.Web;
using SharpMap.Layers;

namespace Mapsui.WindowsPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
            map.Map.Layers.Add(new TileLayer(new OsmTileSource()));
            
            App.Current.Host.Content.Resized += Content_Resized;
        }

        void ContentFullScreenChanged(object sender, EventArgs e)
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