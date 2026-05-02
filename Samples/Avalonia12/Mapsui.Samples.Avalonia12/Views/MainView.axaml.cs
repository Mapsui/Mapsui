using System.Linq;
using Avalonia.Controls;
using Mapsui.Extensions;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.Tiling;

namespace Mapsui.Samples.Avalonia12.Views;

public partial class MainView : UserControl
{
    static MainView()
    {
        SampleConfiguration.ApplyRendererConfig();
        Common.Samples.Register();
    }

    public MainView()
    {
        InitializeComponent();
        MapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());

        CategoryComboBox.SelectionChanged += CategoryComboBoxSelectionChanged;
        SampleComboBox.SelectionChanged += SampleComboBoxSelectionChanged;

        FillCategoryComboBox();
    }

    private void FillCategoryComboBox()
    {
        var categories = AllSamples.GetSamples()
            .Select(s => s.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToArray();

        CategoryComboBox.ItemsSource = categories;
        CategoryComboBox.SelectedIndex = 0;
    }

    private void FillSampleComboBox()
    {
        var selectedCategory = CategoryComboBox.SelectedItem?.ToString() ?? "";

        var samples = AllSamples.GetSamples()
            .Where(s => s.Category == selectedCategory)
            .ToArray();

        SampleComboBox.ItemsSource = samples;
        SampleComboBox.DisplayMemberBinding = new Avalonia.Data.Binding("Name");

        // Auto-load the first sample in the category
        SampleComboBox.SelectedIndex = 0;
    }

    private void CategoryComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        FillSampleComboBox();
    }

    private void SampleComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SampleComboBox.SelectedItem is not ISampleBase sample)
            return;

        Catch.Exceptions(async () =>
        {
            MapControl.Map?.Layers.ClearAllGroups();
            await sample.SetupAsync(MapControl);
            MapControl.Refresh();
        });
    }
}
