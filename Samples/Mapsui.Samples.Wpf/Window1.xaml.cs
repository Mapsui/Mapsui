using System.Threading;
using BruTile;
using BruTile.Extensions;
using BruTile.Web;
using BruTile.Wmts;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Samples.Desktop;
using Mapsui.Styles;
using Mapsui.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Layer = Mapsui.Layers.Layer;

namespace Mapsui.Samples.Wpf
{
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();
            MapControl.ErrorMessageChanged += MapErrorMessageChanged;
            MapControl.FeatureInfo += MapControlFeatureInfo;
            Fps.SetBinding(TextBlock.TextProperty, new Binding("Fps"));
            Fps.DataContext = MapControl.FpsCounter;

            OsmClick(this, null);
        }

        void MapControlFeatureInfo(object sender, FeatureInfoEventArgs e)
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
            Error.Text = MapControl.ErrorMessage;
            Utilities.AnimateOpacity(ErrorBorder, 0.75, 0, 8000);
        }

        private void OsmClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map.Layers.Add(new TileLayer(new OsmTileSource()) { LayerName = "OSM" });

            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void AnimatedPoints(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map.Layers.Add(new TileLayer(new OsmTileSource()) { LayerName = "OSM" });
            MapControl.Map.Layers.Add(new AnimatedPointsWithAutoUpdateLayer { LayerName = "AnimatedLayer" });

            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void RandomPointWithStackLabel(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map.Layers.Add(new TileLayer(new OsmTileSource()) { LayerName = "OSM" });
            var provider = CreateRandomPointsProvider();
            MapControl.Map.Layers.Add(PointLayerSample.CreateStackedLabelLayer(provider));
            MapControl.Map.Layers.Add(PointLayerSample.CreateRandomPointLayer(provider));
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void RandomPointsWithFeatureInfo(object server, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map.Layers.Add(new TileLayer(new OsmTileSource()) { LayerName = "OSM" });
            var pointLayer = PointLayerSample.CreateRandomPointLayer(CreateRandomPointsProvider());
            pointLayer.Style = new StyleCollection {
                new SymbolStyle {
                        SymbolScale = 1, Fill = new Brush(Color.Cyan), 
                        Outline = { Color = Color.White, Width = 2}}
                };
            MapControl.Map.Layers.Add(pointLayer);
            MapControl.MouseInfoUp += MapControlOnMouseInfoDown;
            MapControl.MouseInfoUpLayers.Add(pointLayer);
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void MapControlOnMouseInfoDown(object sender, MouseInfoEventArgs mouseInfoEventArgs)
        {
            if (mouseInfoEventArgs.Feature != null)
            {
                MessageBox.Show(mouseInfoEventArgs.Feature["Label"].ToString());
            }
        }

        private MemoryProvider CreateRandomPointsProvider()
        {
            var randomPoints = PointLayerSample.GenerateRandomPoints(MapControl.Map.Envelope, 100);
            var features = new Features();
            var count = 0;
            foreach (var point in randomPoints)
            {
                var feature = new Feature { Geometry = point };
                feature["Label"] = count.ToString(CultureInfo.InvariantCulture);
                features.Add(feature);
                count++;
            }
            return new MemoryProvider(features);
        }

        private void GeodanWmsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map.Layers.Add(new TileLayer(new GeodanWorldWmsTileSource()));
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void GeodanTmsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map.Layers.Add(new TileLayer("http://geoserver.nl/tiles/tilecache.aspx/1.0.0/worlddark_GM",
                true, ex => MessageBox.Show(ex.Message)));
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.Refresh();
        }

        private void BingMapsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map.Layers.Add(new TileLayer(new BingTileSource(BingRequest.UrlBingStaging, String.Empty, BingMapType.Aerial)));
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void GeodanWmscClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map.Layers.Add(new TileLayer(new GeodanWorldWmsCTileSource()));
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.Refresh();
        }

        private void ShapefileClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map = ShapefileSample.CreateMap();
            MapControl.ZoomToFullEnvelope();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.Refresh();
        }

        private void MapTilerClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map.Layers.Add(new TileLayer(new MapTilerTileSource()));
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void PointSymbolsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map.Layers.Add(new TileLayer(new OsmTileSource()) { LayerName = "OSM" });
            MapControl.Map.Layers.Add(PointLayerSample.Create());
            MapControl.Map.Layers.Add(CreatePointLayerWithWorldUnitSymbol());
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.Refresh();
        }

        private static ILayer CreatePointLayerWithWorldUnitSymbol()
        {
            return new Layer("PointLayer WorldUnits")
                {
                    DataSource = PointLayerWithWorldUnitsForSymbolsSample.Create()
                };
        }

        private void WmsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map.Layers.Add(WmsSample.Create());
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void ArcGISImageServiceClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map.Layers.Add(ArcGISImageServiceSample.CreateLayer());
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void WmtsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            var webRequest = (HttpWebRequest)WebRequest.Create("http://geodata.nationaalgeoregister.nl/wmts/top10nl?VERSION=1.0.0&request=GetCapabilities");
            WebResponse webResponse = webRequest.GetSyncResponse(10000);
            if (webResponse == null) throw (new WebException("An error occurred while fetching tile", null));
            using (var responseStream = webResponse.GetResponseStream())
            {
                var tileSources = WmtsParser.Parse(responseStream);
                var natura2000 = tileSources.First(t => t.Title.ToLower().Contains("natura2000"));
                MapControl.Map.Layers.Add(new TileLayer(natura2000));
                MapControl.ZoomToFullEnvelope();
                MapControl.Refresh();
            }
        }
    }
}

