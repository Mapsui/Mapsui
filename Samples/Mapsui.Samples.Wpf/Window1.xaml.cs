using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Mapsui.Logging;
using Mapsui.Samples.CustomWidget;
using Mapsui.Samples.Wpf.Utilities;
using Mapsui.UI;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Desktop;

namespace Mapsui.Samples.Wpf
{
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();
            MapControl.FeatureInfo += MapControlFeatureInfo;
            MapControl.MouseMove += MapControlOnMouseMove;
            MapControl.Map.RotationLock = false;
            MapControl.UnSnapRotationDegrees = 30;
            MapControl.ReSnapRotationDegrees = 5;
            MapControl.Renderer.WidgetRenders[typeof(CustomWidget.CustomWidget)] = new CustomWidgetSkiaRenderer();

            Logger.LogDelegate += LogMethod;

            CategoryComboBox.SelectionChanged += CategoryComboBoxSelectionChanged;
            RenderMode.SelectionChanged += RenderModeOnSelectionChanged;

            FillComboBoxWithCategories();
            FillListWithSamples();
        }
        
        private void RenderModeOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var selectedValue = ((ComboBoxItem)((ComboBox)sender).SelectedItem).Content.ToString();

            if (selectedValue.ToLower().Contains("wpf"))
                MapControl.RenderMode = UI.Wpf.RenderMode.Wpf;
            else if (selectedValue.ToLower().Contains("skia"))
                MapControl.RenderMode = UI.Wpf.RenderMode.Skia;
            else
                throw new Exception("Unknown ComboBox item");
        }

        private void MapControlOnMouseMove(object sender, MouseEventArgs e)
        {
            var screenPosition = e.GetPosition(MapControl);
            var worldPosition = MapControl.Viewport.ScreenToWorld(screenPosition.X, screenPosition.Y);
            MouseCoordinates.Text = $"{worldPosition.X:F0}, {worldPosition.Y:F0}";
        }

        private void FillListWithSamples()
        {
            var selectedCategory = CategoryComboBox.SelectedValue?.ToString() ?? "";
            SampleList.Children.Clear();
            foreach (var sample in AllSamples.GetSamples().Where(s => s.Category == selectedCategory))
                SampleList.Children.Add(CreateRadioButton(sample));

            var firstRadioButton = (RadioButton)SampleList.Children[0];
            firstRadioButton.IsChecked = true;
            firstRadioButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void CategoryComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillListWithSamples();
        }
        
        private void FillComboBoxWithCategories()
        {
            // todo: find proper way to load assembly
            WmsSample.LoadAssembly();
            Tests.Common.Utilities.LoadAssembly();

            var categories = AllSamples.GetSamples().Select(s => s.Category).Distinct().OrderBy(c => c);
            foreach (var category in categories)
            {
                CategoryComboBox.Items.Add(category);
            }

            CategoryComboBox.SelectedIndex = 1;
        }

        private UIElement CreateRadioButton(ISample sample)
        {
            var radioButton = new RadioButton
            {
                FontSize = 16,
                Content = sample.Name,
                Margin = new Thickness(4)
            };

            radioButton.Click += (s, a) =>
            {
                MapControl.Map.Layers.Clear();

                sample.Setup(MapControl);

                MapControl.Info += MapControlOnInfo;
                LayerList.Initialize(MapControl.Map.Layers);
            };
            return radioButton;
        }

        readonly LimitedQueue<LogModel> _logMessage = new LimitedQueue<LogModel>(6);

        private void LogMethod(LogLevel logLevel, string message, Exception exception)
        {
            _logMessage.Enqueue(new LogModel { Exception = exception, LogLevel = logLevel, Message = message });
            Dispatcher.Invoke(() => LogTextBox.Text = ToMultiLineString(_logMessage));
        }

        private string ToMultiLineString(LimitedQueue<LogModel> logMessages)
        {
            var result = new StringBuilder();

            var copy = logMessages.ToList();
            foreach (var logMessage in copy)
            {
                if (logMessage == null) continue;
                result.Append($"{logMessage.LogLevel} {logMessage.Message}{Environment.NewLine}");
            }

            return result.ToString();
        }

        private static void MapControlFeatureInfo(object sender, FeatureInfoEventArgs e)
        {
            MessageBox.Show(e.FeatureInfo.ToDisplayText());
        }
        
        private void RotationSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var percent = RotationSlider.Value / (RotationSlider.Maximum - RotationSlider.Minimum);
            MapControl.Navigator.RotateTo(percent * 360);
            MapControl.Refresh();
        }

        private void MapControlOnInfo(object sender, MapInfoEventArgs args)
        {
            if (args.MapInfo.Feature != null)
            {
                FeatureInfoBorder.Visibility = Visibility.Visible;
                FeatureInfo.Text = $"Click Info:{Environment.NewLine}{args.MapInfo.Feature.ToDisplayText()}";
            }
            else
            {
                FeatureInfoBorder.Visibility = Visibility.Collapsed;
            }

        }
    }
}