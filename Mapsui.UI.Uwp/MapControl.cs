// Copyright 2008 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA f

using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using SkiaSharp.Views.UWP;
using Point = Windows.Foundation.Point;

namespace Mapsui.UI.Uwp
{
    public class MapControl : Grid, IMapControl
    {
        private readonly IRenderer _renderer;
        private readonly Rectangle _bboxRect = CreateSelectRectangle();
        private readonly SKXamlCanvas _renderTarget = CreateRenderTarget();
        private readonly AttributionPanel _attributionPanel = CreateAttributionPanel();
        private readonly DoubleAnimation _zoomAnimation = new DoubleAnimation();
        private readonly Storyboard _zoomStoryBoard = new Storyboard();
        private bool _invalid;
        private Map _map;
        private Point _previousPosition;
        private bool _viewportInitialized;
        private Geometries.Point _skiaScale;

        public event EventHandler ViewportInitialized;

        public MapControl()
        {
            Background = new SolidColorBrush(Colors.White); // DON'T REMOVE! Touch events do not work without a background

            Children.Add(_renderTarget);
            Children.Add(_bboxRect);
            Children.Add(_attributionPanel);

            _renderTarget.PaintSurface += _renderTarget_PaintSurface;
                        
            Map = new Map();

            Loaded += MapControlLoaded;

            SizeChanged += MapControlSizeChanged;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            _renderer = new MapRenderer();
            PointerWheelChanged += MapControl_PointerWheelChanged;

            ManipulationMode = ManipulationModes.Scale | ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            ManipulationDelta += OnManipulationDelta;
            ManipulationCompleted += OnManipulationCompleted;
            ManipulationInertiaStarting += OnManipulationInertiaStarting;

            var orientationSensor = SimpleOrientationSensor.GetDefault();
            if (orientationSensor != null)
                orientationSensor.OrientationChanged += (sender, args) =>
                    Task.Run(() => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Refresh))
                        .ConfigureAwait(false);
        }

        private static Rectangle CreateSelectRectangle()
        {
            return new Rectangle
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
        }

        private static AttributionPanel CreateAttributionPanel()
        {
            return new AttributionPanel
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right
            };
        }

        private static SKXamlCanvas CreateRenderTarget()
        {
            return new SKXamlCanvas
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(Colors.Transparent)
            };
        }

        public bool ZoomToBoxMode { get; set; }
        
        public Map Map
        {
            get { return _map; }
            set
            {
                if (_map != null)
                {
                    var temp = _map;
                    _map = null;
                    temp.DataChanged -= MapDataChanged;
                    temp.PropertyChanged -= MapPropertyChanged;
                    temp.Dispose();
                }

                _map = value;

                if (_map != null)
                {
                    _viewportInitialized = false;
                    _map.DataChanged += MapDataChanged;
                    _map.PropertyChanged += MapPropertyChanged;
                    _map.ViewChanged(true);
                    _attributionPanel.Populate(Map.Layers);
                }

                RefreshGraphics();
            }
        }

        public string ErrorMessage { get; private set; }

        public bool ZoomLocked { get; set; }

        public event EventHandler ErrorMessageChanged;
        public event EventHandler<ViewChangedEventArgs> ViewChanged;

        private async void MapPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => MapPropertyChanged(e));
        }

        private void MapPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Layer.Enabled))
            {
                RefreshGraphics();
            }
            else if (e.PropertyName == nameof(Layer.Opacity))
            {
                RefreshGraphics();
            }
            else if (e.PropertyName == nameof(Map.Layers))
            {
                _attributionPanel.Populate(Map.Layers);
            }
        }

        private void MapControl_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (ZoomLocked) return;
            if (!_viewportInitialized) return;

            var currentPoint = e.GetCurrentPoint(this);
            //Needed for both MouseMove and MouseWheel event for mousewheel event

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

            RefreshGraphics();
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
                ViewChanged?.Invoke(this, new ViewChangedEventArgs { Viewport = Map.Viewport, UserAction = userAction });
        }

        public void Refresh()
        {
            _map.ViewChanged(true);
            RefreshGraphics();
        }

        public void RefreshGraphics()
        {
            InvalidateArrange();
            InvalidateMeasure();
            _renderTarget.InvalidateArrange();
            _renderTarget.InvalidateMeasure();
            _invalid = true;
        }

        public void Clear()
        {
            _map?.ClearCache();
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
            ErrorMessageChanged?.Invoke(this, e);
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
            Storyboard.SetTargetProperty(_zoomAnimation, nameof(Map.Viewport.Resolution));
            _zoomStoryBoard.Children.Add(_zoomAnimation);
        }

        private void MapControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_viewportInitialized) InitializeViewport();
            Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };
            UpdateSize();
            _map.ViewChanged(true);
            OnViewChanged();
            Refresh();
        }

        private void UpdateSize()
        {
            if (Map.Viewport == null) return;
            Map.Viewport.Width = ActualWidth;
            Map.Viewport.Height = ActualHeight;
        }

        public void MapDataChanged(object sender, DataChangedEventArgs e)
        {
            if (!Dispatcher.HasThreadAccess)
            {
                Task.Run(() => Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal, () => MapDataChanged(sender, e)))
                    .ConfigureAwait(false);
            }
            else
            {
                if (e.Cancelled)
                {
                    ErrorMessage = "Cancelled";
                    OnErrorMessageChanged(EventArgs.Empty);
                }
                else if (e.Error is WebException)
                {
                    ErrorMessage = "WebException: " + e.Error.Message;
                    OnErrorMessageChanged(EventArgs.Empty);
                }
                else if (e.Error != null)
                {
                    ErrorMessage = e.Error.GetType() + ": " + e.Error.Message;
                    OnErrorMessageChanged(EventArgs.Empty);
                }
                else // no problems
                {
                    RefreshGraphics();
                }
            }
        }

        private void CompositionTarget_Rendering(object sender, object e)
        {
            _renderTarget.Invalidate();
        }


        private void _renderTarget_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (!_viewportInitialized) InitializeViewport();
            if (!_viewportInitialized) return; //stop if the line above failed. 
            if (!_invalid) return;

            if ((_renderer != null) && (_map != null))
            {
                if (_skiaScale == null) _skiaScale = GetSkiaScale();
                e.Surface.Canvas.Scale((float)_skiaScale.X, (float)_skiaScale.Y);
                _renderer.Render(e.Surface.Canvas, Map.Viewport, _map.Layers, _map.BackColor);
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

            ZoomHelper.ZoomToBoudingbox(beginPoint.X, beginPoint.Y, endPoint.X, endPoint.Y, ActualWidth, out x, out y,
                out resolution);
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
            Map.Viewport.Resolution = Map.Envelope.Width / ActualWidth;
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
            Map.InvokeInfo(e.Position.ToMapsui());
        }

        private void InitializeViewport()
        {
            if (ViewportHelper.TryInitializeViewport(_map, ActualWidth, ActualHeight))
            {
                _viewportInitialized = true;
                Map.ViewChanged(true);
                OnViewportInitialized();
            }
        }

        private void OnViewportInitialized()
        {
            ViewportInitialized?.Invoke(this, EventArgs.Empty);
        }

        private Geometries.Point GetSkiaScale()
        {
            var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            return new Geometries.Point(scaleFactor, scaleFactor);
        }
    }
}