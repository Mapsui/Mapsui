using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Rendering.Skia;
using Mapsui.Utilities;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Point = System.Windows.Point;
using VerticalAlignment = System.Windows.VerticalAlignment;
using XamlVector = System.Windows.Vector;

namespace Mapsui.UI.Wpf
{
    public enum RenderMode
    {
        Skia,
        Wpf
    }

    public partial class MapControl : Grid, IMapControl
    {
        // ReSharper disable once UnusedMember.Local // This registration triggers the call to OnResolutionChanged
        private static readonly DependencyProperty ResolutionProperty =
            DependencyProperty.Register(
                "Resolution", typeof(double), typeof(MapControl),
                new PropertyMetadata(OnResolutionChanged));

        private readonly Rectangle _selectRectangle = CreateSelectRectangle();
        private readonly DoubleAnimation _zoomAnimation = new DoubleAnimation();
        private readonly Storyboard _zoomStoryBoard = new Storyboard();
        private Geometries.Point _currentMousePosition;
        private Geometries.Point _downMousePosition;
        private bool _mouseDown;
        private Geometries.Point _previousMousePosition;
        private RenderMode _renderMode;
        private double _toResolution = double.NaN;
        private bool _hasBeenManipulated;
        private double _innerRotation;

        public MapControl()
        {
            Children.Add(WpfCanvas);
            Children.Add(SkiaCanvas);
            Children.Add(_selectRectangle);

            SkiaCanvas.PaintSurface += SKElementOnPaintSurface;

            Map = new Map();

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

            RenderMode = RenderMode.Skia;
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (RenderMode == RenderMode.Wpf) PaintWpf();
            base.OnRender(dc);
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

        public RenderMode RenderMode
        {
            get => _renderMode;
            set
            {
                _renderMode = value;
                if (_renderMode == RenderMode.Skia)
                {
                    WpfCanvas.Visibility = Visibility.Collapsed;
                    SkiaCanvas.Visibility = Visibility.Visible;
                    Renderer = new MapRenderer();
                    RefreshGraphics();
                }
                else
                {
                    SkiaCanvas.Visibility = Visibility.Collapsed;
                    WpfCanvas.Visibility = Visibility.Visible;
                    Renderer = new Rendering.Xaml.MapRenderer();
                    RefreshGraphics();
                }
                OnPropertyChanged();
            }
        }

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

        [Obsolete("Use Viewport.ViewportChanged", true)]
        // ReSharper disable once UnusedMember.Global
#pragma warning disable 67
        public event EventHandler<ViewChangedEventArgs> ViewChanged;
#pragma warning restore 67

        public event EventHandler<FeatureInfoEventArgs> FeatureInfo; // todo: Remove and add sample for alternative

        public void RefreshGraphics()
        {
            RunOnUIThread(InvalidateCanvas);
        }

        internal void InvalidateCanvas()
        {
            if (RenderMode == RenderMode.Wpf) InvalidateVisual(); // To trigger OnRender of this MapControl
            else SkiaCanvas.InvalidateVisual();

        }

        [Obsolete("Use Navigator.ZoomIn instead", true)]
        public void ZoomIn() {}

        [Obsolete("Use Navigator.ZoomOut instead", true)]
        public void ZoomOut() {}

        private static void OnResolutionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var newResolution = (double)e.NewValue;
            ((MapControl)dependencyObject).ZoomToResolution(newResolution);
        }

        private void ZoomToResolution(double resolution)
        {
            var current = _currentMousePosition;
            Navigator.ZoomTo(resolution, current);
            RefreshGraphics();
        }

        private void MapControlLoaded(object sender, RoutedEventArgs e)
        {
            SetViewportSize();

            InitAnimation();
            Focusable = true;
        }
        
        public float PixelDensity => DeterminePixelDensity();

        private float DeterminePixelDensity()
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

        private void InitAnimation()
        {
            _zoomAnimation.Completed += ZoomAnimationCompleted;
            _zoomAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 1000));
            _zoomAnimation.EasingFunction = new QuarticEase();
            Storyboard.SetTarget(_zoomAnimation, this);
            Storyboard.SetTargetProperty(_zoomAnimation, new PropertyPath("Resolution"));
            _zoomStoryBoard.Children.Add(_zoomAnimation);
        }

        private void MapControlMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Map.ZoomLock) return;
            if (!Viewport.HasSize) return;

            _currentMousePosition = e.GetPosition(this).ToMapsui();
            //Needed for both MouseMove and MouseWheel event for mousewheel event

            if (double.IsNaN(_toResolution))
                _toResolution = Viewport.Resolution;

            if (e.Delta > Constants.Epsilon)
            {
                _toResolution = ZoomHelper.ZoomIn(_map.Resolutions, _toResolution);
            }
            else if (e.Delta < Constants.Epsilon)
            {
                _toResolution = ZoomHelper.ZoomOut(_map.Resolutions, _toResolution);
            }

            // Some cheating to trigger a zoom animation if resolution does not change.
            // This workaround could be ommitted if the zoom animations was on CenterX, CenterY and Resolution, not Resolution alone.
            // todo: Remove this workaround once animations are centralized.
            Navigator.CenterOn(new Geometries.Point(Viewport.Center.X + 0.000000001, Viewport.Center.Y + 0.000000001));

            StartZoomAnimation(Viewport.Resolution, _toResolution);
        }

        private void StartZoomAnimation(double begin, double end)
        {
            _zoomStoryBoard.Pause(); //using Stop() here causes unexpected results while zooming very fast.
            _zoomAnimation.From = begin;
            _zoomAnimation.To = end;
            _zoomStoryBoard.Begin();
        }

        private void ZoomAnimationCompleted(object sender, EventArgs e)
        {
            _toResolution = double.NaN;
        }

        private void MapControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };
            SetViewportSize();
        }

        private void MapControlMouseLeave(object sender, MouseEventArgs e)
        {
            _previousMousePosition = new Geometries.Point();
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
            CaptureMouse();

            if (!IsInBoxZoomMode())
            {
                if (IsClick(_currentMousePosition, _downMousePosition))
                {
                    HandleFeatureInfo(e);
                    var mapInfoEventArgs = InvokeInfo(touchPosition, _downMousePosition, e.ClickCount);
                    OnInfo(mapInfoEventArgs);
                }
            }
        }

        private static bool IsInBoxZoomMode()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        }

        private void MapControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(this).ToMapsui();

            if (IsInBoxZoomMode())
            {
                var previous = Viewport.ScreenToWorld(_previousMousePosition.X, _previousMousePosition.Y);
                var current = Viewport.ScreenToWorld(mousePosition.X, mousePosition.Y);
                ZoomToBox(previous, current);
            }

            RefreshData();
            _mouseDown = false;

            _previousMousePosition = new Geometries.Point();
            ReleaseMouseCapture();
        }

        private static bool IsClick(Geometries.Point currentPosition, Geometries.Point previousPosition)
        {
            return
                Math.Abs(currentPosition.X - previousPosition.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(currentPosition.Y - previousPosition.Y) < SystemParameters.MinimumVerticalDragDistance;
        }

        private void MapControlTouchUp(object sender, TouchEventArgs e)
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
            Process.Start(url);
        }

        private void HandleFeatureInfo(MouseButtonEventArgs e)
        {
            if (FeatureInfo == null) return; // don't fetch if you the call back is not set.

            if (_downMousePosition == e.GetPosition(this).ToMapsui())
                foreach (var layer in Map.Layers)
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    (layer as IFeatureInfo)?.GetFeatureInfo(Viewport, _downMousePosition.X, _downMousePosition.Y,
                        OnFeatureInfo);
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
                if (_previousMousePosition == null || _previousMousePosition.IsEmpty())
                {
                    // Usually MapControlMouseLeftButton down initializes _previousMousePosition but in some
                    // situations it can be null. So far I could only reproduce this in debug mode when putting
                    // a breakpoint and continuing.
                    return;
                }
                
                _viewport.Transform(_currentMousePosition, _previousMousePosition);
                RefreshGraphics();
                _previousMousePosition = _currentMousePosition;
            }
        }

        public void ZoomToBox(Geometries.Point beginPoint, Geometries.Point endPoint)
        {
            var width = Math.Abs(endPoint.X - beginPoint.X);
            var height = Math.Abs(endPoint.Y - beginPoint.Y);
            if (width <= 0) return;
            if (height <= 0) return;

            ZoomHelper.ZoomToBoudingbox(beginPoint.X, beginPoint.Y, endPoint.X, endPoint.Y,
                ActualWidth, ActualHeight, out var x, out var y, out var resolution);

            Navigator.NavigateTo(new Geometries.Point(x, y), resolution);

            _toResolution = resolution; // for animation

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
        private float ViewportHeight => (float) ActualHeight;

        private static void OnManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            e.TranslationBehavior.DesiredDeceleration = 25 * 96.0 / (1000.0 * 1000.0);
        }

        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            _hasBeenManipulated = false;
            _innerRotation = _viewport.Rotation;
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
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

            if (!Map.RotationLock)
            {
                _innerRotation += angle - prevAngle;
                _innerRotation %= 360;

                if (_innerRotation > 180)
                    _innerRotation -= 360;
                else if (_innerRotation < -180)
                    _innerRotation += 360;

                if (Viewport.Rotation == 0 && Math.Abs(_innerRotation) >= Math.Abs(UnSnapRotationDegrees))
                    rotationDelta = _innerRotation;
                else if (Viewport.Rotation != 0)
                {
                    if (Math.Abs(_innerRotation) <= Math.Abs(ReSnapRotationDegrees))
                        rotationDelta = -Viewport.Rotation;
                    else
                        rotationDelta = _innerRotation - Viewport.Rotation;
                }
            }

            _viewport.Transform(center, previousCenter, radius / previousRadius, rotationDelta);
            RefreshGraphics();
            e.Handled = true;
        }

        private double GetDeltaScale(XamlVector scale)
        {
            if (Map.ZoomLock) return 1;
            var deltaScale = (scale.X + scale.Y) / 2;
            if (Math.Abs(deltaScale) < Constants.Epsilon)
                return 1; // If there is no scaling the deltaScale will be 0.0 in Windows Phone (while it is 1.0 in wpf)
            if (!(Math.Abs(deltaScale - 1d) > Constants.Epsilon)) return 1;
            return deltaScale;
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            Refresh();
        }

        private void SKElementOnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            if (Renderer == null) return;
            if (_map == null) return;

            args.Surface.Canvas.Scale(PixelDensity, PixelDensity);
            Renderer.Render(args.Surface.Canvas, Viewport, Map.Layers, Map.Widgets, Map.BackColor);
        }
        
        private void PaintWpf()
        {
            if (Renderer == null) return;
            if (_map == null) return;

            Renderer.Render(WpfCanvas, Viewport, _map.Layers, Map.Widgets, _map.BackColor);
        }
    }
}
