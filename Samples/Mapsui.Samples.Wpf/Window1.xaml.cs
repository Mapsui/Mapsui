using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Desktop;
using Mapsui.UI.Xaml;

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

            Logger.LogDelegate += LogMethod;
        }

        private void LogMethod(LogLevel logLevel, string s, Exception exception)
        {
            Dispatcher.Invoke(() => LogTextBox.Text = $"{logLevel} {s}");
        }

        static void MapControlFeatureInfo(object sender, FeatureInfoEventArgs e)
        {
            MessageBox.Show(FeaturesToString(e.FeatureInfo));
        }

        static string FeaturesToString(IEnumerable<KeyValuePair<string, IEnumerable<IFeature>>> featureInfos)
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
            LogTextBox.Clear(); // Should a list of messages
            LogTextBox.AppendText(MapControl.ErrorMessage + "\n");
        }

        private void RotationSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var percent = RotationSlider.Value / (RotationSlider.Maximum - RotationSlider.Minimum);
            MapControl.Map.Viewport.Rotation = percent * 360;
            MapControl.Refresh();
        }
        private static void MapControlOnMouseInfoDown(object sender, MouseInfoEventArgs mouseInfoEventArgs)
        {
            if (mouseInfoEventArgs.Feature != null)
            {
                MessageBox.Show(mouseInfoEventArgs.Feature["Label"].ToString());
            }
        }
        
        // ************************************ start click events ******************************************

        private void OsmClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();

            MapControl.Map.Layers.Add(OsmSample.CreateLayer());

            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void ProjectedPointClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map.Transformation = new MinimalTransformation();
            MapControl.Map.CRS = "EPSG:3857";
            MapControl.Map.Layers.Add(OsmSample.CreateLayer());
            MapControl.Map.Layers.Add(PointsInWgs84Sample.CreateLayer());

            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void AnimatedPointsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map.Layers.Add(OsmSample.CreateLayer());
            MapControl.Map.Layers.Add(AnimatedPointsSample.CreateLayer());

            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void RandomPointWithStackLabelClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();

            MapControl.Map.Layers.Add(OsmSample.CreateLayer());
            var provider = PointsSample.CreateRandomPointsProvider(MapControl.Map.Envelope);
            MapControl.Map.Layers.Add(PointsWithStackedLabelsSample.CreateLayer(provider));
            MapControl.Map.Layers.Add(PointsSample.CreateRandomPointLayer(provider));

            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void RandomPointsWithFeatureInfoClick(object server, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();

            MapControl.Map.Layers.Add(OsmSample.CreateLayer());
            MapControl.Map.Layers.Add(PointsWithFeatureInfoSample.CreateLayer(MapControl.Map.Envelope));
            MapControl.MouseInfoUp += MapControlOnMouseInfoDown;
            MapControl.MouseInfoUpLayers.Add(MapControl.Map.Layers.FindLayer("Points with feature info").First());

            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }
        
        private void GeodanWmsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();

            MapControl.Map.Layers.Add(TiledWmsSample.CreateLayer());

            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void GeodanTmsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();

            MapControl.Map.Layers.Add(TmsSample.CreateLayer());

            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void BingMapsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();

            MapControl.Map.Layers.Add(BingSample.CreateLayer());

            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void GeodanWmscClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();

            MapControl.Map.Layers.Add(WmscSample.CreateLayer());

            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void ShapefileClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();

            foreach (var layer in ShapefileSample.CreateLayers())
            {
                MapControl.Map.Layers.Add(layer);
            }
            
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void MapTilerClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();

            MapControl.Map.Layers.Add(MapTilerSample.CreateLayer());

            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void PointSymbolsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();

            MapControl.Map.Layers.Add(OsmSample.CreateLayer());
            MapControl.Map.Layers.Add(PointsSample.Create());
            MapControl.Map.Layers.Add(PointsWithSymbolsInWorldUnitsSample.CreateLayer());

            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void WmsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();

            MapControl.Map.CRS = "EPSG:28992";
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

            MapControl.Map.Layers.Add(WmtsSample.CreateLayer());
            MapControl.Map.Layers.Add(GeodanOfficesSample.CreateLayer());

            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void PointsWithLabelsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();

            MapControl.Map.Layers.Add(OsmSample.CreateLayer());
            MapControl.Map.Layers.Add(PointsSample.CreatePointLayerWithLabels());

            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void RasterizingLabelWithPointsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();

            MapControl.Map.Layers.Add(OsmSample.CreateLayer());
            var layer = CreatePointLayer();
            var rasterizingLayer = new RasterizingLayer(layer);

            MapControl.Map.Layers.Add(rasterizingLayer);
        }

        private static MemoryLayer CreatePointLayer()
        {
            var provider = new MemoryProvider();
            var rnd = new Random();
            for (var i = 0; i < 100; i++)
            {
                var feature = new Feature
                {
                    Geometry = new Geometries.Point(rnd.Next(100000, 5000000), rnd.Next(100000, 5000000))
                };
                provider.Features.Add(feature);
            }
            var layer = new MemoryLayer {DataSource = provider};
            return layer;
        }
    }
}

