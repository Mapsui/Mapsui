using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.UI.Uwp;
using Mapsui.Utilities;

namespace Mapsui.UI.Avalonia
{
    public partial class MapControl : Grid, IMapControl
    {
        private Point mousePos;
        private MapsuiCustomDrawOp _drawOp;
        private readonly Rectangle _selectRectangle = CreateSelectRectangle();
        private Geometries.Point _currentMousePosition;
        private Geometries.Point _downMousePosition;
        private bool _mouseDown;
        private Geometries.Point _previousMousePosition;
        private double _toResolution = double.NaN;
        public event EventHandler<FeatureInfoEventArgs> FeatureInfo;

        public MapControl()
        {
            ClipToBounds = true;
            Children.Add(_selectRectangle);

            CommonInitialize();
            Initialize();
        }

        void Initialize()
        {
            _invalidate = () => { RunOnUIThread(this.InvalidateVisual); };

            this.Initialized += MapControlInitialized;

            PointerPressed += MapControl_PointerPressed;
            PointerReleased += MapControl_PointerReleased;
            PointerMoved += MapControlMouseMove;
            PointerLeave += MapControlMouseLeave;

            PointerWheelChanged += MapControlMouseWheel;

            Tapped += OnSingleTapped;
            DoubleTapped += OnDoubleTapped;
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            switch (change.Property.Name)
            {
                case nameof(this.Bounds):
                    // size changed
                    MapControlSizeChanged();
                    break;
            }
        }

        private void MapControl_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                MapControlMouseLeftButtonDown(e);
            }
        }

        private void MapControlMouseWheel(object sender, global::Avalonia.Input.PointerWheelEventArgs e)
        {
            if (Map.ZoomLock) return;
            if (!Viewport.HasSize) return;

            _currentMousePosition = e.GetPosition(this).ToMapsui();
            //Needed for both MouseMove and MouseWheel event for mousewheel event

            if (double.IsNaN(_toResolution))
                _toResolution = Viewport.Resolution;

            if (e.Delta.Y > Constants.Epsilon)
            {
                _toResolution = ZoomHelper.ZoomIn(_map.Resolutions, _toResolution);
            }
            else if (e.Delta.Y < Constants.Epsilon)
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
            //TODO TEMP
            Navigator.ZoomTo(_toResolution, _currentMousePosition);
            RefreshGraphics();
        }

        private void MapControlMouseLeftButtonDown(global::Avalonia.Input.PointerPressedEventArgs e)
        {
            var touchPosition = e.GetPosition(this).ToMapsui();
            _previousMousePosition = touchPosition;
            _downMousePosition = touchPosition;
            _mouseDown = true;
            e.Pointer.Capture(this);
            
            if (!IsInBoxZoomMode())
            {
                if (IsClick(_currentMousePosition, _downMousePosition))
                {
                    HandleFeatureInfo(e);
                    OnInfo(InvokeInfo(touchPosition, _downMousePosition, e.ClickCount));
                }
            }
        }

        private void HandleFeatureInfo(global::Avalonia.Input.PointerPressedEventArgs e)
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

        private void MapControlMouseLeave(object sender, global::Avalonia.Input.PointerEventArgs e)
        {
            _previousMousePosition = new Geometries.Point();
        }

        private void MapControlMouseMove(object sender, global::Avalonia.Input.PointerEventArgs e)
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
                    to = to.WithX(temp.X);
                }

                if (from.Y > to.Y)
                {
                    var temp = from;
                    from.Y = to.Y;
                    to = to.WithY(temp.Y);
                }

                _selectRectangle.Width = to.X - from.X;
                _selectRectangle.Height = to.Y - from.Y;
                _selectRectangle.Margin = new Thickness(from.X, from.Y, 0, 0);
                _selectRectangle.IsVisible = true;
            }
        }

        private void MapControl_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.PointerUpdateKind ==
                global::Avalonia.Input.PointerUpdateKind.LeftButtonReleased)
            {
                MapControlMouseLeftButtonUp(e);
            }
        }

        private static bool IsInBoxZoomMode()
        {
            // return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            return false;
        }

        private void MapControlMouseLeftButtonUp(global::Avalonia.Input.PointerReleasedEventArgs e)
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
            //      ReleaseMouseCapture();
        }

        public void ZoomToBox(Geometries.Point beginPoint, Geometries.Point endPoint)
        {
            var width = Math.Abs(endPoint.X - beginPoint.X);
            var height = Math.Abs(endPoint.Y - beginPoint.Y);
            if (width <= 0) return;
            if (height <= 0) return;

            ZoomHelper.ZoomToBoudingbox(beginPoint.X, beginPoint.Y, endPoint.X, endPoint.Y,
                Bounds.Width, Bounds.Height, out var x, out var y, out var resolution);

            Navigator.NavigateTo(new Geometries.Point(x, y), resolution);

            _toResolution = resolution; // for animation

            RefreshData();
            RefreshGraphics();
            ClearBBoxDrawing();
        }

        private void ClearBBoxDrawing()
        {
            RunOnUIThread(() => _selectRectangle.IsVisible = false);
        }

        private static bool IsClick(Geometries.Point currentPosition, Geometries.Point previousPosition)
        {
            return
                Math.Abs(currentPosition.X - previousPosition.X) < 1 &&
                Math.Abs(currentPosition.Y - previousPosition.Y) < 1;
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            mousePos = e.GetPosition(this);
        }

        private void OnDoubleTapped(object? sender, RoutedEventArgs e)
        {
            // We have a new interaction with the screen, so stop all navigator animations
            Navigator.StopRunningAnimation();
            var tapPosition = mousePos.ToMapsui();
            OnInfo(InvokeInfo(tapPosition, tapPosition, 2));
        }

        private void OnSingleTapped(object? sender, RoutedEventArgs e)
        {
            // We have a new interaction with the screen, so stop all navigator animations
            Navigator.StopRunningAnimation();

            var tapPosition = mousePos.ToMapsui();
            OnInfo(InvokeInfo(tapPosition, tapPosition, 1));
        }

        public override void Render(DrawingContext context)
        {
            if (_drawOp == null) _drawOp = new MapsuiCustomDrawOp(new Rect(0, 0, Bounds.Width, Bounds.Height), this);
            _drawOp.Bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
            context.Custom(_drawOp);
        }

        private void MapControlInitialized(object? sender, EventArgs eventArgs)
        {
            SetViewportSize();
        }

        private void MapControlSizeChanged()
        {
            SetViewportSize();
        }

        private void RunOnUIThread(Action action)
        {
            Task.Run(() => Dispatcher.UIThread.InvokeAsync(action));
        }

        public void OpenBrowser(string url)
        {
            using (Process process = Process.Start(new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? url : "open",
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"-e {url}" : "",
                CreateNoWindow = true,
                UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            })) ;
        }

        private float ViewportWidth => Convert.ToSingle(Bounds.Width);
        private float ViewportHeight => Convert.ToSingle(Bounds.Height);

        private float GetPixelDensity()
        {
            if (this.VisualRoot != null)
            {
                return Convert.ToSingle(this.VisualRoot.RenderScaling);
            }

            return 1f;
        }

        private static Rectangle CreateSelectRectangle()
        {
            return new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Red),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 3,
                StrokeDashArray = new AvaloniaList<double>() { 3.0 },
                Opacity = 0.3,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                IsVisible = false
            };
        }

        private class MapsuiCustomDrawOp : ICustomDrawOperation
        {
            private readonly MapControl mapControl;

            private readonly FormattedText noSkia = new FormattedText()
            {
                Text = "Current rendering API is not Skia"
            };

            public MapsuiCustomDrawOp(Rect bounds, MapControl mapControl)
            {
                Bounds = bounds;
                this.mapControl = mapControl;
            }

            public void Dispose()
            {
                // No-op
            }

            public Rect Bounds { get; set; }

            public bool HitTest(Point p)
            {
                return true;
            }

            public bool Equals(ICustomDrawOperation other) => false;

            public void Render(IDrawingContextImpl context)
            {
                var canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;
                if (canvas == null)
                    context.DrawText(Brushes.Black, new Point(), noSkia.PlatformImpl);
                else
                {
                    canvas.Save();
                    mapControl.CommonDrawControl(canvas);
                    canvas.Restore();
                }
            }
        }
    }
}