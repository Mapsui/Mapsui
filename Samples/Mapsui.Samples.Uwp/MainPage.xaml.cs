﻿using System;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Mapsui.Extensions;
using Mapsui.Layers.Tiling;
using Mapsui.Providers.Wms;
using Mapsui.UI;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.CustomWidget;

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

            MapControl.Map!.Layers.Add(OpenStreetMap.CreateTileLayer());
            MapControl.Map!.RotationLock = false;
            MapControl.UnSnapRotationDegrees = 30;
            MapControl.ReSnapRotationDegrees = 5;
            MapControl.Renderer.WidgetRenders[typeof(CustomWidget.CustomWidget)] = new CustomWidgetSkiaRenderer();

            CategoryComboBox.SelectionChanged += CategoryComboBoxSelectionChanged;

            FillComboBoxWithCategories();
            FillListWithSamples();
        }

        private void FillComboBoxWithCategories()
        {
            var categories = AllSamples.GetSamples()?.Select(s => s.Category).Distinct().OrderBy(c => c);

            if (categories == null)
                return;

            foreach (var category in categories)
            {
                CategoryComboBox.Items?.Add(category);
            }

            CategoryComboBox.SelectedIndex = 1;
        }

        private void MapOnInfo(object? sender, MapInfoEventArgs args)
        {
            if (args.MapInfo?.Feature != null)
                FeatureInfo.Text = $"Click Info:{Environment.NewLine}{args.MapInfo.Feature.ToDisplayText()}";
        }

        private void FillListWithSamples()
        {
            var selectedCategory = CategoryComboBox.SelectedValue?.ToString() ?? "";
            SampleList.Children.Clear();
            var enumerable = AllSamples.GetSamples()?.Where(s => s.Category == selectedCategory);
            if (enumerable == null)
                return;

            foreach (var sample in enumerable)
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

            radioButton.Click += (_, _) => {
                MapControl.Map?.Layers.Clear();
                MapControl.Info -= MapOnInfo;
                sample.Setup(MapControl);
                MapControl.Info += MapOnInfo;
                MapControl.Refresh();
            };

            return radioButton;
        }

        private static string MbTilesLocationOnUwp => ApplicationData.Current.LocalFolder.Path;

        private void RotationSlider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var percent = RotationSlider.Value / (RotationSlider.Maximum - RotationSlider.Minimum);
            MapControl.Navigator.RotateTo(percent * 360);
            MapControl.Refresh();
        }
    }
}
