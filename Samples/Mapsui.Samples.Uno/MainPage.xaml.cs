using System.Collections.Generic;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.Tiling;

namespace Mapsui.Samples.Uno;

public sealed partial class MainPage : Page
{
    private readonly IReadOnlyList<ISampleBase> _allSamples;
    private IReadOnlyList<ISampleBase> _samplesInCategory = [];
    private bool _isLoadingSample;

    static MainPage()
    {
        SampleConfiguration.ApplyRendererConfig();
        Common.Samples.Register();
    }

    public MainPage()
    {
        InitializeComponent();

        _allSamples = AllSamples.GetSamples().ToList();

        MapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
        MapControl.Map.Navigator.RotationLock = false;

        FillCategoryComboBox();
    }

    private void FillCategoryComboBox()
    {
        var categories = _allSamples
            .Select(sample => sample.Category)
            .Distinct()
            .OrderBy(category => category)
            .ToList();

        CategoryComboBox.ItemsSource = categories;
        CategoryComboBox.SelectedIndex = categories.Count > 0 ? 0 : -1;
    }

    private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedCategory = CategoryComboBox.SelectedItem?.ToString() ?? string.Empty;

        _samplesInCategory = _allSamples
            .Where(sample => sample.Category == selectedCategory)
            .OrderBy(sample => sample.Name)
            .ToList();

        SampleComboBox.ItemsSource = _samplesInCategory.Select(sample => sample.Name).ToList();
        SampleComboBox.SelectedIndex = _samplesInCategory.Count > 0 ? 0 : -1;
    }

    private void SampleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoadingSample)
            return;

        var index = SampleComboBox.SelectedIndex;
        if (index < 0 || index >= _samplesInCategory.Count)
            return;

        var selectedSample = _samplesInCategory[index];

        Catch.Exceptions(async () =>
        {
            _isLoadingSample = true;
            try
            {
                MapControl.Map!.Layers.ClearAllGroups();
                await selectedSample.SetupAsync(MapControl);
                MapControl.Refresh();
            }
            finally
            {
                _isLoadingSample = false;
            }
        });
    }
}

