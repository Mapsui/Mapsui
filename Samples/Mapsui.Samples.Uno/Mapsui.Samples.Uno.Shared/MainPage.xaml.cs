using System;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Mapsui.Extensions;
using Mapsui.Providers.Wms;
using Mapsui.UI;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.CustomWidget;
using Mapsui.Tiling;

namespace Mapsui.Samples.Uwp;

// ReSharper disable once RedundantExtendsListEntry
public sealed partial class MainPage : Page
{
    static MainPage()
    {
        // todo: find proper way to load assembly
        Mapsui.Tests.Common.Utilities.LoadAssembly();
    }

    public MainPage()
    {
        InitializeComponent();

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

    private UIElement CreateRadioButton(ISampleBase sample)
    {
        var radioButton = new RadioButton
        {
            FontSize = 16,
            Content = sample.Name,
            Margin = new Thickness(4)
        };

        radioButton.Click += (_, _) =>
        {
            Catch.Exceptions(async () =>
            {
                MapControl.Map?.Layers.Clear();
                MapControl.Info -= MapOnInfo;
                await sample.SetupAsync(MapControl);
                MapControl.Info += MapOnInfo;
                MapControl.Refresh();
            });
        };

        return radioButton;
    }

    private void RotationSlider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        var percent = RotationSlider.Value / (RotationSlider.Maximum - RotationSlider.Minimum);
        MapControl.Navigator?.RotateTo(percent * 360);
        MapControl.Refresh();
    }
}
