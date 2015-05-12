// Copyright 2008 - Paul den Dulk (Geodan)
// 
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA f

using Mapsui.Fetcher;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Rendering.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace Mapsui.UI.Xaml
{
    public class MapControl : Grid
    {
        private Map _map;
        private string _errorMessage;
        private readonly DoubleAnimation _zoomAnimation = new DoubleAnimation();
        private readonly Storyboard _zoomStoryBoard = new Storyboard();
        private bool _viewportInitialized;
        private readonly IRenderer _renderer;
        private bool _invalid;
        private readonly Rectangle _bboxRect;
        private readonly Canvas _renderTarget;
        Point _previousPosition;
        private readonly SimpleOrientationSensor _orientationSensor;

        public event EventHandler ErrorMessageChanged;
        public event EventHandler<ViewChangedEventArgs> ViewChanged;

        public bool ZoomToBoxMode { get; set; }
        public IViewport Viewport { get { return Map.Viewport; } }
        
        public Map Map
        {
            get
            {
                return _map;
            }
            set
            {
                if (_map != null)
                {
                    var temp = _map;
                    _map = null;
                    temp.PropertyChanged -= MapPropertyChanged;
                    temp.Dispose();
                }

                _map = value;
                //all changes of all layers are returned through this event handler on the map
                if (_map != null)
                {
                    _map.DataChanged += MapDataChanged;
                    _map.PropertyChanged += MapPropertyChanged;
                    _map.ViewChanged(true);
                }
                OnViewChanged();
                RefreshGraphics();
            }
        }

        void MapPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Envelope")
            {
                InitializeViewport();
                _map.ViewChanged(true);
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
        }

        public bool ZoomLocked { get; set; }

        public MapControl()
        {
            _renderTarget = new Canvas
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = new SolidColorBrush(Colors.Transparent),
                };
            Children.Add(_renderTarget);

            _bboxRect = new Rectangle
                {
                    Fill = new SolidColorBrush(Colors.Red),
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 3,
                    RadiusX = 0.5,
                    RadiusY = 0.5,
                    StrokeDashArray = new DoubleCollection { 3.0 },
                    Opacity = 0.3,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Visibility = Visibility.Collapsed
                };
            Children.Add(_bboxRect);
            
            Map = new Map();
            Loaded += MapControlLoaded;

            SizeChanged += MapControlSizeChanged;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            _renderer = new MapRenderer(_renderTarget);
            PointerWheelChanged += MapControl_PointerWheelChanged;
            
            ManipulationMode = ManipulationModes.Scale | ManipulationModes.TranslateX | ManipulationModes.TranslateY;     
            ManipulationDelta += OnManipulationDelta;
            ManipulationCompleted += OnManipulationCompleted;
            ManipulationInertiaStarting += OnManipulationInertiaStarting;

            _orientationSensor = SimpleOrientationSensor.GetDefault();
            if (_orientationSensor != null)
            {
                _orientationSensor.OrientationChanged += (sender, args) =>
                    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Refresh);
            }
        }

        void MapControl_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (ZoomLocked) return;
            if (!_viewportInitialized) return;

            var currentPoint = e.GetCurrentPoint(this); //Needed for both MouseMove and MouseWheel event for mousewheel event

            var mousePosition = new Geometries.Point(currentPoint.RawPosition.X, currentPoint.RawPosition.Y);

            var newResolution = DetermineNewResolution(currentPoint.Properties.MouseWheelDelta, Map.Viewport.Resolution);

            // 1) Temporarily center on the mouse position
            Map.Viewport.Center = Map.Viewport.ScreenToWorld(mousePosition.X, mousePosition.Y);

            // 2) Then zoom 
            Map.Viewport.Resolution = newResolution;

            // 3) Then move the temporary center of the map back to the mouse position
            Map.Viewport.Center = Map.Viewport.ScreenToWorld(
              Map.Viewport.Width - mousePosition.X,
              Map.Viewport.Height - mousePosition.Y);
            
            e.Handled = true; 

            _map.ViewChanged(true);
            OnViewChanged(true);
        }

        private double DetermineNewResolution(int mouseWheelDelta, double currentResolution)
        {
            if (mouseWheelDelta > 0) return ZoomHelper.ZoomIn(_map.Resolutions, currentResolution);
            if (mouseWheelDelta < 0) return ZoomHelper.ZoomOut(_map.Resolutions, currentResolution);
            return currentResolution;
        }

        private void OnViewChanged(bool userAction = false)
        {
            if (_map != null)
            {
                if (ViewChanged != null)
                {
                    ViewChanged(this, new ViewChangedEventArgs { Viewport = Map.Viewport, UserAction = userAction });
                }
            }
        }

        public void Refresh()
        {
            _map.ViewChanged(true);
            RefreshGraphics();
        }

        private void RefreshGraphics() 
        {
            InvalidateArrange();
            InvalidateMeasure();
            _renderTarget.InvalidateArrange();
            _renderTarget.InvalidateMeasure();
            _invalid = true;
        }

        public void Clear()
        {
            if (_map != null)
            {
                _map.ClearCache();
            }
            RefreshGraphics();
        }

        public void ZoomIn()
        {
            if (ZoomLocked) return;
            if (!_viewportInitialized) return;

            Map.Viewport.Resolution = ZoomHelper.ZoomIn(_map.Resolutions, Map.Viewport.Resolution);

            OnViewChanged();
        }

        public void ZoomOut()
        {
            if (ZoomLocked) return;
            if (!_viewportInitialized) return;

            Map.Viewport.Resolution = ZoomHelper.ZoomOut(_map.Resolutions, Map.Viewport.Resolution);

            OnViewChanged();
        }

        protected void OnErrorMessageChanged(EventArgs e)
        {
            if (ErrorMessageChanged != null)
            {
                ErrorMessageChanged(this, e);
            }
        }
        
        private void MapControlLoaded(object sender, RoutedEventArgs e)
        {
            if (!_viewportInitialized) InitializeViewport();
            UpdateSize();
            InitAnimation();
        }

        private void InitAnimation()
        {
            _zoomAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 1000));
            _zoomAnimation.EasingFunction = new QuarticEase();
            Storyboard.SetTarget(_zoomAnimation, this);
            Storyboard.SetTargetProperty(_zoomAnimation, "Resolution");
            _zoomStoryBoard.Children.Add(_zoomAnimation);
        }

        private void MapControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_viewportInitialized) InitializeViewport();
            Clip = new RectangleGeometry {Rect = new Rect(0, 0, ActualWidth, ActualHeight)};
            UpdateSize();
            _map.ViewChanged(true);
            OnViewChanged();
            Refresh();
        }

        private void UpdateSize()
        {
            if (Viewport == null) return;
            Map.Viewport.Width = ActualWidth;
            Map.Viewport.Height = ActualHeight;
        }

        public void MapDataChanged(object sender, DataChangedEventArgs e)
        {
            if (!Dispatcher.HasThreadAccess)
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => MapDataChanged(sender, e));
            }
            else
            {
                if (e.Cancelled)
                {
                    _errorMessage = "Cancelled";
                    OnErrorMessageChanged(EventArgs.Empty);
                }
                else if (e.Error is System.Net.WebException)
                {
                    _errorMessage = "WebException: " + e.Error.Message;
                    OnErrorMessageChanged(EventArgs.Empty);
                }
                else if (e.Error != null)
                {
                    _errorMessage = e.Error.GetType() + ": " + e.Error.Message;
                    OnErrorMessageChanged(EventArgs.Empty);
                }
                else // no problems
                {
                    RefreshGraphics();
                }
            }
        }

        private void InitializeViewport()
        {
            if (ActualWidth.IsNanOrZero()) return;
            if (_map == null) return;
            if (_map.Envelope == null) return;
            if (_map.Envelope.Width.IsNanOrZero()) return;
            if (_map.Envelope.Height.IsNanOrZero()) return;
            if (_map.Envelope.GetCentroid() == null) return;
 
            if (double.IsNaN(Map.Viewport.Resolution)) 
                Map.Viewport.Resolution = _map.Envelope.Width / ActualWidth;
            if (double.IsNaN(Map.Viewport.Center.X) || double.IsNaN(Map.Viewport.Center.Y)) 
                Map.Viewport.Center = _map.Envelope.GetCentroid();
            
            _viewportInitialized = true;
        }

        void CompositionTarget_Rendering(object sender, object e)
        {
            if (!_viewportInitialized) InitializeViewport();
            if (!_viewportInitialized) return; //stop if the line above failed. 
            if (!_invalid) return;

            if ((_renderer != null) && (_map != null))
            {
                _renderer.Render(Map.Viewport, _map.Layers);
                _renderTarget.Arrange(new Rect(0, 0, Map.Viewport.Width, Map.Viewport.Height));
                _invalid = false;
            }
        }

        public void ZoomToBox(Geometries.Point beginPoint, Geometries.Point endPoint)
        {
            double x, y, resolution;
            var width = Math.Abs(endPoint.X - beginPoint.X);
            var height = Math.Abs(endPoint.Y - beginPoint.Y);
            if (width <= 0) return;
            if (height <= 0) return;

            ZoomHelper.ZoomToBoudingbox(beginPoint.X, beginPoint.Y, endPoint.X, endPoint.Y, ActualWidth, out x, out y, out resolution);
            resolution = ZoomHelper.ClipToExtremes(_map.Resolutions, resolution);

            Map.Viewport.Center = new Geometries.Point(x, y);
            Map.Viewport.Resolution = resolution;

            _map.ViewChanged(true);
            OnViewChanged();
            RefreshGraphics();
            ClearBBoxDrawing();
        }

        private void ClearBBoxDrawing()
        {
            _bboxRect.Margin = new Thickness(0, 0, 0, 0);
            _bboxRect.Width = 0;
            _bboxRect.Height = 0;
        }

        public void ZoomToFullEnvelope()
        {
            if (Map.Envelope == null) return;
            if (ActualWidth.IsNanOrZero()) return;
            Map.Viewport.Resolution =  Map.Envelope.Width / ActualWidth;
            Map.Viewport.Center = Map.Envelope.GetCentroid();

            OnViewChanged();
        }

        private static void OnManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            e.TranslationBehavior.DesiredDeceleration = 25 * 96.0 / (1000.0 * 1000.0);
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
           if (_previousPosition == default(Point) || double.IsNaN(_previousPosition.X))
            {
                _previousPosition = e.Position;
                return;
            }

            // The problem: When you are pinch zooming and you are lifting your hand
            // it is very likely that one finger will leave before the other. The moment after
            // the first finger leaves the center of the pinch suddenly jumps to the position
            // of the last touching finger. Causing a sudden change of the map center.
            // The solution: This hacked up workaround below. When the distance is high
            // but the velocity is low, do not move the map.

            if (Distance(e.Position.X, e.Position.Y, _previousPosition.X, _previousPosition.Y) > 50
                && Math.Sqrt(Math.Pow(e.Velocities.Linear.X, 2.0) + Math.Pow(e.Velocities.Linear.Y, 2.0)) < 1)
            {
                _previousPosition = default(Point);
                return;
            }

            Map.Viewport.Transform(e.Position.X, e.Position.Y, _previousPosition.X, _previousPosition.Y, e.Delta.Scale);

            _previousPosition = e.Position;
            
            _invalid = true;

            OnViewChanged(true);
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2.0) + Math.Pow(y1 - y2, 2.0));
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _previousPosition = default(Point);
            Refresh();
        }
    }

    public class ViewChangedEventArgs : EventArgs
    {
        public IViewport Viewport { get; set; }
        public bool UserAction { get; set; }
    }

    public class MouseInfoEventArgs : EventArgs
    {
        public MouseInfoEventArgs()
        {
            LayerName = string.Empty;
        }

        public string LayerName { get; set; }
        public IFeature Feature { get; set; }
    }

    public class FeatureInfoEventArgs : EventArgs
    {
        public IDictionary<string, IEnumerable<IFeature>> FeatureInfo { get; set; }
    }
}