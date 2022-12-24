using Mapsui.Rendering;
using Mapsui.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Utilities;
#if __MAUI__
using Mapsui.UI.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using Color = Microsoft.Maui.Graphics.Color;
using Logger = Mapsui.Logging.Logger;
using KnownColor = Mapsui.UI.Maui.KnownColor;
#else
using SkiaSharp.Views.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;
using KnownColor = Xamarin.Forms.Color;
#endif

#if __MAUI__
namespace Mapsui.UI.Maui
#else
namespace Mapsui.UI.Forms
#endif
{
    /// <summary>
    /// Class, that uses the API of all other Mapsui MapControls
    /// </summary>
    public partial class MapControl : ContentView, IMapControl, IDisposable
    {
#if __MAUI__
        // GPU does not work currently on MAUI
        // See https://github.com/mono/SkiaSharp/issues/1893
        public static bool UseGPU = DeviceInfo.Platform != DevicePlatform.WinUI;
#else
        public static bool UseGPU = true;
#endif

        private class TouchEvent
        {
            public long Id { get; }
            public MPoint Location { get; }
            public long Tick { get; }

            public TouchEvent(long id, MPoint screenPosition, long tick)
            {
                Id = id;
                Location = screenPosition;
                Tick = tick;
            }
        }

        private SKGLView? _glView;
        private SKCanvasView? _canvasView;

        // See http://grepcode.com/file/repository.grepcode.com/java/ext/com.google.android/android/4.0.4_r2.1/android/view/ViewConfiguration.java#ViewConfiguration.0PRESSED_STATE_DURATION for values
        private const int ShortTap = 125;
        private const int ShortClick = 250;
        private const int DelayTap = 200;
        private const int longTap = 500;

        /// <summary>
        /// If a finger touches down and up it counts as a tap if the distance between the down and up location is smaller
        /// then the touch slob.
        /// The slob is initialized at 8. How did we get to 8? Well you could read the discussion here: https://github.com/Mapsui/Mapsui/issues/602
        /// We basically copied it from the Java source code: https://android.googlesource.com/platform/frameworks/base/+/master/core/java/android/view/ViewConfiguration.java#162
        /// </summary>
        private const int TouchSlop = 8;

        protected readonly bool _initialized;

        private double _virtualRotation;
        private readonly ConcurrentDictionary<long, TouchEvent> _touches = new();
        private MPoint? _firstTouch;
        private bool _waitingForDoubleTap;
        private int _numOfTaps;
        private readonly FlingTracker _flingTracker = new();
        private MPoint? _previousCenter;

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
            CommonInitialize();
            Initialize();

            _initialized = true;
        }

        public float ScreenWidth => (float)Width;

        public float ScreenHeight => (float)Height;

        private float ViewportWidth => ScreenWidth;

        private float ViewportHeight => ScreenHeight;

        public ISymbolCache SymbolCache => _renderer.SymbolCache;

        public bool UseDoubleTap = true;
        public bool UseFling = true;
#if __MAUI__
        private Size oldSize;
#endif

        private void Initialize()
        {
            View view;

            if (UseGPU)
            {
                // Use GPU backend
                _glView = new SKGLView
                {
                    HasRenderLoop = false,
                    EnableTouchEvents = true,
                };
                // Events
                _glView.Touch += OnTouch;
                _glView.PaintSurface += OnGLPaintSurface;
                _invalidate = () => { RunOnUIThread(() => _glView.InvalidateSurface()); };
                view = _glView;
            }
            else
            {
                // Use CPU backend
                _canvasView = new SKCanvasView
                {
                    EnableTouchEvents = true,
                };
                // Events
                _canvasView.Touch += OnTouch;
                _canvasView.PaintSurface += OnPaintSurface;
                _invalidate = () => { RunOnUIThread(() => _canvasView.InvalidateSurface()); };
                view = _canvasView;
            }

#if __MAUI__
            view.PropertyChanged += View_PropertyChanged;
#else
            view.SizeChanged += OnSizeChanged;
#endif

            Content = view;

            BackgroundColor = KnownColor.White;
        }

#if __MAUI__
        private void View_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.Width):
                case nameof(this.Height):
                    var newSize = new Size(this.Width, this.Height);

                    if (newSize.Width > 0 && newSize.Height > 0 && this.oldSize != newSize)
                    {
                        this.oldSize = newSize;
                        // Maui Workaround because the OnSizeChanged Events don't fire.
                        // Maybe this is a Bug and will be fixed in later versions.
                        this.OnSizeChanged(this, EventArgs.Empty);
                    }

                    break;
            }
        }
#endif

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            _touches.Clear();
            SetViewportSize();
        }

        private async void OnTouch(object? sender, SKTouchEventArgs e)
        {
            try
            {
                // Save time, when the event occurs
                var ticks = DateTime.Now.Ticks;

                var location = GetScreenPosition(e.Location);

                // if user handles action by his own return
                TouchAction?.Invoke(sender, e);
                if (e.Handled) return;

                if (e.ActionType == SKTouchAction.Pressed)
                {
                    _firstTouch = location;

                    _touches[e.Id] = new TouchEvent(e.Id, location, ticks);

                    _flingTracker.Clear();

                    // Do we have a doubleTapTestTimer running?
                    // If yes, stop it and increment _numOfTaps
                    if (_waitingForDoubleTap)
                    {
                        _waitingForDoubleTap = false;
                        _numOfTaps++;
                    }
                    else
                        _numOfTaps = 1;

                    e.Handled = OnTouchStart(_touches.Select(t => t.Value.Location).ToList());
                }
                // Delete e.Id from _touches, because finger is released
                else if (e.ActionType == SKTouchAction.Released && _touches.TryRemove(e.Id, out var releasedTouch))
                {
                    // Is this a fling or swipe?
                    if (_touches.Count == 0)
                    {
                        double velocityX;
                        double velocityY;

                        if (UseFling)
                        {
                            (velocityX, velocityY) = _flingTracker.CalcVelocity(e.Id, ticks);

                            if (Math.Abs(velocityX) > 200 || Math.Abs(velocityY) > 200)
                            {
                                // This was the last finger on screen, so this is a fling
                                e.Handled = OnFlinged(velocityX, velocityY);
                            }
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
                        var isAround = IsAround(releasedTouch);

                        // If touch start and end is in the same area and the touch time is shorter
                        // than longTap, than we have a tap.
                        if (isAround && (ticks - releasedTouch.Tick) < (e.DeviceType == SKTouchDeviceType.Mouse ? ShortClick : longTap) * 10000)
                        {
                            _waitingForDoubleTap = true;
                            if (UseDoubleTap) { await Task.Delay(DelayTap); }

                            if (_numOfTaps > 1)
                            {
                                if (!e.Handled)
                                    e.Handled = OnDoubleTapped(location, _numOfTaps);
                            }
                            else
                            {
                                if (!e.Handled)
                                {
                                    e.Handled = OnSingleTapped(location);
                                }
                            }
                            _numOfTaps = 1;
                            if (_waitingForDoubleTap)
                            {
                                _waitingForDoubleTap = false; ;
                            }
                        }
                        else if (isAround && (ticks - releasedTouch.Tick) >= longTap * 10000)
                        {
                            if (!e.Handled)
                                e.Handled = OnLongTapped(location);
                        }
                    }

                    _flingTracker.RemoveId(e.Id);

                    if (_touches.Count == 1)
                    {
                        e.Handled = OnTouchStart(_touches.Select(t => t.Value.Location).ToList());
                    }

                    if (!e.Handled)
                        e.Handled = OnTouchEnd(_touches.Select(t => t.Value.Location).ToList(), releasedTouch.Location);
                }
                else if (e.ActionType == SKTouchAction.Moved)
                {
                    _touches[e.Id] = new TouchEvent(e.Id, location, ticks);

                    if (e.InContact)
                        _flingTracker.AddEvent(e.Id, location, ticks);

                    if (e.InContact && !e.Handled)
                        e.Handled = OnTouchMove(_touches.Select(t => t.Value.Location).ToList());
                    else
                        e.Handled = OnHovered(_touches.Select(t => t.Value.Location).FirstOrDefault());
                }
                else if (e.ActionType == SKTouchAction.Cancelled)
                {
                    // This gesture is cancelled, so clear all touches
                    _touches.Clear();
                }
                else if (e.ActionType == SKTouchAction.Exited && _touches.TryRemove(e.Id, out var exitedTouch))
                {
                    e.Handled = OnTouchExited(_touches.Select(t => t.Value.Location).ToList(), exitedTouch.Location);
                }
                else if (e.ActionType == SKTouchAction.Entered)
                {
                    e.Handled = OnTouchEntered(_touches.Select(t => t.Value.Location).ToList());
                }
                else if (e.ActionType == SKTouchAction.WheelChanged)
                {
                    if (e.WheelDelta > 0)
                    {
                        OnZoomIn(location);
                    }
                    else
                    {
                        OnZoomOut(location);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
            }
        }

        private bool IsAround(TouchEvent releasedTouch)
        {
            if (_firstTouch == null) { return false; }
            if (releasedTouch.Location == null) { return false; }
            return _firstTouch != null && Utilities.Algorithms.Distance(releasedTouch.Location, _firstTouch) < TouchSlop;
        }

        private void OnGLPaintSurface(object? sender, SKPaintGLSurfaceEventArgs args)
        {
            if (!_initialized && _glView?.GRContext == null)
            {
                // Could this be null before Home is called? If so we should change the logic.
                Logger.Log(LogLevel.Warning, "Refresh can not be called because GRContext is null");
                return;
            }

            // Called on UI thread
            PaintSurface(args.Surface.Canvas);
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            // Called on UI thread
            PaintSurface(args.Surface.Canvas);
        }

        private void PaintSurface(SKCanvas canvas)
        {
            if (PixelDensity <= 0)
                return;

            canvas.Scale(PixelDensity, PixelDensity);

            CommonDrawControl(canvas);
        }

        private MPoint GetScreenPosition(SKPoint point)
        {
            return new MPoint(point.X / PixelDensity, point.Y / PixelDensity);
        }

        /// <summary>
        /// Event handlers
        /// </summary>

        /// <summary>
        /// TouchStart is called, when user press a mouse button or touch the display
        /// </summary>
        public event EventHandler<TouchedEventArgs>? TouchStarted;

        /// <summary>
        /// TouchEnd is called, when user release a mouse button or doesn't touch display anymore
        /// </summary>
        public event EventHandler<TouchedEventArgs>? TouchEnded;

        /// <summary>
        /// TouchEntered is called, when user moves an active touch onto the view
        /// </summary>
        public event EventHandler<TouchedEventArgs>? TouchEntered;

        /// <summary>
        /// TouchExited is called, when user moves an active touch off the view
        /// </summary>
        public event EventHandler<TouchedEventArgs>? TouchExited;

        /// <summary>
        /// TouchMove is called, when user move mouse over map (independent from mouse button state) or move finger on display
        /// </summary>
#if __WPF__
        public new event EventHandler<TouchedEventArgs>? TouchMove;
#else
        public event EventHandler<TouchedEventArgs>? TouchMove;

        /// <summary>
        /// TouchAction is called, when user provoques a touch event
        /// </summary>
        public event EventHandler<SKTouchEventArgs>? TouchAction;
#endif

        /// <summary>
        /// Hover is called, when user move mouse over map without pressing mouse button
        /// </summary>
        public event EventHandler<HoveredEventArgs>? Hovered;

        /// <summary>
        /// Swipe is called, when user release mouse button or lift finger while moving with a certain speed 
        /// </summary>
        public event EventHandler<SwipedEventArgs>? Swipe;

        /// <summary>
        /// Fling is called, when user release mouse button or lift finger while moving with a certain speed, higher than speed of swipe 
        /// </summary>
        public event EventHandler<SwipedEventArgs>? Fling;

        /// <summary>
        /// SingleTap is called, when user clicks with a mouse button or tap with a finger on map 
        /// </summary>
        public event EventHandler<TappedEventArgs>? SingleTap;

        /// <summary>
        /// LongTap is called, when user clicks with a mouse button or tap with a finger on map for 500 ms
        /// </summary>
        public event EventHandler<TappedEventArgs>? LongTap;

        /// <summary>
        /// DoubleTap is called, when user clicks with a mouse button or tap with a finger two or more times on map
        /// </summary>
        public event EventHandler<TappedEventArgs>? DoubleTap;

        /// <summary>
        /// Zoom is called, when map should be zoomed
        /// </summary>
        public event EventHandler<ZoomedEventArgs>? Zoomed;

        /// <summary>
        /// Called, when map should zoom out
        /// </summary>
        /// <param name="screenPosition">Center of zoom out event</param>
        private bool OnZoomOut(MPoint screenPosition)
        {
            if (Map?.ZoomLock ?? true)
            {
                return true;
            }

            var args = new ZoomedEventArgs(screenPosition, ZoomDirection.ZoomOut);

            Zoomed?.Invoke(this, args);

            if (args.Handled)
                return true;

            // Perform standard behavior
            Navigator?.ZoomOut(screenPosition);

            return true;
        }

        /// <summary>
        /// Called, when map should zoom in
        /// </summary>
        /// <param name="screenPosition">Center of zoom in event</param>
        private bool OnZoomIn(MPoint screenPosition)
        {
            if (Map?.ZoomLock ?? true)
            {
                return true;
            }

            var args = new ZoomedEventArgs(screenPosition, ZoomDirection.ZoomIn);

            Zoomed?.Invoke(this, args);

            if (args.Handled)
                return true;

            // Perform standard behavior
            Navigator?.ZoomIn(screenPosition);

            return true;
        }

        /// <summary>
        /// Called, when mouse/finger/pen hovers around
        /// </summary>
        /// <param name="screenPosition">Actual position of mouse/finger/pen</param>
        private bool OnHovered(MPoint? screenPosition)
        {
            if (screenPosition == null)
                return false;
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

            if (args.Handled)
                return true;

            Navigator?.FlingWith(velocityX, velocityY, 1000);

            return true;
        }

        /// <summary>
        /// Called, when mouse/finger/pen click/touch map
        /// </summary>
        /// <param name="touchPoints">List of all touched points</param>
        private bool OnTouchStart(List<MPoint> touchPoints)
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
                _virtualRotation = Viewport.Rotation;
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
        private bool OnTouchEnd(List<MPoint> touchPoints, MPoint releasedPoint)
        {
            var args = new TouchedEventArgs(touchPoints);

            TouchEnded?.Invoke(this, args);

            // Last touch released
            if (touchPoints.Count == 0)
            {
                _mode = TouchMode.None;
                if (_viewport.Extent != null)
                {
                    var fetchInfo = new FetchInfo(_viewport.Extent, _viewport.Resolution, Map?.CRS, ChangeType.Discrete);
                    Map?.RefreshData(fetchInfo);
                }
            }

            return args.Handled;
        }

        /// <summary>
        /// Called when touch enters map
        /// </summary>
        /// <param name="touchPoints">List of all touched points</param>
        private bool OnTouchEntered(List<MPoint> touchPoints)
        {
            // Sanity check
            if (touchPoints.Count == 0)
                return false;

            var args = new TouchedEventArgs(touchPoints);

            TouchEntered?.Invoke(this, args);

            if (args.Handled)
                return true;

            return true;
        }

        /// <summary>
        /// Called when touch exits map
        /// </summary>
        /// <param name="touchPoints">List of all touched points</param>
        /// <param name="releasedPoint">Released point, which was touched before</param>
        private bool OnTouchExited(List<MPoint> touchPoints, MPoint releasedPoint)
        {
            var args = new TouchedEventArgs(touchPoints);

            TouchExited?.Invoke(this, args);

            // Last touch released
            if (touchPoints.Count == 0)
            {
                _mode = TouchMode.None;
                if (_viewport.Extent != null)
                {
                    var fetchInfo = new FetchInfo(_viewport.Extent, _viewport.Resolution, Map?.CRS, ChangeType.Discrete);
                    Map?.RefreshData(fetchInfo);
                }
            }

            return args.Handled;
        }

        /// <summary>
        /// Called, when mouse/finger/pen moves over map
        /// </summary>
        /// <param name="touchPoints">List of all touched points</param>
        private bool OnTouchMove(List<MPoint> touchPoints)
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

                        if (!(Map?.PanLock ?? false) && _previousCenter != null)
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

                        if (Map?.RotationLock == false)
                        {
                            var deltaRotation = angle - prevAngle;
                            _virtualRotation += deltaRotation;

                            rotationDelta = RotationCalculations.CalculateRotationDeltaWithSnapping(
                                _virtualRotation, _viewport.Rotation, _unSnapRotationDegrees, _reSnapRotationDegrees);
                        }
                    
                        if (prevCenter != null)
                            _viewport.Transform(center, prevCenter, (Map?.ZoomLock ?? true) ? 1 : radius / prevRadius, rotationDelta);

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
        private bool OnDoubleTapped(MPoint screenPosition, int numOfTaps)
        {
            var args = new TappedEventArgs(screenPosition, numOfTaps);

            DoubleTap?.Invoke(this, args);

            if (args.Handled)
                return true;

            var eventReturn = InvokeInfo(screenPosition, screenPosition, numOfTaps);

            if (eventReturn?.Handled == true)
                return true;

            // Double tap as zoom
            return OnZoomIn(screenPosition);
        }

        /// <summary>
        /// Called, when mouse/finger/pen tapped on map one time
        /// </summary>
        /// <param name="screenPosition">Clicked/touched position on screen</param>
        /// <returns>True, if the event is handled</returns>
        private bool OnSingleTapped(MPoint screenPosition)
        {
            var args = new TappedEventArgs(screenPosition, 1);

            SingleTap?.Invoke(this, args);

            if (args.Handled)
                return true;

            var infoToInvoke = InvokeInfo(screenPosition, screenPosition, 1);

            if (infoToInvoke?.Handled == true)
                return true;

            OnInfo(infoToInvoke);
            return infoToInvoke?.Handled ?? false;
        }

        /// <summary>
        /// Called, when mouse/finger/pen tapped long on map
        /// </summary>
        /// <param name="screenPosition">Clicked/touched position on screen</param>
        /// <returns>True, if the event is handled</returns>
        private bool OnLongTapped(MPoint screenPosition)
        {
            var args = new TappedEventArgs(screenPosition, 1);

            LongTap?.Invoke(this, args);

            return args.Handled;
        }

        private static (MPoint centre, double radius, double angle) GetPinchValues(List<MPoint> locations)
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

            return (new MPoint(centerX, centerY), radius, angle);
        }

        /// <summary>
        /// Public functions
        /// </summary>

        public void OpenBrowser(string url)
        {
            Launcher.OpenAsync(new Uri(url));
        }

        protected void RunOnUIThread(Action action)
        {
#if __MAUI__ // WORKAROUND for Preview 11 will be fixed in Preview 13 https://github.com/dotnet/maui/issues/3597
            Application.Current?.Dispatcher.Dispatch(action);
#else
            Device.BeginInvokeOnMainThread(action);
#endif
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Map?.Dispose();
            }
            CommonDispose(disposing);
        }

        ~MapControl()
        {
            Dispose(false);
        }

        private float GetPixelDensity()
        {
            if (Width <= 0) return 0;
            if (UseGPU)
                return (float)(_glView!.CanvasSize.Width / Width);
            else
                return (float)(_canvasView!.CanvasSize.Width / Width);
        }
    }
}
