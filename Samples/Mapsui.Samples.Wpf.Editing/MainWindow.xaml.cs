using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Samples.Wpf.Editing.Editing;
using Mapsui.Samples.Wpf.Editing.Utilities;
using Mapsui.Logging;

using Mapsui.UI.Wpf.Extensions;

namespace Mapsui.Samples.Wpf.Editing;

public partial class MainWindow
{
    // In this class we cast to GeometryFeature and this only works because we happen to know
    // there we use only IGeometryFeatures in the writable layer. We need to cast to allow
    // us to copy. This needs to be improved.

    private WritableLayer? _targetLayer;
    private IEnumerable<IFeature>? _tempFeatures;
    private readonly EditManager _editManager = new();
    private readonly EditManipulation _editManipulation = new();
    private bool _selectMode;
    private readonly LimitedQueue<LogModel> _logMessage = new(6);

    public MainWindow()
    {
        InitializeComponent();

        MapControl.MouseMove += MapControlOnMouseMove;
        MapControl.MouseLeftButtonDown += MapControlOnMouseLeftButtonDown;
        MapControl.MouseLeftButtonUp += MapControlOnMouseLeftButtonUp;

        MapControl.Map!.RotationLock = false;
        MapControl.UnSnapRotationDegrees = 30;
        MapControl.ReSnapRotationDegrees = 5;

        Logger.LogDelegate += LogMethod;

        FillComboBoxWithDemoSamples();

        TargetLayer.SelectionChanged += TargetLayerOnSelectionChanged;
        var firstRadioButton = (RadioButton)SampleList.Children[0];
        firstRadioButton.IsChecked = true;
        firstRadioButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    }
    private void TargetLayerOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
        var selectedValue = ((ComboBoxItem)((ComboBox)sender).SelectedItem).Content.ToString();

        if (selectedValue == "Layer 1")
            _targetLayer = MapControl.Map?.Layers.First(l => l.Name == "Layer 1") as WritableLayer;
        else if (selectedValue == "Layer 2")
            _targetLayer = MapControl.Map?.Layers.First(l => l.Name == "Layer 2") as WritableLayer;
        else if (selectedValue == "Layer 3")
            _targetLayer = MapControl.Map?.Layers.First(l => l.Name == "Layer 3") as WritableLayer;
        else
            throw new Exception("Unknown ComboBox item");
    }

    private void FillComboBoxWithDemoSamples()
    {
        SampleList.Children.Clear();
        foreach (var sample in DemoSamples().ToList())
        {
            SampleList.Children.Add(CreateRadioButton(sample));
        }
    }

    private static Dictionary<string, Func<Map>> DemoSamples()
    {
        return AllSamples.CreateList();
    }

    private UIElement CreateRadioButton(KeyValuePair<string, Func<Map>> sample)
    {
        var radioButton = new RadioButton
        {
            FontSize = 16,
            Content = sample.Key,
            Margin = new Thickness(4)
        };

        radioButton.Click += (_, _) =>
        {
            MapControl!.Map?.Layers.Clear();
            MapControl.Map = sample.Value();

            LayerList.Initialize(MapControl!.Map.Layers);
            InitializeZoomSlider(MapControl!.Map.Resolutions);
            if (MapControl!.Map.Layers.Any(l => l.Name.ToLower().Contains("edit"))) InitializeEditSetup();
        };
        return radioButton;
    }

    private void InitializeZoomSlider(IReadOnlyList<double> mapResolutions)
    {
        ZoomSlider.BeginInit();
        ZoomSlider.Ticks.Clear();
        for (var i = 0; i < mapResolutions.Count; i++)
        {
            ZoomSlider.Ticks.Add(i);
        }
        ZoomSlider.Minimum = 0;
        ZoomSlider.Maximum = mapResolutions.Count - 1;
        ZoomSlider.IsSnapToTickEnabled = true;
        ZoomSlider.TickPlacement = TickPlacement.BottomRight;
        ZoomSlider.EndInit();
    }

    private void LogMethod(LogLevel logLevel, string? message, Exception? exception)
    {
        Dispatcher.BeginInvoke(() =>
        {
            _logMessage.Enqueue(new LogModel { Exception = exception, LogLevel = logLevel, Message = message });
            return LogTextBox.Text = ToMultiLineString(_logMessage);
        });
    }

    private string ToMultiLineString(LimitedQueue<LogModel> logMessages)
    {
        var result = new StringBuilder();

        var copy = logMessages.ToList();
        foreach (var logMessage in copy)
        {
            if (logMessage == null) continue;
            result.Append($"[{logMessage.LogLevel}] {logMessage.Message}{Environment.NewLine}");
        }

        return result.ToString();
    }

    private void RotationSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
    {
        var percent = RotationSlider.Value / (RotationSlider.Maximum - RotationSlider.Minimum);
        MapControl.Navigator?.RotateTo(percent * 360);
        MapControl.Refresh();
    }

    private void ZoomSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
    {
        if (MapControl.Map != null)
            MapControl.Navigator?.ZoomTo(MapControl.Map.Resolutions[(int)args.NewValue]);
    }

    private void InitializeEditSetup()
    {
        _editManager.Layer = (WritableLayer)MapControl.Map!.Layers.First(l => l.Name == "EditLayer");
        _targetLayer = (WritableLayer)MapControl.Map.Layers.First(l => l.Name == "Layer 3");

        // Load the polygon layer on startup so you can start modifying right away
        _editManager.Layer.AddRange(_targetLayer.GetFeatures().Copy());
        _targetLayer.Clear();

        _editManager.EditMode = EditMode.Modify;
        Loaded += (_, _) =>
        {
            var extent = _editManager.Layer.Extent!.Grow(_editManager.Layer.Extent.Width * 0.2);
            MapControl.Navigator?.NavigateTo(extent);
        };
    }

    private void AddPoint_OnClick(object sender, RoutedEventArgs args)
    {
        var features = _targetLayer?.GetFeatures().Copy() ?? Array.Empty<IFeature>();

        foreach (var feature in features)
        {
            feature.RenderedGeometry.Clear();
        }

        _tempFeatures = new List<IFeature>(features);

        _editManager.EditMode = EditMode.AddPoint;
    }

    private void Modify_OnClick(object sender, RoutedEventArgs args)
    {
        _editManager.EditMode = EditMode.Modify;
    }

    private void Save_OnClick(object sender, RoutedEventArgs args)
    {
        _targetLayer?.AddRange(_editManager.Layer?.GetFeatures().Copy() ?? new List<IFeature>());
        _editManager.Layer?.Clear();

        MapControl.RefreshGraphics();
    }

    private void None_OnClick(object sender, RoutedEventArgs args)
    {
        _editManager.EditMode = EditMode.None;
    }

    private void Select_OnClick(object sender, RoutedEventArgs args)
    {
        _selectMode = !_selectMode;
    }

    private void AddLine_OnClick(object sender, RoutedEventArgs args)
    {
        var features = _targetLayer?.GetFeatures().Copy() ?? Array.Empty<IFeature>();

        foreach (var feature in features)
        {
            feature.RenderedGeometry.Clear();
        }

        _tempFeatures = new List<IFeature>(features);

        _editManager.EditMode = EditMode.AddLine;
    }

    private void AddPolygon_OnClick(object sender, RoutedEventArgs args)
    {
        var features = _targetLayer?.GetFeatures().Copy() ?? Array.Empty<IFeature>();

        foreach (var feature in features)
        {
            feature.RenderedGeometry.Clear();
        }

        _tempFeatures = new List<IFeature>(features);

        _editManager.EditMode = EditMode.AddPolygon;
    }
    private void Rotate_OnClick(object sender, RoutedEventArgs e)
    {
        _editManager.EditMode = EditMode.Rotate;
    }

    private void Scale_OnClick(object sender, RoutedEventArgs e)
    {
        _editManager.EditMode = EditMode.Scale;
    }

    private void Load_OnClick(object sender, RoutedEventArgs args)
    {
        var features = _targetLayer?.GetFeatures().Copy() ?? Array.Empty<IFeature>();

        foreach (var feature in features)
        {
            feature.RenderedGeometry.Clear();
        }

        _tempFeatures = new List<IFeature>(features);

        _editManager.Layer?.AddRange(features);
        _targetLayer?.Clear();

        MapControl.RefreshGraphics();
    }

    private void Cancel_OnClick(object sender, RoutedEventArgs args)
    {
        if (_targetLayer != null && _tempFeatures != null)
        {
            _targetLayer.Clear(); _targetLayer.AddRange(_tempFeatures.Copy());
            MapControl.RefreshGraphics();
        }

        _editManager.Layer?.Clear();

        MapControl.RefreshGraphics();

        _editManager.EditMode = EditMode.None;

        _tempFeatures = null;
    }

    private void Delete_OnClick(object sender, RoutedEventArgs args)
    {
        if (_selectMode)
        {
            var selectedFeatures = _editManager.Layer?.GetFeatures().Where(f => (bool?)f["Selected"] == true) ?? Array.Empty<IFeature>();

            foreach (var selectedFeature in selectedFeatures)
            {
                _editManager.Layer?.TryRemove(selectedFeature);
            }
            MapControl.RefreshGraphics();
        }
    }

    private void MapControlOnMouseMove(object sender, MouseEventArgs args)
    {
        var screenPosition = args.GetPosition(MapControl).ToMapsui();
        var worldPosition = MapControl.Viewport.ScreenToWorld(screenPosition);
        MouseCoordinates.Text = $"{worldPosition.X:F0}, {worldPosition.Y:F0}";

        if (args.LeftButton == MouseButtonState.Pressed)
        {
            _editManipulation.Manipulate(MouseState.Dragging, screenPosition,
                _editManager, MapControl);
        }
        else
        {
            _editManipulation.Manipulate(MouseState.Moving, screenPosition,
                _editManager, MapControl);
        }
    }

    private void MapControlOnMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
    {
        if (MapControl.Map != null)
            MapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.Up,
            args.GetPosition(MapControl).ToMapsui(), _editManager, MapControl);

        if (_selectMode)
        {
            var infoArgs = MapControl.GetMapInfo(args.GetPosition(MapControl).ToMapsui());
            if (infoArgs?.Feature != null)
            {
                var currentValue = (bool?)infoArgs.Feature["Selected"] == true;
                infoArgs.Feature["Selected"] = !currentValue; // invert current value
            }
        }
    }

    private void MapControlOnMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
    {
        if (MapControl.Map == null)
            return;

        if (args.ClickCount > 1)
        {
            MapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.DoubleClick,
                args.GetPosition(MapControl).ToMapsui(), _editManager, MapControl);
            args.Handled = true;
        }
        else
        {
            MapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.Down,
                args.GetPosition(MapControl).ToMapsui(), _editManager, MapControl);
        }
    }
}
