﻿using Microsoft.UI.Xaml.Controls.Primitives;
using Mapsui.Extensions;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.Tiling;
using RadioButton = Microsoft.UI.Xaml.Controls.RadioButton;
using Mapsui.Samples.Common.Maps.Widgets;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mapsui.Samples.Uno.WinUI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    static MainPage()
    {
        Mapsui.Tests.Common.Samples.Register();
        Mapsui.Samples.Common.Samples.Register();
    }

    public MainPage()
    {
        InitializeComponent();

        MapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
        MapControl.Map.Navigator.RotationLock = false;
        MapControl.Renderer.WidgetRenders[typeof(CustomWidget)] = new CustomWidgetSkiaRenderer();

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
                MapControl.Map!.Layers.ClearAllGroups();
                await sample.SetupAsync(MapControl);
                MapControl.Refresh();
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
