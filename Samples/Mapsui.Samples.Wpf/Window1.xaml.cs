using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Samples.CustomWidget;
using Mapsui.Samples.Wpf.Utilities;
using Mapsui.UI;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;

namespace Mapsui.Samples.Wpf;

public partial class Window1
{
    static Window1()
    {
        // todo: find proper way to load assembly
        Mapsui.Tests.Common.Utilities.LoadAssembly();
    }

    public Window1()
    {
        InitializeComponent();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        MapControl.FeatureInfo += MapControlFeatureInfo;
        MapControl.MouseMove += MapControlOnMouseMove;
        MapControl.Map!.RotationLock = false;
        MapControl.UnSnapRotationDegrees = 30;
        MapControl.ReSnapRotationDegrees = 5;
        MapControl.Renderer.WidgetRenders[typeof(CustomWidget.CustomWidget)] = new CustomWidgetSkiaRenderer();

        Logger.LogDelegate += LogMethod;

        CategoryComboBox.SelectionChanged += CategoryComboBoxSelectionChanged;

        FillComboBoxWithCategories();
        FillListWithSamples();
    }

    private void MapControlOnMouseMove(object sender, MouseEventArgs e)
    {
        var screenPosition = e.GetPosition(MapControl);
        var worldPosition = MapControl.Viewport.ScreenToWorld(screenPosition.X, screenPosition.Y);
        MouseCoordinates.Text = $"{worldPosition.X:F0}, {worldPosition.Y:F0}";
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
                MapControl.Map?.Layers.Clear();

                await sample.SetupAsync(MapControl);

                MapControl.Info += MapControlOnInfo;
                if (MapControl.Map != null)
                    LayerList.Initialize(MapControl.Map.Layers);
            });
        };
        return radioButton;
    }

    readonly LimitedQueue<LogModel> _logMessage = new LimitedQueue<LogModel>(6);

    private void LogMethod(LogLevel logLevel, string? message, Exception? exception)
    {
        _logMessage.Enqueue(new LogModel { Exception = exception, LogLevel = logLevel, Message = message });
        Dispatcher.BeginInvoke(() => LogTextBox.Text = ToMultiLineString(_logMessage));
    }

    private string ToMultiLineString(LimitedQueue<LogModel> logMessages)
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

    private static void MapControlFeatureInfo(object? sender, FeatureInfoEventArgs e)
    {
        MessageBox.Show(e.FeatureInfo?.ToDisplayText());
    }

    private void RotationSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var percent = RotationSlider.Value / (RotationSlider.Maximum - RotationSlider.Minimum);
        MapControl.Navigator?.RotateTo(percent * 360);
        MapControl.Refresh();
    }

    private void MapControlOnInfo(object? sender, MapInfoEventArgs args)
    {
        if (args.MapInfo?.Feature != null)
        {
            FeatureInfoBorder.Visibility = Visibility.Visible;
            FeatureInfo.Text = $"Click Info:{Environment.NewLine}{args.MapInfo.Feature.ToDisplayText()}";
        }
        else
        {
            FeatureInfoBorder.Visibility = Visibility.Collapsed;
        }

    }
}
