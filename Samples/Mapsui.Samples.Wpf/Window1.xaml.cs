using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Mapsui.Logging;
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
            MapControl.MouseInfoUp += MapControlOnMouseInfoUp;

            Fps.SetBinding(TextBlock.TextProperty, new Binding("Fps"));
            Fps.DataContext = MapControl.FpsCounter;

            OsmClick(this, null);

            Logger.LogDelegate += LogMethod;
        }

        private void LogMethod(LogLevel logLevel, string s, Exception exception)
        {
            Dispatcher.Invoke(() => LogTextBox.Text = $"{logLevel} {s}");
        }

        private static void MapControlFeatureInfo(object sender, FeatureInfoEventArgs e)
        {
            MessageBox.Show(FeaturesToString(e.FeatureInfo));
        }

        private static string FeaturesToString(IEnumerable<KeyValuePair<string, IEnumerable<IFeature>>> featureInfos)
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
            LogTextBox.Clear(); // Should be a list of messages
            LogTextBox.AppendText(MapControl.ErrorMessage + "\n");
        }

        private void RotationSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var percent = RotationSlider.Value / (RotationSlider.Maximum - RotationSlider.Minimum);
            MapControl.Map.Viewport.Rotation = percent * 360;
            MapControl.Refresh();
        }

        private static void MapControlOnMouseInfoUp(object sender, MouseInfoEventArgs mouseInfoEventArgs)
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
            MapControl.Map = OsmSample.CreateMap();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void ProjectedPointClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map = ProjectionSample.CreateMap();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void AnimatedPointsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map = AnimatedPointsSample.CreateMap();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void RandomPointWithStackLabelClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map = StackedLabelsSample.CreateMap();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void RandomPointsWithFeatureInfoClick(object server, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map = InfoLayersSample.CreateMap();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }
        
        private void GeodanWmsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map = TiledWmsSample.CreateMap();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void GeodanTmsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map = TmsSample.CreateMap();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void BingMapsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map = BingSample.CreateMap();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void GeodanWmscClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map = WmscSample.CreateMap();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void ShapefileClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map = ShapefileSample.CreateMap();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void MapTilerClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map = MapTilerSample.CreateMap();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void PointSymbolsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map = SymbolsInWorldUnitsSample.CreateMap();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void WmsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map = WmsSample.CreateMap();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope(); 
            MapControl.Refresh();
        }

        private void WmtsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map = WmtsSample.CreateMap();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void PointsWithLabelsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map = LabelsSample.CreateMap();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }

        private void RasterizingLabelWithPointsClick(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Clear();
            MapControl.Map = RasterizingLayerSample.CreateMap();
            LayerList.Initialize(MapControl.Map.Layers);
            MapControl.ZoomToFullEnvelope();
            MapControl.Refresh();
        }
    }
}