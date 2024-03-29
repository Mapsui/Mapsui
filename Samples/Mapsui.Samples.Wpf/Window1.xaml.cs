using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Mapsui.Extensions;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.Samples.Common.Maps.Widgets;

namespace Mapsui.Samples.Wpf;

// Line below had to be added to suppress Warning CA1416 'Call site reachable by all platforms', although WPF only runs on Windows.
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class Window1
{
    static Window1()
    {
        Mapsui.Tests.Common.Samples.Register();
        Mapsui.Samples.Common.Samples.Register();
    }

    public Window1()
    {
        InitializeComponent();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        MapControl.Map.Navigator.RotationLock = false;
        MapControl.Renderer.WidgetRenders[typeof(CustomWidget)] = new CustomWidgetSkiaRenderer();

        CategoryComboBox.SelectionChanged += CategoryComboBoxSelectionChanged;

        FillComboBoxWithCategories();
        FillListWithSamples();
    }

    private void FillListWithSamples()
    {
        var selectedCategory = CategoryComboBox.SelectedValue?.ToString() ?? "";
        SampleList.Children.Clear();
        foreach (var sample in AllSamples.GetSamples().Where(s => s.Category == selectedCategory))
            SampleList.Children.Add(CreateRadioButton(sample));

        var firstRadioButton = (RadioButton)SampleList.Children[0];
        firstRadioButton.IsChecked = true;
        firstRadioButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    }

    private void CategoryComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        FillListWithSamples();
    }

    private void FillComboBoxWithCategories()
    {
        Mapsui.Tests.Common.Samples.Register();
        Mapsui.Samples.Common.Samples.Register();

        var categories = AllSamples.GetSamples().Select(s => s.Category).Distinct().OrderBy(c => c);
        foreach (var category in categories)
        {
            CategoryComboBox.Items.Add(category);
        }

        CategoryComboBox.SelectedIndex = 0;
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

                await sample.SetupAsync(MapControl);

                if (MapControl.Map != null)
                    LayerList.Initialize(MapControl.Map.Layers);
            });
        };
        return radioButton;
    }

    private void RotationSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var percent = RotationSlider.Value / (RotationSlider.Maximum - RotationSlider.Minimum);
        MapControl.Map.Navigator.RotateTo(percent * 360);
    }
}
