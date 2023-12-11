using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Samples.CustomWidget;
using Mapsui.Samples.Wpf.Utilities;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.UI.Wpf;
using System.Windows.Threading;

namespace Mapsui.Samples.Wpf;

// Line below had to be added to suppress Warning CA1416 'Call site reachable by all platforms', although WPF only runs on Windows.
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class Window1
{
    static Window1()
    {
        // todo: find proper way to load assembly
        Tests.Common.Utilities.LoadAssembly();
    }

    public Window1()
    {
        InitializeComponent();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        MapControl.Map.Navigator.RotationLock = false;
        MapControl.UnSnapRotationDegrees = 30;
        MapControl.ReSnapRotationDegrees = 5;
        MapControl.Renderer.WidgetRenders[typeof(CustomWidget.CustomWidget)] = new CustomWidgetSkiaRenderer();

        Logger.LogDelegate += LogMethod;

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
        // todo: find proper way to load assembly
        Tests.Common.Utilities.LoadAssembly();

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

    readonly LimitedQueue<LogModel> _logMessage = new(6);

    private void LogMethod(LogLevel logLevel, string? message, Exception? exception)
    {
        _logMessage.Enqueue(new LogModel { Exception = exception, LogLevel = logLevel, Message = message });
        Dispatcher.BeginInvoke(() => LogTextBox.Text = ToMultiLineString(_logMessage));
    }

    private static string ToMultiLineString(LimitedQueue<LogModel> logMessages)
    {
        var result = new StringBuilder();

        var copy = logMessages.ToList();
        foreach (var logMessage in copy)
        {
            if (logMessage == null) continue;
            result.Append($"{logMessage.LogLevel} {logMessage.Message}{Environment.NewLine}");
        }

        return result.ToString();
    }

    private void RotationSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var percent = RotationSlider.Value / (RotationSlider.Maximum - RotationSlider.Minimum);
        MapControl.Map.Navigator.RotateTo(percent * 360);
    }
}
