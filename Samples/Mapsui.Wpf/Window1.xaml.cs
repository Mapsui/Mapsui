using BruTile.Web;
using Mapsui.Samples.Common;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Mapsui.Samples.Desktop;
using Mapsui.Windows.Layers;

namespace Mapsui.Wpf
{
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            mapControl.ErrorMessageChanged += MapErrorMessageChanged;
            mapControl.FeatureInfo += MapControlFeatureInfo;
            fps.SetBinding(TextBlock.TextProperty, new Binding("Fps"));
            fps.DataContext = mapControl.FpsCounter;

            OsmClick(this, null);
        }

        void MapControlFeatureInfo(object sender, Windows.FeatureInfoEventArgs e)
        {
            MessageBox.Show(FeaturesToString(e.FeatureInfo));
        }

        string FeaturesToString(IEnumerable<KeyValuePair<string, IEnumerable<IFeature>>> featureInfos)
        {
            var result = string.Empty;

            foreach (var layer in featureInfos)
            {
                result += layer.Key + "\n";
                foreach (var feature in layer.Value)
                {
                    foreach (var field in feature.Fields)
                    {
                        result += field + ":" + feature[field] + ".";
                    }
                    result += "\n";
                }
                result += "\n";
            }
            return result;
        }

        private void MapErrorMessageChanged(object sender, EventArgs e)
        {
            Error.Text = mapControl.ErrorMessage;
            Utilities.AnimateOpacity(errorBorder, 0.75, 0, 8000);
        }

        private void OsmClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map.Layers.Clear();
            mapControl.Map.Layers.Add(new TileLayer(new OsmTileSource()) { LayerName = "OSM" });
            var provider = CreateRandomPointsProvider();
            mapControl.Map.Layers.Add(PointLayerSample.CreateRandomPointLayerWithLabel(provider));
            mapControl.Map.Layers.Add(PointLayerSample.CreateStackedLabelLayer(provider));

            layerList.Initialize(mapControl.Map.Layers);
            mapControl.Refresh();
        }

        private MemoryProvider CreateRandomPointsProvider()
        {
            var randomPoints = PointLayerSample.GenerateRandomPoints(mapControl.Map.Envelope, 200);
            var features = new Features();
            var count = 0;
            foreach (var point in randomPoints)
            {
                var feature = new Feature { Geometry = point };
                feature["Label"] = count.ToString();
                features.Add(feature);
                count++;
            }
            return new MemoryProvider(features);
        }
        private void GeodanWmsClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map.Layers.Clear();
            mapControl.Map.Layers.Add(new TileLayer(new GeodanWorldWmsTileSource()));
            layerList.Initialize(mapControl.Map.Layers);
            mapControl.Refresh();
        }

        private void GeodanTmsClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map.Layers.Clear();
            mapControl.Map.Layers.Add(new TileLayer("http://geoserver.nl/tiles/tilecache.aspx/1.0.0/worlddark_GM", 
                true, ex => MessageBox.Show(ex.Message)));
            layerList.Initialize(mapControl.Map.Layers);
            mapControl.Refresh();
        }

        private void BingMapsClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map.Layers.Clear();
            mapControl.Map.Layers.Add(new TileLayer(new BingTileSource(BingRequest.UrlBingStaging, String.Empty, BingMapType.Aerial)));
            layerList.Initialize(mapControl.Map.Layers);
            mapControl.Refresh();
        }

        private void GeodanWmscClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map.Layers.Clear();
            mapControl.Map.Layers.Add(new TileLayer(new GeodanWorldWmsCTileSource()));
            layerList.Initialize(mapControl.Map.Layers);
            mapControl.Refresh();
        }

        private void GroupTileLayerClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map.Layers.Clear();
            mapControl.Map.Layers.Add(CreateGroupLayer());
            layerList.Initialize(mapControl.Map.Layers);
            mapControl.Refresh();
        }

        private void ShapefileClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map = ShapefileSample.CreateMap();
            layerList.Initialize(mapControl.Map.Layers);
            mapControl.Refresh();
        }

        private void MapTilerClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map.Layers.Clear();
            mapControl.Map.Layers.Add(new TileLayer(new MapTilerTileSource()));
            layerList.Initialize(mapControl.Map.Layers);
            mapControl.Refresh();
        }

        private static ILayer CreateGroupLayer()
        {
            var osmLayer = new TileLayer(new OsmTileSource()) { LayerName = "OSM" };
            var wmsLayer = new TileLayer(new GeodanWorldWmsTileSource()) { LayerName = "Geodan WMS" };
            var groupLayer = new GroupTileLayer(new[] { osmLayer, wmsLayer });
            return groupLayer;
        }

        private void PointSymbolsClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map.Layers.Clear();
            mapControl.Map.Layers.Add(new TileLayer(new OsmTileSource()) { LayerName = "OSM" });
            mapControl.Map.Layers.Add(PointLayerSample.Create());
            mapControl.Map.Layers.Add(PointLayerWithWorldUnitsForSymbolsSample.Create());
            layerList.Initialize(mapControl.Map.Layers);
            mapControl.Refresh();
        }

        private void WmsClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map.Layers.Clear();
            mapControl.Map.Layers.Add(WmsSample.Create());
            layerList.Initialize(mapControl.Map.Layers);
            mapControl.Refresh();
        }

        private void ArcGISImageServiceClick(object sender, RoutedEventArgs e)
        {
            mapControl.Map.Layers.Clear();
            mapControl.Map.Layers.Add(ArcGISImageServiceSample.Create());
            layerList.Initialize(mapControl.Map.Layers);
            mapControl.Refresh();
        }
    }
}

