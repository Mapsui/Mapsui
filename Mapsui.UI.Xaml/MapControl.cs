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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Rendering.Xaml;
using Mapsui.Utilities;
#if !SILVERLIGHT && !WINDOWS_PHONE
using XamlVector = System.Windows.Vector;
#else
using XamlVector = System.Windows.Point;
#endif

namespace Mapsui.UI.Xaml
{
    public class MapControl : Grid
    {
        #region Fields
        private Map _map;
        private readonly Viewport _viewport = new Viewport { Center = { X = double.NaN, Y = double.NaN }, Resolution = double.NaN };
        private Point _previousMousePosition;
        private Point _currentMousePosition;
        private Point _downMousePosition;
        private string _errorMessage;
        private readonly FpsCounter _fpsCounter = new FpsCounter();
        private readonly DoubleAnimation _zoomAnimation = new DoubleAnimation();
        private readonly Storyboard _zoomStoryBoard = new Storyboard();
        private double _toResolution = double.NaN;
        private bool _mouseDown;
        private bool _viewportInitialized;
        private readonly Canvas _renderCanvas = new Canvas();
        private bool _invalid;
        private readonly Rectangle _bboxRect;
        #endregion

        #region Events
        public event EventHandler ErrorMessageChanged;
        public event EventHandler<ViewChangedEventArgs> ViewChanged;
        public event EventHandler<MouseInfoEventArgs> MouseInfoOver;
        public event EventHandler MouseInfoLeave;
        public event EventHandler<MouseInfoEventArgs> MouseInfoUp;
        public event EventHandler<FeatureInfoEventArgs> FeatureInfo;
        #endregion

        public IRenderer Renderer { get; set; }
        private bool IsInBoxZoomMode { get; set; }
        public IList<ILayer> MouseInfoOverLayers { get; private set; }
        public IList<ILayer> MouseInfoUpLayers { get; private set; }

        public bool ZoomToBoxMode { get; set; }
        public Viewport Viewport { get { return _viewport; } }

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
                    if (!double.IsNaN(_viewport.RenderResolution)) _map.ViewChanged(true, _viewport.Extent, _viewport.RenderResolution);
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
                _map.ViewChanged(true, _viewport.Extent, _viewport.RenderResolution);
            }
        }

        public FpsCounter FpsCounter
        {
            get
            {
                return _fpsCounter;
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

        public Canvas RenderCanvas
        {
            get { return _renderCanvas; }
        }
        
        private static readonly DependencyProperty ResolutionProperty =
          DependencyProperty.Register(
          "Resolution", typeof(double), typeof(MapControl),
          new PropertyMetadata(OnResolutionChanged));
        
        public MapControl()
        {
            _renderCanvas = new Canvas
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = new SolidColorBrush(Colors.Transparent),
                };
            Children.Add(RenderCanvas);

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
            MouseInfoOverLayers = new List<ILayer>();
            MouseInfoUpLayers = new List<ILayer>();
            Loaded += MapControlLoaded;
            KeyDown += MapControlKeyDown;
            KeyUp += MapControlKeyUp;
            MouseLeftButtonDown += MapControlMouseLeftButtonDown;
            MouseLeftButtonUp += MapControlMouseLeftButtonUp;
#if (!WINDOWS_PHONE) //turn off mouse controls
            MouseMove += MapControlMouseMove;
            MouseLeave += MapControlMouseLeave;
            MouseWheel += MapControlMouseWheel;
#endif
            SizeChanged += MapControlSizeChanged;
            CompositionTarget.Rendering += CompositionTargetRendering;
            Renderer = new MapRenderer(RenderCanvas);
            
#if (!SILVERLIGHT && !WINDOWS_PHONE)
            ManipulationDelta += OnManipulationDelta;
            ManipulationCompleted += OnManipulationCompleted;
            ManipulationInertiaStarting += OnManipulationInertiaStarting;
            Dispatcher.ShutdownStarted += DispatcherShutdownStarted;
            IsManipulationEnabled = true;
#elif WINDOWS_PHONE
           _renderCanvas.ManipulationDelta += OnManipulationDelta;
           _renderCanvas.ManipulationCompleted += OnManipulationCompleted;
#endif
        }

        #region Public methods

        public void OnViewChanged()
        {
            OnViewChanged(false);
        }

        private void OnViewChanged(bool userAction)
        {
            if (_map == null) return;

            if (ViewChanged != null)
            {
                ViewChanged(this, new ViewChangedEventArgs { Viewport = _viewport, UserAction = userAction });
            }
        }

        public void Refresh()
        {
            _map.ViewChanged(true, _viewport.Extent, _viewport.RenderResolution);
            RefreshGraphics();
        }

        private void RefreshGraphics() //should be private soon
        {
#if (!SILVERLIGHT && !WINDOWS_PHONE)
            InvalidateVisual();
#endif
            InvalidateArrange();
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
            if (ZoomLocked)
                return;

            if (double.IsNaN(_toResolution))
                _toResolution = _viewport.Resolution;

            _toResolution = ZoomHelper.ZoomIn(_map.Resolutions, _toResolution);
            ZoomMiddle();
        }

        public void ZoomOut()
        {
            if (double.IsNaN(_toResolution))
                _toResolution = _viewport.Resolution;

            _toResolution = ZoomHelper.ZoomOut(_map.Resolutions, _toResolution);
            ZoomMiddle();
        }

        #endregion

        #region Protected and private methods

        protected void OnErrorMessageChanged(EventArgs e)
        {
            if (ErrorMessageChanged != null)
            {
                ErrorMessageChanged(this, e);
            }
        }

        private static void OnResolutionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var newResolution = (double)e.NewValue;
            ((MapControl)dependencyObject).ZoomToResolution(newResolution);
        }

        private void ZoomToResolution(double resolution)
        {
            var current = _currentMousePosition;

            _viewport.Transform(current.X, current.Y, current.X, current.Y, _viewport.Resolution / resolution);

            _map.ViewChanged(true, _viewport.Extent, _viewport.RenderResolution);
            OnViewChanged();
            RefreshGraphics();
        }

        private void ZoomMiddle()
        {
            _currentMousePosition = new Point(ActualWidth / 2, ActualHeight / 2);
            StartZoomAnimation(_viewport.Resolution, _toResolution);
        }

        private void MapControlLoaded(object sender, RoutedEventArgs e)
        {
            if (!_viewportInitialized) InitializeViewport();
            UpdateSize();
            InitAnimation();

#if (!SILVERLIGHT && !WINDOWS_PHONE)
            Focusable = true;
#endif
        }

        private void InitAnimation()
        {
            _zoomAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 1000));
            _zoomAnimation.EasingFunction = new QuarticEase();
            Storyboard.SetTarget(_zoomAnimation, this);
            Storyboard.SetTargetProperty(_zoomAnimation, new PropertyPath("Resolution"));
            _zoomStoryBoard.Children.Add(_zoomAnimation);
        }

        private void MapControlMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!_viewportInitialized) return;
            if (ZoomLocked) return;

            _currentMousePosition = e.GetPosition(this); //Needed for both MouseMove and MouseWheel event for mousewheel event

            if (double.IsNaN(_toResolution))
            {
                _toResolution = _viewport.Resolution;
            }

            if (e.Delta > 0)
            {
                _toResolution = ZoomHelper.ZoomIn(_map.Resolutions, _toResolution);
            }
            else if (e.Delta < 0)
            {
                _toResolution = ZoomHelper.ZoomOut(_map.Resolutions, _toResolution);
            }

            e.Handled = true; //so that the scroll event is not sent to the html page.

            // Some cheating for personal gain. This workaround could be ommitted if the zoom animations was on CenterX, CenterY and Resolution, not Resolution alone.
            _viewport.Center.X += 0.000000001;
            _viewport.Center.Y += 0.000000001;

            StartZoomAnimation(_viewport.Resolution, _toResolution);
        }

        private void StartZoomAnimation(double begin, double end)
        {
            _zoomStoryBoard.Pause(); //using Stop() here causes unexpected results while zooming very fast.
            _zoomAnimation.From = begin;
            _zoomAnimation.To = end;
            _zoomAnimation.Completed += ZoomAnimationCompleted;
            _zoomStoryBoard.Begin();
        }

        private void ZoomAnimationCompleted(object sender, EventArgs e)
        {
            _toResolution = double.NaN;
        }

        private void MapControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_viewportInitialized) InitializeViewport();
            Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };
            UpdateSize();
            _map.ViewChanged(true, _viewport.Extent, _viewport.RenderResolution);
            OnViewChanged();
            Refresh();
        }

        private void UpdateSize()
        {
            if (_viewport != null)
            {
                _viewport.Width = ActualWidth;
                _viewport.Height = ActualHeight;
            }
        }

        private void MapControlMouseLeave(object sender, MouseEventArgs e)
        {
            _previousMousePosition = new Point();
            ReleaseMouseCapture();
        }

        public void MapDataChanged(object sender, DataChangedEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new DataChangedEventHandler(MapDataChanged), new[] { sender, e });
            }
            else
            {
                if (e == null)
                {
                    _errorMessage = "Unexpected error: DataChangedEventArgs can not be null";
                    OnErrorMessageChanged(EventArgs.Empty);
                }
                else if (e.Cancelled)
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

        private void MapControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
#if !SILVERLIGHT
            if (e.StylusDevice != null) return;
#endif
            _previousMousePosition = e.GetPosition(this);
            _downMousePosition = e.GetPosition(this);
            _mouseDown = true;
            CaptureMouse();
        }

        private void MapControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
#if !SILVERLIGHT
            if (e.StylusDevice != null) return;
#endif
            if (IsInBoxZoomMode || ZoomToBoxMode)
            {
                ZoomToBoxMode = false;
                Geometries.Point previous = Viewport.ScreenToWorld(_previousMousePosition.X, _previousMousePosition.Y);
                Geometries.Point current = Viewport.ScreenToWorld(e.GetPosition(this).X, e.GetPosition(this).Y);
                ZoomToBox(previous, current);
            }
            else
            {
                HandleFeatureInfo(e);
                var eventArgs = GetMouseInfoEventArgs(e.GetPosition(this), MouseInfoUpLayers);
                OnMouseInfoUp(eventArgs ?? new MouseInfoEventArgs());
            }

            _map.ViewChanged(true, _viewport.Extent, _viewport.RenderResolution);
            OnViewChanged(true);
            _mouseDown = false;

            _previousMousePosition = new Point();
            ReleaseMouseCapture();
        }

        private void HandleFeatureInfo(MouseButtonEventArgs e)
        {
            if (FeatureInfo == null) return; // don't fetch if you the call back is not set.

            if (_downMousePosition == e.GetPosition(this))
            {
                foreach (var layer in Map.Layers)
                {
                    if (layer is IFeatureInfo)
                    {
                        (layer as IFeatureInfo).GetFeatureInfo(_viewport, _downMousePosition.X, _downMousePosition.Y, OnFeatureInfo);
                    }
                }
            }
        }

        private void OnFeatureInfo(IDictionary<string, IEnumerable<IFeature>> features)
        {
            if (FeatureInfo != null)
            {
                FeatureInfo(this, new FeatureInfoEventArgs { FeatureInfo = features });
            }
        }

        private void MapControlMouseMove(object sender, MouseEventArgs e)
        {
#if !SILVERLIGHT
            if (e.StylusDevice != null) return;
#endif
            if (IsInBoxZoomMode || ZoomToBoxMode)
            {
                DrawBbox(e.GetPosition(this));
                return;
            }

            if (!_mouseDown) RaiseMouseInfoEvents(e.GetPosition(this));

            if (_mouseDown)
            {
                if (_previousMousePosition == default(Point))
                
                {
                    return; // It turns out that sometimes MouseMove+Pressed is called before MouseDown
                }

                _currentMousePosition = e.GetPosition(this); //Needed for both MouseMove and MouseWheel event
                _viewport.Transform(_currentMousePosition.X, _currentMousePosition.Y, _previousMousePosition.X, _previousMousePosition.Y);
                _previousMousePosition = _currentMousePosition;
                _map.ViewChanged(false, _viewport.Extent, _viewport.RenderResolution);
                OnViewChanged(true);
                RefreshGraphics();
            }
        }

        private void RaiseMouseInfoEvents(Point mousePosition)
        {
            var mouseEventArgs = GetMouseInfoEventArgs(mousePosition, MouseInfoOverLayers);
            if (mouseEventArgs == null) OnMouseInfoLeave();
            else OnMouseInfoOver(mouseEventArgs);
        }

        private MouseInfoEventArgs GetMouseInfoEventArgs(Point mousePosition, IEnumerable<ILayer> layers)
        {
            var margin = 16 * Viewport.Resolution;
            var point = Viewport.ScreenToWorld(new Geometries.Point(mousePosition.X, mousePosition.Y));

            foreach (var layer in layers)
            {
                var feature = layer.GetFeaturesInView(Map.Envelope, 0)
                    .Where(f => f.Geometry.GetBoundingBox().GetCentroid().Distance(point) < margin)
                    .OrderBy(f => f.Geometry.GetBoundingBox().GetCentroid().Distance(point))
                    .FirstOrDefault();

                if (feature != null)
                {
                    return new MouseInfoEventArgs { LayerName = layer.LayerName, Feature = feature };
                }
            }
            return null;
        }

        protected void OnMouseInfoLeave()
        {
            if (MouseInfoLeave != null)
            {
                MouseInfoLeave(this, new EventArgs());
            }
        }

        protected void OnMouseInfoOver(MouseInfoEventArgs e)
        {
            if (MouseInfoOver != null)
            {
                MouseInfoOver(this, e);
            }
        }

        protected void OnMouseInfoUp(MouseInfoEventArgs e)
        {
            if (MouseInfoUp != null)
            {
                MouseInfoUp(this, e);
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

            if (double.IsNaN(_viewport.Resolution))
                _viewport.Resolution = _map.Envelope.Width / ActualWidth;
            if (double.IsNaN(_viewport.Center.X) || double.IsNaN(_viewport.Center.Y))
                _viewport.Center = _map.Envelope.GetCentroid();

            _viewport.Width = ActualWidth;
            _viewport.Height = ActualHeight;

            _viewport.RenderResolutionMultiplier = 1.0;

            _viewportInitialized = true;
        }

        private void CompositionTargetRendering(object sender, EventArgs e)
        {
            if (!_viewportInitialized) InitializeViewport();
            if (!_viewportInitialized) return; //stop if the line above failed. 
            if (!_invalid) return;

            if ((Renderer != null) && (_map != null))
            {
                Renderer.Render(_viewport, _map.Layers);
                _fpsCounter.FramePlusOne();
                _invalid = false;
            }
        }

#if !SILVERLIGHT
        private void DispatcherShutdownStarted(object sender, EventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTargetRendering;
            if (_map != null)
            {
                _map.Dispose();
            }
        }
#endif

        #endregion

        #region Bbox zoom

        public void ZoomToBox(Geometries.Point beginPoint, Geometries.Point endPoint)
        {
            double x, y, resolution;
            var width = Math.Abs(endPoint.X - beginPoint.X);
            var height = Math.Abs(endPoint.Y - beginPoint.Y);
            if (width <= 0) return;
            if (height <= 0) return;

            ZoomHelper.ZoomToBoudingbox(beginPoint.X, beginPoint.Y, endPoint.X, endPoint.Y, ActualWidth, out x, out y, out resolution);
            resolution = ZoomHelper.ClipToExtremes(_map.Resolutions, resolution);

            _viewport.Center = new Geometries.Point(x, y);
            _viewport.Resolution = resolution;
            _toResolution = resolution;

            _map.ViewChanged(true, _viewport.Extent, _viewport.RenderResolution);
            OnViewChanged(true);
            RefreshGraphics();
            ClearBBoxDrawing();
        }

        private void ClearBBoxDrawing()
        {
            _bboxRect.Margin = new Thickness(0, 0, 0, 0);
            _bboxRect.Width = 0;
            _bboxRect.Height = 0;
        }

        private void MapControlKeyUp(object sender, KeyEventArgs e)
        {
            String keyName = e.Key.ToString().ToLower();
            if (keyName.Equals("ctrl") || keyName.Equals("leftctrl") || keyName.Equals("rightctrl"))
            {
                IsInBoxZoomMode = false;
            }
        }

        private void MapControlKeyDown(object sender, KeyEventArgs e)
        {
            String keyName = e.Key.ToString().ToLower();
            if (keyName.Equals("ctrl") || keyName.Equals("leftctrl") || keyName.Equals("rightctrl"))
            {
                IsInBoxZoomMode = true;
            }
        }

        private void DrawBbox(Point newPos)
        {
            if (_mouseDown)
            {
                Point from = _previousMousePosition;
                Point to = newPos;

                if (from.X > to.X)
                {
                    Point temp = from;
                    from.X = to.X;
                    to.X = temp.X;
                }

                if (from.Y > to.Y)
                {
                    Point temp = from;
                    from.Y = to.Y;
                    to.Y = temp.Y;
                }

                _bboxRect.Width = to.X - from.X;
                _bboxRect.Height = to.Y - from.Y;
                _bboxRect.Margin = new Thickness(from.X, from.Y, 0, 0);
            }
        }

        #endregion

        public void ZoomToFullEnvelope()
        {
            if (Map.Envelope == null) return;
            if (ActualWidth.IsNanOrZero()) return;
            _viewport.Resolution = Map.Envelope.Width / ActualWidth;
            _viewport.Center = Map.Envelope.GetCentroid();
        }

        #region WPF4 Touch Support

#if (!SILVERLIGHT && !WINDOWS_PHONE)

        private static void OnManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            e.TranslationBehavior.DesiredDeceleration = 25 * 96.0 / (1000.0 * 1000.0);
        }

#endif
        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            var previousX = e.ManipulationOrigin.X;
            var previousY = e.ManipulationOrigin.Y;
            var currentX = e.ManipulationOrigin.X + e.DeltaManipulation.Translation.X;
            var currentY = e.ManipulationOrigin.Y + e.DeltaManipulation.Translation.Y;
            var deltaScale = GetDeltaScale(e.DeltaManipulation.Scale);

            _viewport.Transform(currentX, currentY, previousX, previousY, deltaScale);

            _invalid = true;
            OnViewChanged(true);
            e.Handled = true;
        }

        private double GetDeltaScale(XamlVector scale)
        {
            if (ZoomLocked) return 1;
            var deltaScale = (scale.X + scale.Y) / 2;
            if (Math.Abs(deltaScale) < Constants.Epsilon) return 1; // If there is no scaling the deltaScale will be 0.0 in Windows Phone (while it is 1.0 in wpf)
            if (!(Math.Abs(deltaScale - 1d) > Constants.Epsilon)) return 1;
            return deltaScale;
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            Refresh();
        }

        #endregion
    }

    public class ViewChangedEventArgs : EventArgs
    {
        public Viewport Viewport { get; set; }
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