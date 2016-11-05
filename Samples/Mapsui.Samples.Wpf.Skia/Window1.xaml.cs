using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Rendering.Skia.UI;
using Mapsui.Samples.Common.Desktop;

namespace Mapsui.Samples.Wpf.Skia
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

            Logger.LogDelegate += LogMethod;

            FillComboBoxWithDemoSamples();

            SampleSet.SelectionChanged += SampleSet_SelectionChanged;

            var firstRadioButton = (RadioButton) SampleList.Children[0];
            firstRadioButton.IsChecked = true;
            firstRadioButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void FillComboBoxWithDemoSamples()
        {
            SampleList.Children.Clear();
            foreach (var sample in AllSamples().ToList())
                SampleList.Children.Add(CreateRadioButton(sample));
        }

        private void FillComboBoxWithTestSamples()
        {
            SampleList.Children.Clear();
            foreach (var sample in TestSamples().ToList())
                SampleList.Children.Add(CreateRadioButton(sample));
        }

        private Dictionary<string, Func<Map>> TestSamples()
        {
            var result = new Dictionary<string, Func<Map>>();
            var i = 0;
            foreach (var sample in Tests.Common.AllSamples.CreateList())
            {
                result[i.ToString()] = sample;
                i++;
            }
            return result;
        }

        private void SampleSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedValue = ((ComboBoxItem) ((ComboBox) sender).SelectedItem).Content.ToString();

            if (selectedValue == "Demo samples")
                FillComboBoxWithDemoSamples();
            else if (selectedValue == "Test samples")
                FillComboBoxWithTestSamples();
            else
                throw new Exception("Unknown ComboBox item");
        }

        private static Dictionary<string, Func<Map>> AllSamples()
        {
            var allSamples = Common.AllSamples.CreateList();
            // Append samples from Mapsui.Desktop
            allSamples["Shapefile"] = ShapefileSample.CreateMap;
            allSamples["MapTiler (tiles on disk)"] = MapTilerSample.CreateMap;
            allSamples["WMS"] = WmsSample.CreateMap;
            return allSamples;
        }

        private UIElement CreateRadioButton(KeyValuePair<string, Func<Map>> sample)
        {
            var radioButton = new RadioButton
            {
                FontSize = 16,
                Content = sample.Key,
                Margin = new Thickness(4)
            };

            radioButton.Click += (s, a) =>
            {
                MapControl.Map.Layers.Clear();
                MapControl.Map = sample.Value();
                LayerList.Initialize(MapControl.Map.Layers);
                MapControl.Refresh();
            };
            return radioButton;
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
                        result += field + ":" + feature[field] + ".";
                    result += "\n";
                }
                result += "\n";
            }
            return result;
        }

        private void MapErrorMessageChanged(object sender, EventArgs e)
        {
            LogTextBox.Text = MapControl.ErrorMessage;
        }

        private void RotationSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var percent = RotationSlider.Value/(RotationSlider.Maximum - RotationSlider.Minimum);
            MapControl.Map.Viewport.Rotation = percent*360;
            MapControl.Refresh();
        }

        private static void MapControlOnMouseInfoUp(object sender, MouseInfoEventArgs mouseInfoEventArgs)
        {
            if (mouseInfoEventArgs.Feature != null)
                MessageBox.Show(mouseInfoEventArgs.Feature["Label"].ToString());
        }
    }
}