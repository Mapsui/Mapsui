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
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Utilities;
using Mapsui.Widgets;
using SkiaSharp.Views.UWP;
using HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment;
using VerticalAlignment = Windows.UI.Xaml.VerticalAlignment;

namespace Mapsui.UI.Uwp
{
    public partial class MapControl : Grid, IMapControl
    {
        private readonly IRenderer _renderer;
        private readonly Rectangle _bboxRect = CreateSelectRectangle();
        private readonly SKXamlCanvas _renderTarget = CreateRenderTarget();
        private bool _invalid;
        private Map _map;
        private double _innerRotation;

        public event EventHandler ViewportInitialized;

        public MapControl()
        {
            Background = new SolidColorBrush(Colors.White); // DON'T REMOVE! Touch events do not work without a background

            Children.Add(_renderTarget);
            Children.Add(_bboxRect);

            _renderTarget.PaintSurface += _renderTarget_PaintSurface;

            Map = new Map();

            Loaded += MapControlLoaded;

            SizeChanged += MapControlSizeChanged;
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            _renderer = new MapRenderer();
            _scale = GetSkiaScale();

            PointerWheelChanged += MapControl_PointerWheelChanged;

            ManipulationMode = ManipulationModes.Scale | ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Rotate;
            ManipulationDelta += OnManipulationDelta;
            ManipulationCompleted += OnManipulationCompleted;
            ManipulationInertiaStarting += OnManipulationInertiaStarting;

            Tapped += OnSingleTapped;
            DoubleTapped += OnDoubleTapped;

            var orientationSensor = SimpleOrientationSensor.GetDefault();
            if (orientationSensor != null)
                orientationSensor.OrientationChanged += (sender, args) =>
                    Task.Run(() => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Refresh))
                        .ConfigureAwait(false);
        }

        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var tabPosition = e.GetPosition(this).ToMapsui();
            Map.InvokeInfo(tabPosition, tabPosition, _scale, _renderer.SymbolCache, WidgetTouched, 2);
        }

        private void OnSingleTapped(object sender, TappedRoutedEventArgs e)
        {
            var tabPosition = e.GetPosition(this).ToMapsui();
            Map.InvokeInfo(tabPosition, tabPosition, _scale, _renderer.SymbolCache, WidgetTouched, 1);
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
            get => _map;
            set
            {
                if (_map != null)
                {
                    UnsubscribeFromMapEvents(_map);
                    _map = null;

                }

                _map = value;

                if (_map != null)
                {
                    SubscribeToMapEvents(_map);

                    _map.ViewChanged(true);
                }

                RefreshGraphics();
            }
        }

        private void MapRefreshGraphics(object o, EventArgs eventArgs)
        {
            RefreshGraphics();
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
        }

        private void MapControl_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (ZoomLocked) return;
            if (!_map.Viewport.Initialized) return;

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
            if (mouseWheelDelta > 0)
            {
                var resolution = ZoomHelper.ZoomIn(_map.Resolutions, currentResolution);

                return ViewportLimiter.LimitResolution(resolution, _map.Viewport.Width, _map.Viewport.Height,
                    _map.ZoomMode, _map.ZoomLimits, _map.Resolutions, _map.Envelope);
            }
            if (mouseWheelDelta < 0)
            {
                var resolution = ZoomHelper.ZoomOut(_map.Resolutions, currentResolution);

                return ViewportLimiter.LimitResolution(resolution, _map.Viewport.Width, _map.Viewport.Height,
                    _map.ZoomMode, _map.ZoomLimits, _map.Resolutions, _map.Envelope);
            }
            return currentResolution;
        }

        private void OnViewChanged(bool userAction = false)
        {
            if (_map != null)
                ViewChanged?.Invoke(this, new ViewChangedEventArgs { Viewport = Map.Viewport, UserAction = userAction });
        }

        public void Refresh()
        {
            RefreshData();
            RefreshGraphics();
        }

        public void RefreshGraphics()
        {
            InvalidateCanvas();
            _invalid = true;
        }

        public void RefreshData()
        {
            _map.ViewChanged(true);
        }

        internal void InvalidateCanvas()
        {
            InvalidateArrange();
            InvalidateMeasure();
            _renderTarget.InvalidateArrange();
            _renderTarget.InvalidateMeasure();
        }

        public void Clear()
        {
            _map?.ClearCache();
            RefreshGraphics();
        }

        public void ZoomIn()
        {
            if (ZoomLocked) return;
            if (!_map.Viewport.Initialized) return;

            Map.Viewport.Resolution = ZoomHelper.ZoomIn(_map.Resolutions, Map.Viewport.Resolution);

            OnViewChanged();
        }

        public void ZoomOut()
        {
            if (ZoomLocked) return;
            if (!_map.Viewport.Initialized) return;

            Map.Viewport.Resolution = ZoomHelper.ZoomOut(_map.Resolutions, Map.Viewport.Resolution);

            OnViewChanged();
        }

        protected void OnErrorMessageChanged(EventArgs e)
        {
            ErrorMessageChanged?.Invoke(this, e);
        }

        private void MapControlLoaded(object sender, RoutedEventArgs e)
        {
            TryInitializeViewport();
            UpdateSize();
        }
        
        private void MapControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            TryInitializeViewport();
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
                try
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
                catch (Exception exception)
                {
                    Logger.Log(LogLevel.Warning, "Unexpected exception in MapDataChanged", exception);
                }
            }
        }

        private void CompositionTarget_Rendering(object sender, object e)
        {
            _renderTarget.Invalidate();
        }

        private void _renderTarget_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (_renderer == null) return;
            if (_map == null) return;
            if (!_invalid) return;

            TryInitializeViewport();
            if (!_map.Viewport.Initialized) return;

            e.Surface.Canvas.Scale(_scale, _scale);
            _renderer.Render(e.Surface.Canvas, Map.Viewport, _map.Layers, _map.Widgets, _map.BackColor);
            _renderTarget.Arrange(new Rect(0, 0, Map.Viewport.Width, Map.Viewport.Height));
            _invalid = false;

        }

        public void ZoomToBox(Geometries.Point beginPoint, Geometries.Point endPoint)
        {
            var width = Math.Abs(endPoint.X - beginPoint.X);
            var height = Math.Abs(endPoint.Y - beginPoint.Y);
            if (width <= 0) return;
            if (height <= 0) return;

            ZoomHelper.ZoomToBoudingbox(
                beginPoint.X, beginPoint.Y, endPoint.X, endPoint.Y,
                Map.Viewport.Width, Map.Viewport.Height,
                out var x, out var y, out var resolution);

            resolution = ViewportLimiter.LimitResolution(resolution, _map.Viewport.Width, _map.Viewport.Height, _map.ZoomMode, _map.ZoomLimits,
                _map.Resolutions, _map.Envelope);

            _map.Viewport.Resolution = resolution;

            _map.Viewport.Center = new Geometries.Point(x, y);


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
            var (center, radius, angle) = (e.Position.ToMapsui(), e.Delta.Scale, e.Delta.Rotation);
            var (prevCenter, prevRadius, prevAngle) = (e.Position.ToMapsui().Offset(-e.Delta.Translation.X, -e.Delta.Translation.Y), 1f, 0f);

            double rotationDelta = 0;

            if (RotationLock)
            {
                _innerRotation += angle - prevAngle;
                _innerRotation %= 360;

                if (_innerRotation > 180)
                    _innerRotation -= 360;
                else if (_innerRotation < -180)
                    _innerRotation += 360;

                if (_map.Viewport.Rotation == 0 && Math.Abs(_innerRotation) >= Math.Abs(UnSnapRotationDegrees))
                    rotationDelta = _innerRotation;
                else if (_map.Viewport.Rotation != 0)
                {
                    if (Math.Abs(_innerRotation) <= Math.Abs(ReSnapRotationDegrees))
                        rotationDelta = -_map.Viewport.Rotation;
                    else
                        rotationDelta = _innerRotation - _map.Viewport.Rotation;
                }
            }

            _map.Viewport.Transform(center.X, center.Y, prevCenter.X, prevCenter.Y, radius / prevRadius, rotationDelta);

            ViewportLimiter.Limit(_map.Viewport, _map.ZoomMode, _map.ZoomLimits, _map.Resolutions,
                _map.PanMode, _map.PanLimits, _map.Envelope);

            _invalid = true;
            OnViewChanged(true);
            e.Handled = true;
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Refresh();
        }

        private void TryInitializeViewport()
        {
            if (_map.Viewport.Initialized) return;

            if (_map.Viewport.TryInitializeViewport(_map, ActualWidth, ActualHeight))
            {
                Map.ViewChanged(true);
                OnViewportInitialized();
            }
        }

        private void OnViewportInitialized()
        {
            ViewportInitialized?.Invoke(this, EventArgs.Empty);
        }

        private float GetSkiaScale()
        {
            var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            return (float)scaleFactor;
        }

        private void WidgetTouched(IWidget widget, Geometries.Point screenPosition)
        {
            Task.Run(() => Launcher.LaunchUriAsync(new Uri(((Hyperlink)widget).Url)));

            widget.HandleWidgetTouched(screenPosition);
        }
    }
}