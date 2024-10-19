using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Mapsui.Extensions;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;

using Mapsui.Tiling;
using Mapsui.Samples.Common.Maps.Widgets;

namespace Mapsui.Samples.Avalonia.Views;

public partial class MainView : UserControl
{
    static MainView()
    {
        Mapsui.Tests.Common.Samples.Register();
        Mapsui.Samples.Common.Samples.Register();
    }

    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        InitializeComponent(true);

        MapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
        MapControl.Map.Navigator.RotationLock = false;
        MapControl.Renderer.WidgetRenders[typeof(CustomWidget)] = new CustomWidgetSkiaRenderer();

        RotationSlider.PointerMoved += RotationSliderOnPointerMoved;

        CategoryComboBox.SelectionChanged += CategoryComboBoxSelectionChanged;

        FillComboBoxWithCategories();
        FillListWithSamples();
    }

    private void FillComboBoxWithCategories()
    {
        Common.Samples.Register();
        Tests.Common.Samples.Register();

        var categories = AllSamples.GetSamples().Select(s => s.Category).Distinct().OrderBy(c => c).ToArray();

        CategoryComboBox.ItemsSource = categories;

        CategoryComboBox.SelectedIndex = 1;
    }

    private void FillListWithSamples()
    {
        var selectedCategory = CategoryComboBox.SelectedItem?.ToString() ?? "";
        SampleList.Children.Clear();
        foreach (var sample in AllSamples.GetSamples().Where(s => s.Category == selectedCategory))
        {
            SampleList.Children.Add(CreateRadioButton(sample));
        }
    }

    private void CategoryComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        FillListWithSamples();
    }

    private RadioButton CreateRadioButton(ISampleBase sample)
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
                MapControl.Map?.Layers.ClearAllGroups();
                await sample.SetupAsync(MapControl);
                MapControl.Refresh();
            });
        };

        return radioButton;
    }

    private void RotationSliderOnPointerMoved(object? sender, PointerEventArgs e)
    {
        // This is probably not the proper event handler for this but I don't know what is.
        var percent = RotationSlider.Value / (RotationSlider.Maximum - RotationSlider.Minimum);
        MapControl.Map.Navigator.RotateTo(percent * 360);
    }
}
