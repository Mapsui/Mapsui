
namespace Mapsui.Samples.Eto
{
    using System;
    using System.Text;
    using System.Linq;
    using Mapsui.UI;
    using Mapsui.UI.Eto;
    using Mapsui.Logging;
    using Mapsui.Extensions;
    using Mapsui.Samples.Common;
    using Mapsui.Samples.Common.Desktop;

    using global::Eto.Forms;
    using global::Eto.Drawing;

    public partial class MainForm : Form
    {
        DropDown CategoryComboBox = new () { Width = 200 };
        StackLayout SampleList = new ();
        MapControl MapControl = new ();
        Label FeatureInfo = new (); // 'click info'
        Label LogTextBox = new(); // 'information time'
        Label MouseCoordinates = new();
        Slider RotationSlider = new() { Width = 200 };
        public MainForm()
        {
            this.InitializeComponent();

            this.Size = new Size(3, 2) * 340;

            MapControl.MouseMove += MapControlOnMouseMove;
            MapControl.Map!.RotationLock = false;
            MapControl.UnSnapRotationDegrees = 30;
            MapControl.ReSnapRotationDegrees = 5;
            RotationSlider.ValueChanged += RotationSliderChanged;

            Logger.LogDelegate += LogMethod;

            CategoryComboBox.SelectedValueChanged += CategoryComboBox_SelectedValueChanged;

            FillComboBoxWithCategories();
            FillListWithSamples();

            //

            var left = new StackLayout(
                CategoryComboBox,
                SampleList);

#if false
            var right = new DynamicLayout(
                new DynamicRow(new DynamicControl() { Control = MapControl, XScale=true, YScale=true }),
                LogTextBox,
                FeatureInfo);
#elif false
            var right = new LayeredLayout();
            right.Add(MapControl);

#if false
            var layout = new DynamicLayout(
                new DynamicRow(null, null, RotationSlider),
                null,
                new DynamicRow(LogTextBox, null, null), 
                new DynamicRow(FeatureInfo, MouseCoordinates, null));
#elif true
            var layout = new StackLayout();
            layout.Items.Add(new StackLayoutItem(RotationSlider, HorizontalAlignment.Right));
            layout.Items.Add(null);
            layout.Items.Add(new StackLayoutItem(LogTextBox));
            var lower_row = new StackLayout(FeatureInfo, null, MouseCoordinates, null) { Orientation = Orientation.Horizontal };
            layout.Items.Add(new StackLayoutItem(lower_row, HorizontalAlignment.Stretch));
#endif
            right.Add(layout);
            right.MouseMove += MapControlOnMouseMove;
#elif true
            var map_layout = new PixelLayout();
            map_layout.SizeChanged += MapLayoutSizeChanged;
            map_layout.Add(MapControl, Point.Empty);
            map_layout.Add(RotationSlider, Point.Empty);
            map_layout.Add(LogTextBox, Point.Empty);
            map_layout.Add(FeatureInfo, Point.Empty);
            map_layout.Add(MouseCoordinates, Point.Empty);
//            LogTextBox.SizeChanged += (o, e) => map_layout.Move(LogTextBox, 0, map_layout.Height - FeatureInfo.Height - LogTextBox.Height);
#endif
            // FeatureInfo -> demo/map info -> klikk sirkel

            this.Content = new DynamicLayout(new DynamicRow(left, map_layout)) { Spacing = new Size(4,4) };
        }
        private void MapLayoutSizeChanged(object? sender, EventArgs e)
        {
            if (sender is PixelLayout layout)
            {
                MapControl.Size = layout.Size;
                layout.Move(RotationSlider, layout.Width - RotationSlider.Width, 0);
                var feature_info_height = MouseCoordinates.Height * 2;
                var logtext_box_height = MouseCoordinates.Height * _logMessage.Limit;
                layout.Move(LogTextBox, 0, layout.Height - feature_info_height - logtext_box_height);
                layout.Move(FeatureInfo, 0, layout.Height - feature_info_height);
                layout.Move(MouseCoordinates, (layout.Width - MouseCoordinates.Width) / 2, layout.Height - MouseCoordinates.Height);
            }
        }

        private void MapControlOnMouseMove(object? sender, MouseEventArgs e)
        {
//            var screenPosition = e.GetPosition(MapControl);
            var screenPosition = e.Location;
            var worldPosition = MapControl.Viewport.ScreenToWorld(screenPosition.X, screenPosition.Y);
            MouseCoordinates.Text = $"{worldPosition.X:F0}, {worldPosition.Y:F0}";
        }
        private void FillListWithSamples()
        {
            var selectedCategory = CategoryComboBox.SelectedValue?.ToString() ?? "";
            SampleList.Items.Clear();
            foreach (var sample in AllSamples.GetSamples().Where(s => s.Category == selectedCategory))
                SampleList.Items.Add(CreateRadioButton(sample));

            (SampleList.Items.First().Control as RadioButton).Checked = true;
        }
        private void CategoryComboBox_SelectedValueChanged(object? sender, EventArgs e)
        {
            FillListWithSamples();
        }
        private void FillComboBoxWithCategories()
        {
            // todo: find proper way to load assembly
            DesktopSamplesUtilities.LoadAssembly();
            Tests.Common.Utilities.LoadAssembly();

            var categories = AllSamples.GetSamples().Select(s => s.Category).Distinct().OrderBy(c => c);
            foreach (var category in categories)
            {
                CategoryComboBox.Items.Add(category);
            }

            CategoryComboBox.SelectedIndex = 0;
        }
        private RadioButton CreateRadioButton(ISample sample)
        {
            var radioButton = new RadioButton(SampleList.Items.FirstOrDefault()?.Control as RadioButton)
            {
                Font = Fonts.Cursive(12),
                Text = sample.Name,
            };

            radioButton.CheckedChanged += (s, a) => {
                MapControl.Map?.Layers.Clear();

                sample.Setup(MapControl);

                MapControl.Info += MapControlOnInfo;
                if (MapControl.Map != null) // tftf todo control øverst til høyre
                    ; // LayerList.Initialize(MapControl.Map.Layers);
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
            var percent = (double) RotationSlider.Value / (RotationSlider.MaxValue - RotationSlider.MinValue);
            MapControl.Navigator.RotateTo(percent * 360);
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
}
