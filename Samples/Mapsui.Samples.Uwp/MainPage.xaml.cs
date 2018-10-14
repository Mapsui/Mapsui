using System;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Mapsui.UI;
using Mapsui.Utilities;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Samples.Common.Maps;

namespace Mapsui.Samples.Uwp
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            // Hack to tell the platform independent samples where the files can be found on Android.
            MbTilesSample.MbTilesLocation = MbTilesLocationOnUwp;
            MbTilesHelper.DeployMbTilesFile(s => File.Create(Path.Combine(MbTilesLocationOnUwp, s)));

            MapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
            MapControl.RotationLock = false;
            MapControl.UnSnapRotationDegrees = 30;
            MapControl.ReSnapRotationDegrees = 5;

            FillComboBoxWithDemoSamples();

            SampleSet.SelectionChanged += SampleSet_SelectionChanged;
        }

        private void MapOnInfo(object sender, MapInfoEventArgs args)
        {
            if (args.MapInfo.Feature != null)
                FeatureInfo.Text = $"Click Info:{Environment.NewLine}{args.MapInfo.Feature.ToDisplayText()}";
        }

        private void FillComboBoxWithDemoSamples()
        {
            SampleList.Children.Clear();
            foreach (var sample in AllSamples.GetSamples())
                SampleList.Children.Add(CreateRadioButton(sample));
        }

        private void FillComboBoxWithTestSamples()
        {
            SampleList.Children.Clear();
            foreach (var sample in Mapsui.Tests.Common.AllSamples.GetSamples().ToList())
                SampleList.Children.Add(CreateRadioButton(sample));
        }
        
        private void SampleSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBoxItem = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            if (comboBoxItem?.Content != null)
            {
                var selectedValue = comboBoxItem.Content.ToString();

                if (selectedValue == "Demo samples")
                {
                    FillComboBoxWithDemoSamples();
                }
                else if (selectedValue == "Test samples")
                {
                    FillComboBoxWithTestSamples();
                }
                else
                {
                    throw new Exception("Unknown ComboBox item");
                }
            }
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
                sample.Setup(MapControl);
                MapControl.Map.Layers.Clear();
                MapControl.Info -= MapOnInfo;
                sample.Setup(MapControl);
                MapControl.Info += MapOnInfo;
                MapControl.Refresh();
            };

            return radioButton;
        }

        private static string MbTilesLocationOnUwp => ApplicationData.Current.LocalFolder.Path;
    }
}
