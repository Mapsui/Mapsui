using Mapsui.Samples.Common.Extensions;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Samples.Eto;

using System;
using System.Text;
using System.Linq;
using Mapsui.UI;
using Mapsui.UI.Eto;
using Mapsui.Logging;
using Mapsui.Extensions;
using Mapsui.Samples.Common;

using global::Eto.Forms;
using global::Eto.Drawing;

public class MainForm : Form
{
    static MainForm()
    {
        // todo: find proper way to load assembly
        Mapsui.Tests.Common.Utilities.LoadAssembly();
    }

    DropDown CategoryComboBox = new() { Width = 200 };
    StackLayout SampleList = new();
    MapControl MapControl = new();
    Label FeatureInfo = new(); // 'click info'
    Label LogTextBox = new(); // 'information time'
    Label MouseCoordinates = new();
    StackLayout LayerList = new() { HorizontalContentAlignment = HorizontalAlignment.Right };
    Slider RotationSlider = new() { Width = 200 };
    public MainForm()
    {
        this.MinimumSize = new Size(3, 2) * 100;
        this.Size = this.MinimumSize * 4;
        this.Padding = 10;

        var eto_platform = global::Eto.Platform.Instance.ToString();
        var os_platform = System.Environment.OSVersion.ToString();
        this.Title = $"Mapsui SampleApp - {eto_platform} - {os_platform}";

        MapControl.MouseMove += MapControlOnMouseMove;
        MapControl.Map!.RotationLock = false;
        MapControl.UnSnapRotationDegrees = 30;
        MapControl.ReSnapRotationDegrees = 5;
        RotationSlider.ValueChanged += RotationSliderChanged;

        MapControl.ZoomButton = MouseButtons.Alternate;
        MapControl.ZoomModifier = Keys.None;

        Logger.LogDelegate += LogMethod;

        CategoryComboBox.SelectedValueChanged += CategoryComboBox_SelectedValueChanged;

        FillComboBoxWithCategories();
        FillListWithSamples();

        //

        var sample_layout = new StackLayout(CategoryComboBox, SampleList);

        var map_layout = new PixelLayout();
        map_layout.SizeChanged += MapLayoutSizeChanged;
        map_layout.Add(MapControl, Point.Empty);
        map_layout.Add(LayerList, Point.Empty);
        map_layout.Add(LogTextBox, Point.Empty);
        map_layout.Add(FeatureInfo, Point.Empty);
        map_layout.Add(MouseCoordinates, Point.Empty);

        this.Content = new DynamicLayout(new DynamicRow(sample_layout, map_layout)) { Spacing = new Size(4, 4) };
    }
    private void MapLayoutSizeChanged(object? sender, EventArgs e)
    {
        if (sender is PixelLayout layout)
        {
            MapControl.Size = layout.Size;
            layout.Move(LayerList, layout.Width - LayerList.Width, 0);
            var feature_info_height = MouseCoordinates.Height * 2;
            var logtext_box_height = MouseCoordinates.Height * _logMessage.Limit;
            layout.Move(LogTextBox, 0, layout.Height - feature_info_height - logtext_box_height);
            layout.Move(FeatureInfo, 0, layout.Height - feature_info_height);
            layout.Move(MouseCoordinates, (layout.Width - MouseCoordinates.Width) / 2, layout.Height - MouseCoordinates.Height);
        }
    }

    private void MapControlOnMouseMove(object? sender, MouseEventArgs e)
    {
        var worldPosition = MapControl.Viewport.ScreenToWorld(e.Location.X, e.Location.Y);
        MouseCoordinates.Text = $"{worldPosition.X:F0}, {worldPosition.Y:F0}";
    }
    private void FillListWithSamples()
    {
        var selectedCategory = CategoryComboBox.SelectedValue?.ToString() ?? "";
        SampleList.Items.Clear();
        foreach (var sample in AllSamples.GetSamples().Where(s => s.Category == selectedCategory))
            SampleList.Items.Add(CreateRadioButton(sample));

        if (SampleList.Items.First().Control is RadioButton radioButton)
        {
            radioButton.Checked = true;
        }
    }
    private void CategoryComboBox_SelectedValueChanged(object? sender, EventArgs e)
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
        var radioButton = new RadioButton(SampleList.Items.FirstOrDefault()?.Control as RadioButton)
        {
            Font = Fonts.Cursive(12),
            Text = sample.Name,
        };

        radioButton.CheckedChanged += (s, a) =>
        {
            Catch.Exceptions(async () =>
            {
                MapControl.Map?.Layers.Clear();

                await sample.SetupAsync(MapControl);

                MapControl.Info += MapControlOnInfo;

                LayerList.Items.Clear();
                if (MapControl.Map != null)
                    foreach (var layer in MapControl.Map.Layers)
                        LayerList.Items.Add(new LayerListItem(layer));
                LayerList.Items.Add(RotationSlider);
            });
        };
        return radioButton;
    }

    readonly LimitedQueue<LogModel> _logMessage = new(6);
    private void LogMethod(LogLevel logLevel, string? message, Exception? exception)
    {
        _logMessage.Enqueue(new LogModel { Exception = exception, LogLevel = logLevel, Message = message });
        Application.Instance.AsyncInvoke(() => LogTextBox.Text = ToMultiLineString(_logMessage));
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
    private void RotationSliderChanged(object? sender, EventArgs e)
    {
        var percent = (double)RotationSlider.Value / (RotationSlider.MaxValue - RotationSlider.MinValue);
        MapControl.Navigator?.RotateTo(percent * 360);
        MapControl.Refresh();
    }

    private void MapControlOnInfo(object? sender, MapInfoEventArgs args)
    {
        if (args.MapInfo?.Feature != null)
        {
            FeatureInfo.Visible = true;
            FeatureInfo.Text = $"Click Info:{Environment.NewLine}{args.MapInfo.Feature.ToDisplayText()}";
        }
        else
        {
            FeatureInfo.Visible = false;
        }
    }
}
