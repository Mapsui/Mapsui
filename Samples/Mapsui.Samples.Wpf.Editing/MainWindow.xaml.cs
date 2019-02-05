using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Mapsui.Layers;
using Mapsui.Samples.Wpf.Editing.Editing;
using Mapsui.Samples.Wpf.Editing.Utilities;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.UI.Wpf;

namespace Mapsui.Samples.Wpf.Editing
{
    public partial class MainWindow
    {
        private WritableLayer _targetLayer;
        private IEnumerable<IFeature> _tempFeatures;
        private readonly EditManager _editManager = new EditManager();
        private readonly EditManipulation _editManipulation = new EditManipulation();
        private bool _selectMode;
        private readonly LimitedQueue<LogModel> _logMessage = new LimitedQueue<LogModel>(6);
        
        public MainWindow()
        {
            InitializeComponent();

            MapControl.MouseMove += MapControlOnMouseMove;
            MapControl.MouseLeftButtonDown += MapControlOnMouseLeftButtonDown;
            MapControl.MouseLeftButtonUp += MapControlOnMouseLeftButtonUp;

            MapControl.Map.RotationLock = false;
            MapControl.UnSnapRotationDegrees = 30;
            MapControl.ReSnapRotationDegrees = 5;

            Logger.LogDelegate += LogMethod;

            FillComboBoxWithDemoSamples();

            RenderMode.SelectionChanged += RenderModeOnSelectionChanged;
            TargetLayer.SelectionChanged += TargetLayerOnSelectionChanged;
            var firstRadioButton = (RadioButton)SampleList.Children[0];
            firstRadioButton.IsChecked = true;
            firstRadioButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
        private void TargetLayerOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var selectedValue = ((ComboBoxItem)((ComboBox)sender).SelectedItem).Content.ToString();

            if (selectedValue.ToLower().Contains("point"))
                _targetLayer = (WritableLayer)MapControl.Map.Layers.First(l => l.Name == "PointLayer");
            else if (selectedValue.ToLower().Contains("line"))
                _targetLayer = (WritableLayer)MapControl.Map.Layers.First(l => l.Name == "LineLayer");
            else if (selectedValue.ToLower().Contains("polygon"))
                _targetLayer = (WritableLayer)MapControl.Map.Layers.First(l => l.Name == "PolygonLayer");
            else
                throw new Exception("Unknown ComboBox item");
        }

        private void RenderModeOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var selectedValue = ((ComboBoxItem)((ComboBox)sender).SelectedItem).Content.ToString();

            if (selectedValue.ToLower().Contains("wpf"))
                MapControl.RenderMode = UI.Wpf.RenderMode.Wpf;
            else if (selectedValue.ToLower().Contains("skia"))
                MapControl.RenderMode = UI.Wpf.RenderMode.Skia;
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

            radioButton.Click += (s, a) =>
            {
                MapControl.Map.Layers.Clear();
                MapControl.Map = sample.Value();
 
                LayerList.Initialize(MapControl.Map.Layers);
                InitializeZoomSlider(MapControl.Map.Resolutions);
                if (MapControl.Map.Layers.Any(l => l.Name.ToLower().Contains("edit"))) InitializeEditSetup();
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
        
        private void LogMethod(LogLevel logLevel, string message, Exception exception)
        {
            Dispatcher.Invoke(() =>
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
            MapControl.Navigator.RotateTo(percent * 360);
            MapControl.Refresh();
        }

        private void ZoomSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            MapControl.Navigator.ZoomTo(MapControl.Map.Resolutions[(int)args.NewValue]);
        }

        private void InitializeEditSetup()
        {
            _editManager.Layer = (WritableLayer)MapControl.Map.Layers.First(l => l.Name == "EditLayer");
            _targetLayer = (WritableLayer)MapControl.Map.Layers.First(l => l.Name == "PolygonLayer");

            // Load the polygon layer on startup so you can start modifying right away
            _editManager.Layer.AddRange(_targetLayer.GetFeatures().Copy());
            _targetLayer.Clear();

            _editManager.EditMode = EditMode.Modify;
            Loaded += (sender, args) =>
            {
                MapControl.Navigator.NavigateTo(_editManager.Layer.Envelope.Grow(_editManager.Layer.Envelope.Width * 0.2));
            };
        }

        private void AddPoint_OnClick(object sender, RoutedEventArgs args)
        {
            IEnumerable<IFeature> features = _targetLayer.GetFeatures().Copy();

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
            _targetLayer.AddRange(_editManager.Layer.GetFeatures().Copy());
            _editManager.Layer.Clear();

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
            IEnumerable<IFeature> features = _targetLayer.GetFeatures().Copy();

			foreach (var feature in features)
	        {
		        feature.RenderedGeometry.Clear();
	        }

			_tempFeatures = new List<IFeature>(features);

            _editManager.EditMode = EditMode.AddLine;
        }

        private void AddPolygon_OnClick(object sender, RoutedEventArgs args)
        {
			IEnumerable<IFeature> features = _targetLayer.GetFeatures().Copy();

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
	        IEnumerable<IFeature> features = _targetLayer.GetFeatures().Copy();

			foreach (var feature in features)
	        {
		        feature.RenderedGeometry.Clear();
	        }

			_tempFeatures = new List<IFeature>(features);

			_editManager.Layer.AddRange(features);
			_targetLayer.Clear();

            MapControl.RefreshGraphics();
        }

	    private void Cancel_OnClick(object sender, RoutedEventArgs args)
	    {
            if (_targetLayer != null && _tempFeatures != null)
		    {
			    _targetLayer.Clear(); _targetLayer.AddRange(_tempFeatures.Copy());
			    MapControl.RefreshGraphics();
			}

		    _editManager.Layer.Clear();

		    MapControl.RefreshGraphics();

		    _editManager.EditMode = EditMode.None;

			_tempFeatures = null;
	    }

        private void Delete_OnClick(object sender, RoutedEventArgs args)
        {
            if (_selectMode)
            {
                var selectedFeatures = _editManager.Layer.GetFeatures().Where(f => (bool?) f["Selected"] == true);

                foreach (var selectedFeature in selectedFeatures)
                {
                    _editManager.Layer.TryRemove(selectedFeature);
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
            MapControl.Map.PanLock = _editManipulation.Manipulate(MouseState.Up,
                args.GetPosition(MapControl).ToMapsui(), _editManager, MapControl);

            if (_selectMode)
            {
                var infoArgs = MapControl.GetMapInfo(args.GetPosition(MapControl).ToMapsui());
                if (infoArgs.Feature != null)
                {
                    var currentValue = (bool?)infoArgs.Feature["Selected"] == true;
                    infoArgs.Feature["Selected"] = !currentValue; // invert current value
                }
            }
        }

        private void MapControlOnMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
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
}