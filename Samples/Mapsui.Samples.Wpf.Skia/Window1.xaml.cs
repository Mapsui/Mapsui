using System;
using System.Collections.Generic;
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
            MainMapControl.ErrorMessageChanged += MapErrorMessageChanged;
            MainMapControl.FeatureInfo += MapControlFeatureInfo;
            MainMapControl.MouseInfoUp += MapControlOnMouseInfoUp;

            Fps.SetBinding(TextBlock.TextProperty, new Binding("Fps"));
            Fps.DataContext = MainMapControl.FpsCounter;

            Logger.LogDelegate += LogMethod;

            foreach (var sample in InitializeSampleList1())
            {
                SampleList.Children.Add(CreateRadioButton(sample));
            }

            var firstRadioButton = (RadioButton) SampleList.Children[0];
            firstRadioButton.IsChecked = true;
            firstRadioButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        public static Dictionary<string, Func<Map>> AllSamples()
        { 
            var allSamples = Common.AllSamples.CreateList();
            // Append samples from Mapsui.Desktop
            allSamples["Shapefile"] = ShapefileSample.CreateMap;
            allSamples["MapTiler (tiles on disk)"] = MapTilerSample.CreateMap;
            allSamples["WMS"] = WmsSample.CreateMap;
            return allSamples;
        }

        private Dictionary<string, Func<Map>> InitializeSampleList1()
        {
            var result = new Dictionary<string, Func<Map>>();
            var i = 0;
            foreach (var sample in Mapsui.Tests.Common.AllSamples.CreateList())
            {
                result[i.ToString()] = sample;
                i++;
            }
            return result;
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
                MainMapControl.Map.Layers.Clear();
                MainMapControl.Map = sample.Value();
                LayerList.Initialize(MainMapControl.Map.Layers);
                MainMapControl.ZoomToFullEnvelope();
                MainMapControl.Refresh();
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
            LogTextBox.AppendText(MainMapControl.ErrorMessage + "\n");
        }

        private void RotationSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var percent = RotationSlider.Value / (RotationSlider.Maximum - RotationSlider.Minimum);
            MainMapControl.Map.Viewport.Rotation = percent * 360;
            MainMapControl.Refresh();
        }

        private static void MapControlOnMouseInfoUp(object sender, MouseInfoEventArgs mouseInfoEventArgs)
        {
            if (mouseInfoEventArgs.Feature != null)
            {
                MessageBox.Show(mouseInfoEventArgs.Feature["Label"].ToString());
            }
        }
    }
}