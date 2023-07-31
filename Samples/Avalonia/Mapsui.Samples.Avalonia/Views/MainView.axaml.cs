using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Mapsui.Extensions;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.Samples.CustomWidget;
using Mapsui.Tiling;
using Mapsui.UI;

namespace Mapsui.Samples.Avalonia.Views;

public partial class MainView : UserControl
{
    static MainView()
    {
        // todo: find proper way to load assembly
        Mapsui.Tests.Common.Utilities.LoadAssembly();
    }

    private Func<IMapControl?, Mapsui.UI.TappedEventArgs, bool>? _clicker;

    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        InitializeComponent(true);

        MapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
        MapControl.Map.Navigator.RotationLock = false;
        MapControl.UnSnapRotationDegrees = 30;
        MapControl.ReSnapRotationDegrees = 5;
        MapControl.Renderer.WidgetRenders[typeof(CustomWidget.CustomWidget)] = new CustomWidgetSkiaRenderer();
        MapControl.SingleTap += MapControl_SingleTap; ;

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
        Tests.Common.Utilities.LoadAssembly();

        var categories = AllSamples.GetSamples().Select(s => s.Category).Distinct().OrderBy(c => c).ToArray();

        CategoryComboBox.ItemsSource = categories;

        CategoryComboBox.SelectedIndex = 1;
    }

    private void MapOnInfo(object? sender, MapInfoEventArgs args)
    {
        if (args.MapInfo?.Feature != null)
        {
            FeatureInfo.Text = $"Click Info:{Environment.NewLine}{args.MapInfo.Feature.ToDisplayText()}";
        }
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
                MapControl.Map?.Layers.Clear();
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

    private void RotationSliderOnPointerMoved(object? sender, PointerEventArgs e)
    {
        // This is probably not the proper event handler for this but I don't know what is.
        var percent = RotationSlider.Value / (RotationSlider.Maximum - RotationSlider.Minimum);
        MapControl.Map.Navigator.RotateTo(percent * 360);
    }
}
