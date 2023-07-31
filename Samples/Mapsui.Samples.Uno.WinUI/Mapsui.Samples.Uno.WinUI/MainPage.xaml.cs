﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.IO;
using System.Linq;
using Windows.Storage;
using Mapsui.Extensions;
using Mapsui.Providers.Wms;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.CustomWidget;
using Mapsui.Tiling;
using Mapsui.UI.WinUI;
using Mapsui.Samples.Common.Utilities;
using Mapsui.UI;
using RadioButton = Microsoft.UI.Xaml.Controls.RadioButton;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mapsui.Samples.Uno.WinUI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    private Func<object?, EventArgs, bool>? _clicker;

    static MainPage()
    {
        // todo: find proper way to load assembly
        Mapsui.Tests.Common.Utilities.LoadAssembly();
    }

    public MainPage()
    {
        InitializeComponent();

        MapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
        MapControl.Map.Navigator.RotationLock = false;
        MapControl.UnSnapRotationDegrees = 30;
        MapControl.ReSnapRotationDegrees = 5;
        MapControl.Renderer.WidgetRenders[typeof(CustomWidget.CustomWidget)] = new CustomWidgetSkiaRenderer();
        MapControl.SingleTap += MapControl_SingleTap;

        CategoryComboBox.SelectionChanged += CategoryComboBoxSelectionChanged;

        FillComboBoxWithCategories();
        FillListWithSamples();
    }

    private void MapControl_SingleTap(object? sender, UI.TappedEventArgs e)
    {
        e.Handled = _clicker?.Invoke(sender as IMapControl, e) ?? false;
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
        var samples = AllSamples.GetSamples()?.Where(s => s.Category == selectedCategory);
        if (samples == null)
            return;

        foreach (var sample in samples)
            SampleList.Children.Add(CreateRadioButton(sample));
    }

    private void CategoryComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        FillListWithSamples();
    }

    private UIElement CreateRadioButton(ISampleBase sample)
    {
        var radioButton = new RadioButton
        {
            FontSize = 16,
            Content = sample.Name,
            Margin = new Thickness(4)
        };

        radioButton.Click += (s, a) =>
        {
            Catch.Exceptions(async () =>
            {
                MapControl.Map!.Layers.Clear();
                MapControl.Info -= MapOnInfo;
                await sample.SetupAsync(MapControl);
                MapControl.Info += MapOnInfo;
                MapControl.Refresh();

                _clicker = null;
                if (sample is IMapViewSample mapViewSample)
                    _clicker = mapViewSample.OnClick;
            });
        };

        return radioButton;
    }

    private void RotationSlider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        var percent = RotationSlider.Value / (RotationSlider.Maximum - RotationSlider.Minimum);
        MapControl.Map.Navigator.RotateTo(percent * 360);
    }
}
