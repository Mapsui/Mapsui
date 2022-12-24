using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Rendering.Skia;
using Mapsui.UI.Utils;
using Mapsui.UI.Wpf.Extensions;
using Mapsui.Utilities;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Point = System.Windows.Point;
using VerticalAlignment = System.Windows.VerticalAlignment;
using XamlVector = System.Windows.Vector;

namespace Mapsui.UI.Wpf
{
    public partial class MapControl : Grid, IMapControl, IDisposable
    {
        private readonly Rectangle _selectRectangle = CreateSelectRectangle();
        private MPoint? _currentMousePosition;
        private MPoint? _downMousePosition;
        private bool _mouseDown;
        private MPoint? _previousMousePosition;
        private bool _hasBeenManipulated;
        private double _virtualRotation;
        private readonly FlingTracker _flingTracker = new();

        public MouseWheelAnimation MouseWheelAnimation { get; } = new();

        /// <summary>
        /// Fling is called, when user release mouse button or lift finger while moving with a certain speed, higher than speed of swipe 
        /// </summary>
        public event EventHandler<SwipedEventArgs>? Fling;

        public MapControl()
        {
            CommonInitialize();
            Initialize();
        }

        private void Initialize()
        {
            _invalidate = () => {
                if (Dispatcher.CheckAccess()) InvalidateCanvas();
                else RunOnUIThread(InvalidateCanvas);
            };

            Children.Add(WpfCanvas);
            Children.Add(SkiaCanvas);
            Children.Add(_selectRectangle);

            SkiaCanvas.PaintSurface += SKElementOnPaintSurface;

            Loaded += MapControlLoaded;
            MouseLeftButtonDown += MapControlMouseLeftButtonDown;
            MouseLeftButtonUp += MapControlMouseLeftButtonUp;

            TouchUp += MapControlTouchUp;

            MouseMove += MapControlMouseMove;
            MouseLeave += MapControlMouseLeave;
            MouseWheel += MapControlMouseWheel;

            SizeChanged += MapControlSizeChanged;

            ManipulationStarted += OnManipulationStarted;
            ManipulationDelta += OnManipulationDelta;
            ManipulationCompleted += OnManipulationCompleted;
            ManipulationInertiaStarting += OnManipulationInertiaStarting;

            IsManipulationEnabled = true;

            WpfCanvas.Visibility = Visibility.Collapsed;
            SkiaCanvas.Visibility = Visibility.Visible;
            Renderer = new MapRenderer();
            RefreshGraphics();
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

        public Canvas WpfCanvas { get; } = CreateWpfRenderCanvas();

        private SKElement SkiaCanvas { get; } = CreateSkiaRenderElement();

        private static Canvas CreateWpfRenderCanvas()
        {
            return new Canvas
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
        }

        private static SKElement CreateSkiaRenderElement()
        {
            return new SKElement
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
        }

        public event EventHandler<FeatureInfoEventArgs>? FeatureInfo; // todo: Remove and add sample for alternative

        internal void InvalidateCanvas()
        {
            SkiaCanvas.InvalidateVisual();
        }

        private void MapControlLoaded(object sender, RoutedEventArgs e)
        {
            SetViewportSize();

            Focusable = true;
        }

        private void MapControlMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_map?.ZoomLock ?? true) return;
            if (!Viewport.HasSize()) return;

            _currentMousePosition = e.GetPosition(this).ToMapsui();

            var resolution = MouseWheelAnimation.GetResolution(e.Delta, _viewport, _map);
            // Limit target resolution before animation to avoid an animation that is stuck on the max resolution, which would cause a needless delay
            resolution = _map.Limiter.LimitResolution(resolution, Viewport.Width, Viewport.Height, _map.Resolutions, _map.Extent);
            Navigator?.ZoomTo(resolution, _currentMousePosition, MouseWheelAnimation.Duration, MouseWheelAnimation.Easing);
        }

        private void MapControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };
            SetViewportSize();
        }

        private void MapControlMouseLeave(object sender, MouseEventArgs e)
        {
            _previousMousePosition = null;
            ReleaseMouseCapture();
        }

        private void RunOnUIThread(Action action)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        private void MapControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var touchPosition = e.GetPosition(this).ToMapsui();
            _previousMousePosition = touchPosition;
            _downMousePosition = touchPosition;
            _mouseDown = true;
            _flingTracker.Clear();
            CaptureMouse();

        }

        private static bool IsInBoxZoomMode()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        }

        private void MapControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(this).ToMapsui();

            if (_previousMousePosition != null)
            {
                if (IsInBoxZoomMode())
                {
                    var previous = Viewport.ScreenToWorld(_previousMousePosition.X, _previousMousePosition.Y);
                    var current = Viewport.ScreenToWorld(mousePosition.X, mousePosition.Y);
                    ZoomToBox(previous, current);
                }
                else if (_downMousePosition != null && IsClick(mousePosition, _downMousePosition))
                {
                    HandleFeatureInfo(e);
                    OnInfo(InvokeInfo(mousePosition, _downMousePosition, e.ClickCount));
                }
            }

            RefreshData();
            _mouseDown = false;

            double velocityX;
            double velocityY;

            (velocityX, velocityY) = _flingTracker.CalcVelocity(1, DateTime.Now.Ticks);

            if (Math.Abs(velocityX) > 200 || Math.Abs(velocityY) > 200)
            {
                // This was the last finger on screen, so this is a fling
                e.Handled = OnFlinged(velocityX, velocityY);
            }
            _flingTracker.RemoveId(1);

            _previousMousePosition = new MPoint();
            ReleaseMouseCapture();
        }

        /// <summary>
        /// Called, when mouse/finger/pen flinged over map
        /// </summary>
        /// <param name="velocityX">Velocity in x direction in pixel/second</param>
        /// <param name="velocityY">Velocity in y direction in pixel/second</param>
        private bool OnFlinged(double velocityX, double velocityY)
        {
            var args = new SwipedEventArgs(velocityX, velocityY);

            Fling?.Invoke(this, args);

            if (args.Handled)
                return true;

            Navigator?.FlingWith(velocityX, velocityY, 1000);

            return true;
        }

        private static bool IsClick(MPoint currentPosition, MPoint previousPosition)
        {
            return
                Math.Abs(currentPosition.X - previousPosition.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(currentPosition.Y - previousPosition.Y) < SystemParameters.MinimumVerticalDragDistance;
        }

        private void MapControlTouchUp(object? sender, TouchEventArgs e)
        {
            if (!_hasBeenManipulated)
            {
                var touchPosition = e.GetTouchPoint(this).Position.ToMapsui();
                // todo: Pass the touchDown position. It needs to be set at touch down.

                // todo: Figure out how to do a number of taps for WPF
                OnInfo(InvokeInfo(touchPosition, touchPosition, 1));
            }
        }

        public void OpenBrowser(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                // The default for this has changed in .net core, you have to explicitly set if to true for it to work.
                UseShellExecute = true
            });
        }

        private void HandleFeatureInfo(MouseButtonEventArgs e)
        {
            if (FeatureInfo == null) return; // don't fetch if you the call back is not set.

            if (_downMousePosition == e.GetPosition(this).ToMapsui())
                if (this.Map != null)
                {
                    foreach (var layer in Map.Layers)
                    {
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        (layer as IFeatureInfo)?.GetFeatureInfo(Viewport, _downMousePosition.X, _downMousePosition.Y,
                            OnFeatureInfo);
                    }
                }
        }

        private void OnFeatureInfo(IDictionary<string, IEnumerable<IFeature>> features)
        {
            FeatureInfo?.Invoke(this, new FeatureInfoEventArgs { FeatureInfo = features });
        }

        private void MapControlMouseMove(object sender, MouseEventArgs e)
        {
            if (IsInBoxZoomMode())
            {
                DrawBbox(e.GetPosition(this));
                return;
            }

            _currentMousePosition = e.GetPosition(this).ToMapsui(); //Needed for both MouseMove and MouseWheel event

            if (_mouseDown)
            {
                if (_previousMousePosition == null)
                {
                    // Usually MapControlMouseLeftButton down initializes _previousMousePosition but in some
                    // situations it can be null. So far I could only reproduce this in debug mode when putting
                    // a breakpoint and continuing.
                    return;
                }

                _flingTracker.AddEvent(1, _currentMousePosition, DateTime.Now.Ticks);

                _viewport.Transform(_currentMousePosition, _previousMousePosition);
                RefreshGraphics();
                _previousMousePosition = _currentMousePosition;
            }
            else
            {
                if (MouseWheelAnimation.IsAnimating())
                {
                    // Disabled because not performing:
                    // Navigator.ZoomTo(_toResolution, _currentMousePosition, _mouseWheelAnimationDuration, Easing.QuarticOut);
                }

            }
        }

        public void ZoomToBox(MPoint beginPoint, MPoint endPoint)
        {
            var width = Math.Abs(endPoint.X - beginPoint.X);
            var height = Math.Abs(endPoint.Y - beginPoint.Y);
            if (width <= 0) return;
            if (height <= 0) return;

            ZoomHelper.ZoomToBoudingbox(beginPoint.X, beginPoint.Y, endPoint.X, endPoint.Y,
                ActualWidth, ActualHeight, out var x, out var y, out var resolution);

            Navigator?.NavigateTo(new MPoint(x, y), resolution, 384);

            RefreshData();
            RefreshGraphics();
            ClearBBoxDrawing();
        }

        private void ClearBBoxDrawing()
        {
            RunOnUIThread(() => _selectRectangle.Visibility = Visibility.Collapsed);
        }

        private void DrawBbox(Point newPos)
        {
            if (_mouseDown)
            {
                if (_previousMousePosition == null) return; // can happen during debug

                var from = _previousMousePosition;
                var to = newPos;

                if (from.X > to.X)
                {
                    var temp = from;
                    from.X = to.X;
                    to.X = temp.X;
                }

                if (from.Y > to.Y)
                {
                    var temp = from;
                    from.Y = to.Y;
                    to.Y = temp.Y;
                }

                _selectRectangle.Width = to.X - from.X;
                _selectRectangle.Height = to.Y - from.Y;
                _selectRectangle.Margin = new Thickness(from.X, from.Y, 0, 0);
                _selectRectangle.Visibility = Visibility.Visible;
            }
        }

        private float ViewportWidth => (float)ActualWidth;
        private float ViewportHeight => (float)ActualHeight;

        private static void OnManipulationInertiaStarting(object? sender, ManipulationInertiaStartingEventArgs e)
        {
            e.TranslationBehavior.DesiredDeceleration = 25 * 96.0 / (1000.0 * 1000.0);
        }

        private void OnManipulationStarted(object? sender, ManipulationStartedEventArgs e)
        {
            _hasBeenManipulated = false;
            _virtualRotation = _viewport.Rotation;
        }

        private void OnManipulationDelta(object? sender, ManipulationDeltaEventArgs e)
        {
            var translation = e.DeltaManipulation.Translation;
            var center = e.ManipulationOrigin.ToMapsui().Offset(translation.X, translation.Y);
            var radius = GetDeltaScale(e.DeltaManipulation.Scale);
            var angle = e.DeltaManipulation.Rotation;
            var previousCenter = e.ManipulationOrigin.ToMapsui();
            var previousRadius = 1f;
            var prevAngle = 0f;

            _hasBeenManipulated |= Math.Abs(e.DeltaManipulation.Translation.X) > SystemParameters.MinimumHorizontalDragDistance
                     || Math.Abs(e.DeltaManipulation.Translation.Y) > SystemParameters.MinimumVerticalDragDistance;

            double rotationDelta = 0;

            if (Map?.RotationLock == false)
            {
                _virtualRotation += angle - prevAngle;

                rotationDelta = RotationCalculations.CalculateRotationDeltaWithSnapping(
                    _virtualRotation, _viewport.Rotation, _unSnapRotationDegrees, _reSnapRotationDegrees);
            }

            _viewport.Transform(center, previousCenter, radius / previousRadius, rotationDelta);
            RefreshGraphics();
            e.Handled = true;
        }

        private double GetDeltaScale(XamlVector scale)
        {
            if (_map?.ZoomLock ?? true) return 1;
            var deltaScale = (scale.X + scale.Y) / 2;
            if (Math.Abs(deltaScale) < Constants.Epsilon)
                return 1; // If there is no scaling the deltaScale will be 0.0 in Windows Phone (while it is 1.0 in wpf)
            if (!(Math.Abs(deltaScale - 1d) > Constants.Epsilon)) return 1;
            return deltaScale;
        }

        private void OnManipulationCompleted(object? sender, ManipulationCompletedEventArgs e)
        {
            Refresh();
        }

        private void SKElementOnPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            if (PixelDensity <= 0)
                return;

            var canvas = args.Surface.Canvas;

            canvas.Scale(PixelDensity, PixelDensity);

            CommonDrawControl(canvas);
        }

        private void PaintWpf()
        {
            CommonDrawControl(WpfCanvas);
        }

        private float GetPixelDensity()
        {
            var presentationSource = PresentationSource.FromVisual(this);
            if (presentationSource == null) throw new Exception("PresentationSource is null");
            var compositionTarget = presentationSource.CompositionTarget;
            if (compositionTarget == null) throw new Exception("CompositionTarget is null");

            var matrix = compositionTarget.TransformToDevice;

            var dpiX = matrix.M11;
            var dpiY = matrix.M22;

            if (dpiX != dpiY) throw new ArgumentException();

            return (float)dpiX;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _map?.Dispose();
            }

            CommonDispose(disposing);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
