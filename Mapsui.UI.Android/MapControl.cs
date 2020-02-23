using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Mapsui.Geometries.Utilities;
using Mapsui.Logging;
using SkiaSharp.Views.Android;
using Math = System.Math;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.UI.Android
{
    public partial class MapControl : ViewGroup, IMapControl
    {
        private SKGLSurfaceView _canvas;
        private double _innerRotation;
        private GestureDetector _gestureDetector;
        private double _previousAngle;
        private double _previousRadius = 1f;
        private TouchMode _mode = TouchMode.None;
        private Handler _mainLooperHandler;
        /// <summary>
        /// Saver for center before last pinch movement
        /// </summary>
        private Point _previousTouch = new Point();

        public MapControl(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public MapControl(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        public void Initialize()
        {
            SetBackgroundColor(Color.Transparent);
            _canvas = new SKGLSurfaceView(Context);
            _canvas.PaintSurface += CanvasOnPaintSurface;
            AddView(_canvas);

            _mainLooperHandler = new Handler(Looper.MainLooper);

            SetViewportSize(); // todo: check if size is available, perhaps we need a load event

            Map = new Map();
            Touch += MapView_Touch;

            _gestureDetector = new GestureDetector(Context, new GestureDetector.SimpleOnGestureListener());
            _gestureDetector.SingleTapConfirmed += OnSingleTapped;
            _gestureDetector.DoubleTap += OnDoubleTapped;
        }

        public float PixelDensity => Resources.DisplayMetrics.Density;

        private void OnDoubleTapped(object sender, GestureDetector.DoubleTapEventArgs e)
        {
            var position = GetScreenPosition(e.Event, this);
            OnInfo(InvokeInfo(position, position, 2));
        }

        private void OnSingleTapped(object sender, GestureDetector.SingleTapConfirmedEventArgs e)
        {
            var position = GetScreenPosition(e.Event, this);
            OnInfo(InvokeInfo(position, position, 1));
        }

        protected override void OnSizeChanged(int width, int height, int oldWidth, int oldHeight)
        {
            base.OnSizeChanged(width, height, oldWidth, oldHeight);
            SetViewportSize();
        }

        private void RunOnUIThread(Action action)
        {
            if (SynchronizationContext.Current == null)
                _mainLooperHandler.Post(action);
            else
                action();
        }

        private void CanvasOnPaintSurface(object sender, SKPaintGLSurfaceEventArgs args)
        {
            args.Surface.Canvas.Scale(PixelDensity, PixelDensity);
            Renderer.Render(args.Surface.Canvas, Viewport, _map.Layers, _map.Widgets, _map.BackColor);
        }

        public void MapView_Touch(object sender, TouchEventArgs args)
        {
            if (_gestureDetector.OnTouchEvent(args.Event))
                return;

            var touchPoints = GetScreenPositions(args.Event, this);

            switch (args.Event.Action)
            {
                case MotionEventActions.Up:
                    Refresh();
                    _mode = TouchMode.None;
                    break;
                case MotionEventActions.Down:
                case MotionEventActions.Pointer1Down:
                case MotionEventActions.Pointer2Down:
                case MotionEventActions.Pointer3Down:
                    if (touchPoints.Count >= 2)
                    {
                        (_previousTouch, _previousRadius, _previousAngle) = GetPinchValues(touchPoints);
                        _mode = TouchMode.Zooming;
                        _innerRotation = Viewport.Rotation;
                    }
                    else
                    {
                        _mode = TouchMode.Dragging;
                        _previousTouch = touchPoints.First();
                    }
                    break;
                case MotionEventActions.Pointer1Up:
                case MotionEventActions.Pointer2Up:
                case MotionEventActions.Pointer3Up:
                    // Remove the touchPoint that was released from the locations to reset the
                    // starting points of the move and rotation
                    touchPoints.RemoveAt(args.Event.ActionIndex);

                    if (touchPoints.Count >= 2)
                    {
                        (_previousTouch, _previousRadius, _previousAngle) = GetPinchValues(touchPoints);
                        _mode = TouchMode.Zooming;
                        _innerRotation = Viewport.Rotation;
                    }
                    else
                    {
                        _mode = TouchMode.Dragging;
                        _previousTouch = touchPoints.First();
                    }
                    Refresh();
                    break;
                case MotionEventActions.Move:
                    switch (_mode)
                    {
                        case TouchMode.Dragging:
                            {
                                if (touchPoints.Count != 1)
                                    return;

                                var touch = touchPoints.First();
                                if (_previousTouch != null && !_previousTouch.IsEmpty())
                                {
                                    _viewport.Transform(touch, _previousTouch);
                                    RefreshGraphics();
                                }
                                _previousTouch = touch;
                            }
                            break;
                        case TouchMode.Zooming:
                            {
                                if (touchPoints.Count < 2)
                                    return;

                                var (previousTouch, previousRadius, previousAngle) = (_previousTouch, _previousRadius, _previousAngle);
                                var (touch, radius, angle) = GetPinchValues(touchPoints);

                                double rotationDelta = 0;

                                if (!Map.RotationLock)
                                {
                                    _innerRotation += angle - previousAngle;
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

                                _viewport.Transform(touch, previousTouch, radius / previousRadius, rotationDelta);
                                RefreshGraphics();

                                (_previousTouch, _previousRadius, _previousAngle) = (touch, radius, angle);

                                
                            }
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets the screen position in device independent units relative to the MapControl.
        /// </summary>
        /// <param name="motionEvent"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        private List<Point> GetScreenPositions(MotionEvent motionEvent, View view)
        {
            var result = new List<Point>();
            for (var i = 0; i < motionEvent.PointerCount; i++)
            {
                result.Add(new Point(motionEvent.GetX(i) - view.Left, motionEvent.GetY(i) - view.Top)
                    .ToDeviceIndependentUnits(PixelDensity));
            }
            return result;
        }

        /// <summary>
        /// Gets the screen position in device independent units relative to the MapControl.
        /// </summary>
        /// <param name="motionEvent"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        private Point GetScreenPosition(MotionEvent motionEvent, View view)
        {
            return GetScreenPositionInPixels(motionEvent, view)
                .ToDeviceIndependentUnits(PixelDensity);
        }

        /// <summary>
        /// Gets the screen position in pixels relative to the MapControl.
        /// </summary>
        /// <param name="motionEvent"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        private static Point GetScreenPositionInPixels(MotionEvent motionEvent, View view)
        {
            return new PointF(
                motionEvent.GetX(0) - view.Left,
                motionEvent.GetY(0) - view.Top).ToMapsui();
        }

        public void RefreshGraphics()
        {
            RunOnUIThread(RefreshGraphicsWithTryCatch);
        }

        private void RefreshGraphicsWithTryCatch()
        {
            try
            {
                // Bothe Invalidate and _canvas.Invalidate are necessary in different scenarios.
                Invalidate();
                _canvas?.Invalidate();
            }
            catch (ObjectDisposedException e)
            {
                // See issue: https://github.com/Mapsui/Mapsui/issues/433
                // What seems to be happening. The Activity is Disposed. Appently it's children get Disposed
                // explicitly by something in Xamarin. During this Dispose the MessageCenter, which is itself
                // not disposed gets another notification to call RefreshGraphics.
                Logger.Log(LogLevel.Warning, "This can happen when the parent Activity is disposing.", e);
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            _canvas.Top = t;
            _canvas.Bottom = b;
            _canvas.Left = l;
            _canvas.Right = r;
        }

        public void OpenBrowser(string url)
        {
            global::Android.Net.Uri uri = global::Android.Net.Uri.Parse(url);
            Intent intent = new Intent(Intent.ActionView);
            intent.SetData(uri);

            Intent chooser = Intent.CreateChooser(intent, "Open with");

            Context.StartActivity(chooser);
        }

        public new void Dispose()
        {
            Unsubscribe();
            base.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            Unsubscribe();
            base.Dispose(disposing);
        }

        private static (Point centre, double radius, double angle) GetPinchValues(List<Point> locations)
        {
            if (locations.Count < 2)
                throw new ArgumentException();

            double centerX = 0;
            double centerY = 0;

            foreach (var location in locations)
            {
                centerX += location.X;
                centerY += location.Y;
            }

            centerX = centerX / locations.Count;
            centerY = centerY / locations.Count;

            var radius = Algorithms.Distance(centerX, centerY, locations[0].X, locations[0].Y);

            var angle = Math.Atan2(locations[1].Y - locations[0].Y, locations[1].X - locations[0].X) * 180.0 / Math.PI;

            return (new Point(centerX, centerY), radius, angle);
        }

        private float ViewportWidth => ToDeviceIndependentUnits(Width);
        private float ViewportHeight => ToDeviceIndependentUnits(Height);

        /// <summary>
        /// In native Android touch positions are in pixels whereas the canvas needs
        /// to be drawn in device independent units (otherwise labels on raster tiles will be unreadable
        /// and symbols will be too small). This method converts pixels to device independent units.
        /// </summary>
        /// <returns>The pixels given as input translated to device independent units.</returns>
        private float ToDeviceIndependentUnits(float pixelCoordinate)
        {
            return pixelCoordinate / PixelDensity;
        }
    }
}