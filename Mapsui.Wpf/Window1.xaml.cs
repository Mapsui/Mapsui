using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using BruTile.Web;
using BruTile.Web.TmsService;
using DemoConfig;
using SharpMap;
using SharpMap.Layers;
using SharpMap.Samples;

namespace Mapsui.Wpf
{
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            mapControl.ErrorMessageChanged += MapErrorMessageChanged;

            fps.SetBinding(TextBlock.TextProperty, new Binding("Fps"));
            fps.DataContext = mapControl.FpsCounter;
            OsmClick(this, null);
        }

        #region Switching layers
        
        private void OsmClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map = CreateMap(new TileLayer(new OsmTileSource()) { LayerName = "OSM" });
            layerList.Initialize(mapControl.Map.Layers);
        }

        private void GeodanWmsClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map = CreateMap(new TileLayer(new GeodanWorldWmsTileSource()));
            layerList.Initialize(mapControl.Map.Layers);
        }

        private void GeodanTmsClick(object sender, RoutedEventArgs e)
        {
            var client = new WebClient();
            client.OpenReadCompleted += LoadTmsLayer;
            client.OpenReadAsync(new Uri("http://geoserver.nl/tiles/tilecache.aspx/1.0.0/worlddark_GM"));
        }

        private void BingMapsClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map = CreateMap(new TileLayer(new BingTileSource(BingRequest.UrlBingStaging, String.Empty, BingMapType.Aerial)));
            layerList.Initialize(mapControl.Map.Layers);
        }

        private void GeodanWmscClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map = CreateMap(new TileLayer(new GeodanWorldWmsCTileSource()));
            layerList.Initialize(mapControl.Map.Layers);
        }

        private void GroupTileLayerClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map = CreateMap(CreateGroupLayer());
            layerList.Initialize(mapControl.Map.Layers);
        }

        private void SharpMapClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map = CreateMap(ShapefileSample.CreateCountryLayer());
            layerList.Initialize(mapControl.Map.Layers);
        }

        private void MapTilerClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map = CreateMap(new TileLayer(new MapTilerTileSource()));
            layerList.Initialize(mapControl.Map.Layers);
        }

        #endregion

        private static ILayer CreateGroupLayer()
        {
            var osmLayer = new TileLayer(new OsmTileSource()) { LayerName = "OSM" };
            var wmsLayer = new TileLayer(new GeodanWorldWmsTileSource()) { LayerName = "Geodan WMS" };
            var groupLayer = new GroupTileLayer(new List<TileLayer> { osmLayer, wmsLayer });
            return groupLayer;
        }

        private void LoadTmsLayer(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Cancelled) MessageBox.Show("Request was cancelled");
            else if (e.Error != null) MessageBox.Show("An error occurred: " + e.Error.Message);
            else
            {
                mapControl.Map = CreateMap(new TileLayer(TileMapParser.CreateTileSource(e.Result, 
                    "http://geoserver.nl/tiles/tilecache.aspx/1.0.0/worlddark_GM")));
                layerList.Initialize(mapControl.Map.Layers);
            }
        }

        private static Map CreateMap(ILayer layer)
        {
            var map = new Map();
            map.Layers.Add(layer);
            return map;
        }

        private void MapErrorMessageChanged(object sender, EventArgs e)
        {
            Error.Text = mapControl.ErrorMessage;
            AnimateOpacity(errorBorder, 0.75, 0, 8000);
        }

        public static void AnimateOpacity(UIElement target, double from, double to, int duration)
        {
            target.Opacity = 0;
            var animation = new DoubleAnimation();
            animation.From = from;
            animation.To = to;
            animation.Duration = new TimeSpan(0, 0, 0, 0, duration);

            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));

            var storyBoard = new Storyboard();
            storyBoard.Children.Add(animation);
            storyBoard.Begin();
        }

        private void WmsClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map = WmsSample.InitializeMap();
            layerList.Initialize(mapControl.Map.Layers);
        }

        private void PointSymbolsClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map.Layers.Clear();
            mapControl.Map.Layers.Add(new TileLayer(new OsmTileSource()) {LayerName = "OSM"});
            mapControl.Map.Layers.Add(PointLayerSample.Create());
            mapControl.Map.Layers.Add(PointLayerWithWorldUnitsForSymbolsSample.Create());
            layerList.Initialize(mapControl.Map.Layers);
            mapControl.Refresh();
        }    
    }
}

