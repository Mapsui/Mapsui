using Mapsui.Rendering;
using Mapsui.UI.Utils;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries.Utilities;
using Xamarin.Forms;

namespace Mapsui.UI.Forms
{
    /// <summary>
    /// Class, that uses the API of all other Mapsui MapControls
    /// </summary>
    public partial class MapControl : SKGLView, IMapControl, IDisposable
    {
        class TouchEvent
        {
            public long Id { get; }
            public Geometries.Point Location { get; }
            public long Tick { get; }

            public TouchEvent(long id, Geometries.Point screenPosition, long tick)
            {
                Id = id;
                Location = screenPosition;
                Tick = tick;
            }
        }

        private const int None = 0;
        private const int Dragging = 1;
        private const int Zooming = 2;
        // See http://grepcode.com/file/repository.grepcode.com/java/ext/com.google.android/android/4.0.4_r2.1/android/view/ViewConfiguration.java#ViewConfiguration.0PRESSED_STATE_DURATION for values
        private const int shortTap = 125;
        private const int shortClick = 250;
        private const int delayTap = 200;
        private const int longTap = 500;

        /// <summary>
        /// If a finger touches down and up it counts as a tap if the distance between the down and up location is smaller
        /// then the touch slob.
        /// The slob is initialized at 8. How did we get to 8? Well you could read the discussion here: https://github.com/Mapsui/Mapsui/issues/602
        /// We basically copied it from the Java source code: https://android.googlesource.com/platform/frameworks/base/+/master/core/java/android/view/ViewConfiguration.java#162
        /// </summary>
        private const int touchSlop = 8;

        private float _skiaScale;
        private double _innerRotation;
        private Dictionary<long, TouchEvent> _touches = new Dictionary<long, TouchEvent>();
        private Geometries.Point _firstTouch;
        private System.Threading.Timer _doubleTapTestTimer;
        private int _numOfTaps = 0;
        private FlingTracker _velocityTracker = new FlingTracker();
        private Geometries.Point _previousCenter;

        /// <summary>
        /// Saver for angle before last pinch movement
        /// </summary>
        private double _previousAngle;

        /// <summary>
        /// Saver for radius before last pinch movement
        /// </summary>
        private double _previousRadius = 1f;

        private TouchMode _mode;

        public MapControl()
        {
            Initialize();
        }

        public float SkiaScale => _skiaScale;

        public float PixelDensity => SkiaScale;

        public float ScreenWidth => (float)this.Width;

        public float ScreenHeight => (float)this.Height;

        private float ViewportWidth => ScreenWidth;

        private float ViewportHeight => ScreenHeight;

        public ISymbolCache SymbolCache => _renderer.SymbolCache;

        public float PixelsPerDeviceIndependentUnit => SkiaScale;

        public bool UseDoubleTap = true;

        public void Initialize()
        {
            Map = new Map();
            BackgroundColor = Color.White;

            EnableTouchEvents = true;

            PaintSurface += OnPaintSurface;
            Touch += OnTouch;
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            _touches.Clear();
            SetViewportSize();
        }

        private void OnTouch(object sender, SKTouchEventArgs e)
        {
            // Save time, when the event occures
            long ticks = DateTime.Now.Ticks;

            var location = GetScreenPosition(e.Location);

            if (e.ActionType == SKTouchAction.Pressed)
            {
                _firstTouch = location;

                _touches[e.Id] = new TouchEvent(e.Id, location, ticks);

                _velocityTracker.Clear();

                // Do we have a doubleTapTestTimer running?
                // If yes, stop it and increment _numOfTaps
                if (_doubleTapTestTimer != null)
                {
                    _doubleTapTestTimer.Dispose();
                    _doubleTapTestTimer = null;
                    _numOfTaps++;
                }
                else
                    _numOfTaps = 1;

                e.Handled = OnTouchStart(_touches.Select(t => t.Value.Location).ToList());
            }
            if (e.ActionType == SKTouchAction.Released)
            {
                // Delete e.Id from _touches, because finger is released
                var releasedTouch = _touches[e.Id];
                _touches.Remove(e.Id);

                // Is this a fling or swipe?
                if (_touches.Count == 0)
                {
                    double velocityX;
                    double velocityY;

                    (velocityX, velocityY) = _velocityTracker.CalcVelocity(e.Id, ticks);

                    if (Math.Abs(velocityX) > 200 || Math.Abs(velocityY) > 200)
                    {
                        // This was the last finger on screen, so this is a fling
                        e.Handled = OnFlinged(velocityX, velocityY);
                    }

                    // Do we have a tap event
                    if (releasedTouch == null)
                    {
                        e.Handled = false;
                        return;
                    }

                    // While tapping on screen, there could be a small movement of the finger
                    // (especially on Samsung). So check, if touch start location isn't more 
                    // than a number of pixels away from touch end location.

                    var isAround = Algorithms.Distance(releasedTouch.Location, _firstTouch) < touchSlop;

                    // If touch start and end is in the same area and the touch time is shorter
                    // than longTap, than we have a tap.
                    if (isAround && (ticks - releasedTouch.Tick) < (e.DeviceType == SKTouchDeviceType.Mouse ? shortClick : longTap) * 10000)
                    {
                        // Start a timer with timeout delayTap ms. If than isn't arrived another tap, than it is a single
                        _doubleTapTestTimer = new System.Threading.Timer((l) =>
                        {
                            if (_numOfTaps > 1)
                            {
                                if (!e.Handled)
                                    e.Handled = OnDoubleTapped(location, _numOfTaps);
                            }
                            else
                                if (!e.Handled)
                                e.Handled = OnSingleTapped((Geometries.Point)l);
                            _numOfTaps = 1;
                            if (_doubleTapTestTimer != null)
                            {
                                _doubleTapTestTimer.Dispose();
                            }
                            _doubleTapTestTimer = null;
                        }, location, UseDoubleTap ? delayTap : 0, -1);
                    }
                    else if (isAround && (ticks - releasedTouch.Tick) >= longTap * 10000)
                    {
                        if (!e.Handled)
                            e.Handled = OnLongTapped(location);
                    }
                }

                _velocityTracker.RemoveId(e.Id);

                if (_touches.Count == 1)
                {
                    e.Handled = OnTouchStart(_touches.Select(t => t.Value.Location).ToList());
                }

                if (!e.Handled)
                    e.Handled = OnTouchEnd(_touches.Select(t => t.Value.Location).ToList(), releasedTouch.Location);
            }
            if (e.ActionType == SKTouchAction.Moved)
            {
                _touches[e.Id] = new TouchEvent(e.Id, location, ticks);

                if (e.InContact)
                    _velocityTracker.AddEvent(e.Id, location, ticks);

                if (e.InContact && !e.Handled)
                    e.Handled = OnTouchMove(_touches.Select(t => t.Value.Location).ToList());
                else
                    e.Handled = OnHovered(_touches.Select(t => t.Value.Location).FirstOrDefault());
            }
            if (e.ActionType == SKTouchAction.Cancelled)
            {
                _touches.Remove(e.Id);
            }
            if (e.ActionType == SKTouchAction.Exited)
            {
            }
            if (e.ActionType == SKTouchAction.Entered)
            {
            }
        }

        void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs skPaintSurfaceEventArgs)
        {
            _skiaScale = (float)(CanvasSize.Width / Width);
            skPaintSurfaceEventArgs.Surface.Canvas.Scale(_skiaScale, _skiaScale);

            _renderer.Render(skPaintSurfaceEventArgs.Surface.Canvas,
                Viewport, _map.Layers, _map.Widgets, _map.BackColor);
        }

        private Geometries.Point GetScreenPosition(SKPoint point)
        {
            return new Geometries.Point(point.X / _skiaScale, point.Y / _skiaScale);
        }

        public void RefreshGraphics()
        {
            // Could this be null before Home is called? If so we should change the logic.
            if (GRContext != null) RunOnUIThread(InvalidateSurface);
        }

        /// <summary>
        /// Event handlers
        /// </summary>

        /// <summary>
        /// TouchStart is called, when user press a mouse button or touch the display
        /// </summary>
        public event EventHandler<TouchedEventArgs> TouchStarted;

        /// <summary>
        /// TouchEnd is called, when user release a mouse button or doesn't touch display anymore
        /// </summary>
        public event EventHandler<TouchedEventArgs> TouchEnded;

        /// <summary>
        /// TouchMove is called, when user move mouse over map (independent from mouse button state) or move finger on display
        /// </summary>
#if __WPF__
        public new event EventHandler<TouchedEventArgs> TouchMove;
#else
        public event EventHandler<TouchedEventArgs> TouchMove;
#endif

        /// <summary>
        /// Hover is called, when user move mouse over map without pressing mouse button
        /// </summary>
#if __ANDROID__
        public new event EventHandler<HoveredEventArgs> Hovered;
#else
        public event EventHandler<HoveredEventArgs> Hovered;
#endif

        /// <summary>
        /// Swipe is called, when user release mouse button or lift finger while moving with a certain speed 
        /// </summary>
        public event EventHandler<SwipedEventArgs> Swipe;

        /// <summary>
        /// Fling is called, when user release mouse button or lift finger while moving with a certain speed, higher than speed of swipe 
        /// </summary>
        public event EventHandler<SwipedEventArgs> Fling;

        /// <summary>
        /// SingleTap is called, when user clicks with a mouse button or tap with a finger on map 
        /// </summary>
        public event EventHandler<TappedEventArgs> SingleTap;

        /// <summary>
        /// LongTap is called, when user clicks with a mouse button or tap with a finger on map for 500 ms
        /// </summary>
        public event EventHandler<TappedEventArgs> LongTap;

        /// <summary>
        /// DoubleTap is called, when user clicks with a mouse button or tap with a finger two or more times on map
        /// </summary>
        public event EventHandler<TappedEventArgs> DoubleTap;

        /// <summary>
        /// Zoom is called, when map should be zoomed
        /// </summary>
        public event EventHandler<ZoomedEventArgs> Zoomed;

        /// <summary>
        /// Called, when map should zoom out
        /// </summary>
        /// <param name="screenPosition">Center of zoom out event</param>
        private bool OnZoomOut(Geometries.Point screenPosition)
        {
            if (Map.ZoomLock)
            {
                return true;
            }

            var args = new ZoomedEventArgs(screenPosition, ZoomDirection.ZoomOut);

            Zoomed?.Invoke(this, args);

            if (args.Handled)
                return true;

            // Perform standard behavior
            Navigator.ZoomOut(screenPosition);

            return true;
        }

        /// <summary>
        /// Called, when map should zoom in
        /// </summary>
        /// <param name="screenPosition">Center of zoom in event</param>
        private bool OnZoomIn(Geometries.Point screenPosition)
        {
            if (Map.ZoomLock)
            {
                return true;
            }

            var args = new ZoomedEventArgs(screenPosition, ZoomDirection.ZoomIn);

            Zoomed?.Invoke(this, args);

            if (args.Handled)
                return true;

            // Perform standard behavior
            Navigator.ZoomIn(screenPosition);

            return true;
        }

        /// <summary>
        /// Called, when mouse/finger/pen hovers around
        /// </summary>
        /// <param name="screenPosition">Actual position of mouse/finger/pen</param>
        private bool OnHovered(Geometries.Point screenPosition)
        {
            var args = new HoveredEventArgs(screenPosition);

            Hovered?.Invoke(this, args);

            return args.Handled;
        }

        /// <summary>
        /// Called, when mouse/finger/pen swiped over map
        /// </summary>
        /// <param name="velocityX">Velocity in x direction in pixel/second</param>
        /// <param name="velocityY">Velocity in y direction in pixel/second</param>
        private bool OnSwiped(double velocityX, double velocityY)
        {
            var args = new SwipedEventArgs(velocityX, velocityY);

            Swipe?.Invoke(this, args);

            // TODO
            // Perform standard behavior

            return args.Handled;
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

            // TODO
            // Perform standard behavior

            return args.Handled;
        }

        /// <summary>
        /// Called, when mouse/finger/pen click/touch map
        /// </summary>
        /// <param name="touchPoints">List of all touched points</param>
        private bool OnTouchStart(List<Geometries.Point> touchPoints)
        {
            // Sanity check
            if (touchPoints.Count == 0)
                return false;

            var args = new TouchedEventArgs(touchPoints);

            TouchStarted?.Invoke(this, args);

            if (args.Handled)
                return true;

            if (touchPoints.Count == 2)
            {
                (_previousCenter, _previousRadius, _previousAngle) = GetPinchValues(touchPoints);
                _mode = TouchMode.Zooming;
                _innerRotation = Viewport.Rotation;
            }
            else
            {
                _mode = TouchMode.Dragging;
                _previousCenter = touchPoints.First();
            }

            return true;
        }

        /// <summary>
        /// Called, when mouse/finger/pen anymore click/touch map
        /// </summary>
        /// <param name="touchPoints">List of all touched points</param>
        /// <param name="releasedPoint">Released point, which was touched before</param>
        private bool OnTouchEnd(List<Geometries.Point> touchPoints, Geometries.Point releasedPoint)
        {
            var args = new TouchedEventArgs(touchPoints);

            TouchEnded?.Invoke(this, args);

            // Last touch released
            if (touchPoints.Count == 0)
            {
                _mode = TouchMode.None;
                _map.RefreshData(_viewport.Extent, _viewport.Resolution, true);
            }

            return args.Handled;
        }

        /// <summary>
        /// Called, when mouse/finger/pen moves over map
        /// </summary>
        /// <param name="touchPoints">List of all touched points</param>
        private bool OnTouchMove(List<Geometries.Point> touchPoints)
        {
            var args = new TouchedEventArgs(touchPoints);

            TouchMove?.Invoke(this, args);

            if (args.Handled)
                return true;

            switch (_mode)
            {
                case TouchMode.Dragging:
                    {
                        if (touchPoints.Count != 1)
                            return false;

                        var touchPosition = touchPoints.First();

                        if (!Map.PanLock && _previousCenter != null && !_previousCenter.IsEmpty())
                        {
                            _viewport.Transform(touchPosition, _previousCenter);

                            RefreshGraphics();
                        }

                        _previousCenter = touchPosition;
                    }
                    break;
                case TouchMode.Zooming:
                    {
                        if (touchPoints.Count != 2)
                            return false;

                        var (prevCenter, prevRadius, prevAngle) = (_previousCenter, _previousRadius, _previousAngle);
                        var (center, radius, angle) = GetPinchValues(touchPoints);

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

                        _viewport.Transform(center, prevCenter, Map.ZoomLock ? 1 : radius / prevRadius, rotationDelta);

                        (_previousCenter, _previousRadius, _previousAngle) = (center, radius, angle);

                        RefreshGraphics();
                    }
                    break;
            }

            return true;
        }

        /// <summary>
        /// Called, when mouse/finger/pen tapped on map 2 or more times
        /// </summary>
        /// <param name="screenPosition">First clicked/touched position on screen</param>
        /// <param name="numOfTaps">Number of taps on map (2 is a double click/tap)</param>
        /// <returns>True, if the event is handled</returns>
        private bool OnDoubleTapped(Geometries.Point screenPosition, int numOfTaps)
        {
            var args = new TappedEventArgs(screenPosition, numOfTaps);

            DoubleTap?.Invoke(this, args);

            if (args.Handled)
                return true;

            var eventReturn = InvokeInfo(screenPosition, screenPosition, numOfTaps);

            if (eventReturn != null)
            {
                if (!eventReturn.Handled)
                {
                    // Double tap as zoom
                    return OnZoomIn(screenPosition);
                }
            }

            return false;
        }

        /// <summary>
        /// Called, when mouse/finger/pen tapped on map one time
        /// </summary>
        /// <param name="screenPosition">Clicked/touched position on screen</param>
        /// <returns>True, if the event is handled</returns>
        private bool OnSingleTapped(Geometries.Point screenPosition)
        {
            var args = new TappedEventArgs(screenPosition, 1);

            SingleTap?.Invoke(this, args);

            if (args.Handled)
                return true;

            var infoToInvoke = InvokeInfo(screenPosition, screenPosition, 1);
                        
            OnInfo(infoToInvoke);
            return infoToInvoke?.Handled ?? false;
        }

        /// <summary>
        /// Called, when mouse/finger/pen tapped long on map
        /// </summary>
        /// <param name="screenPosition">Clicked/touched position on screen</param>
        /// <returns>True, if the event is handled</returns>
        private bool OnLongTapped(Geometries.Point screenPosition)
        {
            var args = new TappedEventArgs(screenPosition, 1);

            LongTap?.Invoke(this, args);

            return args.Handled;
        }

        private static (Geometries.Point centre, double radius, double angle) GetPinchValues(List<Geometries.Point> locations)
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

            return (new Geometries.Point(centerX, centerY), radius, angle);
        }

        /// <summary>
        /// Public functions
        /// </summary>

        public float GetDeviceIndependentUnits()
        {
            return SkiaScale;
        }

        public void OpenBrowser(string url)
        {
            Device.OpenUri(new Uri(url));
        }

        private void RunOnUIThread(Action action)
        {
            Device.BeginInvokeOnMainThread(action);
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        protected void Dispose(bool disposing)
        {
            Unsubscribe();
        }
    }
}