using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.IO;
using System.Linq;
using Windows.Storage;
using Mapsui.Extensions;
using Mapsui.Providers;
using Mapsui.Providers.Wms;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.CustomWidget;
using Mapsui.UI;
using Mapsui.Utilities;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mapsui.Samples.WinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Hack to tell the platform independent samples where the files can be found on Android.
            MbTilesSample.MbTilesLocation = MbTilesLocationOnWinUI;
            MbTilesHelper.DeployMbTilesFile(s => File.Create(Path.Combine(MbTilesLocationOnWinUI, s)));

            MapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
            MapControl.Map.RotationLock = false;
            MapControl.UnSnapRotationDegrees = 30;
            MapControl.ReSnapRotationDegrees = 5;
            MapControl.Renderer.WidgetRenders[typeof(CustomWidget.CustomWidget)] = new CustomWidgetSkiaRenderer();

            CategoryComboBox.SelectionChanged += CategoryComboBoxSelectionChanged;

            FillComboBoxWithCategories();
            FillListWithSamples();
        }

        private void FillComboBoxWithCategories()
        {
            var categories = AllSamples.GetSamples().Select(s => s.Category).Distinct().OrderBy(c => c);
            foreach (var category in categories)
            {
                CategoryComboBox.Items?.Add(category);
            }

            CategoryComboBox.SelectedIndex = 1;
        }

        private void MapOnInfo(object sender, MapInfoEventArgs args)
        {
            if (args.MapInfo?.Feature != null)
                FeatureInfo.Text = $"Click Info:{Environment.NewLine}{args.MapInfo.Feature.ToDisplayText()}";
        }

        private void FillListWithSamples()
        {
            var selectedCategory = CategoryComboBox.SelectedValue?.ToString() ?? "";
            SampleList.Children.Clear();
            foreach (var sample in AllSamples.GetSamples().Where(s => s.Category == selectedCategory))
                SampleList.Children.Add(CreateRadioButton(sample));
        }
        
        private void CategoryComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillListWithSamples();
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
                MapControl.Info -= MapOnInfo;
                sample.Setup(MapControl);
                MapControl.Info += MapOnInfo;
                MapControl.Refresh();
            };

            return radioButton;
        }

        private static string MbTilesLocationOnWinUI => ApplicationData.Current.LocalFolder.Path;

        private void RotationSlider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var percent = RotationSlider.Value / (RotationSlider.Maximum - RotationSlider.Minimum);
            MapControl.Navigator.RotateTo(percent * 360);
            MapControl.Refresh();
        }
    }
}
