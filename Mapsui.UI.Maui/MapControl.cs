using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.UI.Utils;
using Mapsui.Utilities;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.UI.Maui;

/// <summary>
/// UI component that displays an interactive map 
/// </summary>
public partial class MapControl : ContentView, IMapControl, IDisposable
{
    // GPU does not work currently on MAUI
    // See https://github.com/mono/SkiaSharp/issues/1893
    // https://github.com/Mapsui/Mapsui/issues/1676
    public static bool UseGPU =
        DeviceInfo.Platform != DevicePlatform.WinUI &&
        DeviceInfo.Platform != DevicePlatform.macOS &&
        DeviceInfo.Platform != DevicePlatform.MacCatalyst &&
        DeviceInfo.Platform != DevicePlatform.Android;

    private SKGLView? _glView;
    private SKCanvasView? _canvasView;

    // See http://grepcode.com/file/repository.grepcode.com/java/ext/com.google.android/android/4.0.4_r2.1/android/view/ViewConfiguration.java#ViewConfiguration.0PRESSED_STATE_DURATION for values
    private const int _shortClick = 250;
    private const int _delayTap = 200;
    // If a finger touches down and up it counts as a tap if the distance
    // between the down and up location is smaller then the touch slob.
    // The slob is initialized at 8. How did we get to 8? Well you could
    // read the discussion here: https://github.com/Mapsui/Mapsui/issues/602
    // We basically copied it from the Java source code: https://android.googlesource.com/platform/frameworks/base/+/master/core/java/android/view/ViewConfiguration.java#162
    private const int _touchSlop = 8;
    protected readonly bool _initialized;
    private readonly ConcurrentDictionary<long, MPoint> _touches = new();
    private MPoint? _pointerDownPosition;
    private bool _waitingForDoubleTap;
    private int _numOfTaps;
    private readonly FlingTracker _flingTracker = new();
    private MPoint? _previousCenter;
    private TouchMode _mode;
    private long _pointerDownTicks;
    private long _pointerUpTicks;
    private bool _widgetPointerDown;
    private Size _oldSize;
    private static List<WeakReference<MapControl>>? _listeners;
    private readonly PinchTracker _pinchTracker = new();

    public MapControl()
    {
        CommonInitialize();
        Initialize();

        _initialized = true;
    }
    
    public bool UseDoubleTap { get; set; } = true;
    public bool UseFling { get; set; } = true;
    private double ViewportWidth => Width; // Used in shared code
    private double ViewportHeight => Height; // Used in shared code

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
            _invalidate = () =>
            {
                // The line below sometimes has a null reference exception on application close.
                RunOnUIThread(() => _glView.InvalidateSurface());
            };
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

        view.PropertyChanged += View_PropertyChanged;

        Content = view;
        BackgroundColor = KnownColor.White;
        InitTouchesReset(this);
    }

    private static void InitTouchesReset(MapControl mapControl)
    {
        try
        {
            if (_listeners == null)
            {
                _listeners = [];
                if (Shell.Current != null)
                {
                    Shell.Current.PropertyChanged -= Shell_PropertyChanged;
                    Shell.Current.PropertyChanged += Shell_PropertyChanged;
                }
            }

            // remove dead references
            foreach (var entry in _listeners.ToArray())
            {
                if (!entry.TryGetTarget(out _))
                {
                    _listeners.Remove(entry);
                }
            }

            // add control to listeners
            _listeners.Add(new WeakReference<MapControl>(mapControl));
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }
    }

    private static void Shell_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            switch (e.PropertyName)
            {
                case nameof(Shell.FlyoutIsPresented):
                    if (_listeners != null)
                        foreach (var entry in _listeners.ToArray())
                        {
                            if (entry.TryGetTarget(out var control))
                            {
                                control.ClearTouchState();
                            }
                            else
                            {
                                _listeners.Remove(entry);
                            }
                        }
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }
    }

    private void View_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Width):
            case nameof(Height):
                var newSize = new Size(Width, Height);

                if (newSize.Width > 0 && newSize.Height > 0 && _oldSize != newSize)
                {
                    _oldSize = newSize;
                    // Maui Workaround because the OnSizeChanged Events don't fire.
                    // Maybe this is a Bug and will be fixed in later versions.
                    OnSizeChanged(this, EventArgs.Empty);
                }

                break;
        }
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        ClearTouchState();
        SetViewportSize();
    }

    private async void OnTouch(object? sender, SKTouchEventArgs e)
    {
        try
        {
            // Save time, when the event occurs
            var ticks = DateTime.Now.Ticks;

            var location = GetScreenPosition(e.Location);

            if (e.Handled) return;

            if (e.ActionType == SKTouchAction.Pressed)
            {
                _widgetPointerDown = false;
                _touches[e.Id] = location;

                if (_touches.Count == 1)
                {
                    // In case of touch we need to check if another finger was not already touching.
                    _pointerDownPosition = location;
                    _pointerDownTicks = DateTime.UtcNow.Ticks;
                }

                if (HandleWidgetPointerDown(location, true, Math.Max(1, _numOfTaps), false))
                {
                    e.Handled = true;
                    _widgetPointerDown = true;
                    return;
                }

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

                e.Handled = OnTouchStart(_touches.Select(t => t.Value).ToList());
            }
            // Delete e.Id from _touches, because finger is released
            else if (e.ActionType == SKTouchAction.Released && _touches.TryRemove(e.Id, out var releasedTouch))
            {
                if (HandleWidgetPointerUp(location, _pointerDownPosition, true, 0, false))
                {
                    e.Handled = true;
                    return;
                }

                if (_touches.IsEmpty)
                {
                    _pointerUpTicks = DateTime.UtcNow.Ticks;

                    // Is this a fling?
                    if (UseFling)
                    {
                        double velocityX;
                        double velocityY;

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
                    if (isAround)
                    {
                        _waitingForDoubleTap = true;
                        if (UseDoubleTap) { await Task.Delay(_delayTap); }

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
                }

                _flingTracker.RemoveId(e.Id);

                if (_touches.Count == 1)
                {
                    e.Handled = OnTouchStart(_touches.Select(t => t.Value).ToList());
                }

                if (!e.Handled)
                    e.Handled = OnTouchEnd(_touches.Select(t => t.Value).ToList());
            }
            else if (e.ActionType == SKTouchAction.Moved)
            {
                if (HandleWidgetPointerMove(location, true, Math.Max(1, _numOfTaps), false))
                {
                    e.Handled = true;
                    return;
                }

                _touches[e.Id] = location;

                if (e.InContact)
                    _flingTracker.AddEvent(e.Id, location, ticks);

                if (e.InContact && !e.Handled && !_widgetPointerDown)
                    e.Handled = OnTouchMove(_touches.Select(t => t.Value).ToList());
            }
            else if (e.ActionType == SKTouchAction.Cancelled)
            {
                // This gesture is cancelled, so clear all touches
                _touches.Clear();
            }
            else if (e.ActionType == SKTouchAction.Exited && _touches.TryRemove(e.Id, out var exitedTouch))
            {
                e.Handled = OnTouchExited(_touches.Select(t => t.Value).ToList());
            }
            else if (e.ActionType == SKTouchAction.WheelChanged)
            {
                OnZoomInOrOut(e.WheelDelta, location);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }
    }

    public bool ShiftPressed { get; set; }

    private bool IsAround(MPoint releasedTouch)
    {
        if (_pointerDownPosition == null) { return false; }
        if (releasedTouch == null) { return false; }
        return _pointerDownPosition != null && Algorithms.Distance(releasedTouch, _pointerDownPosition) < _touchSlop;
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
    /// Called, when map should zoom in or out
    /// </summary>
    /// <param name="currentMousePosition">Center of zoom out event</param>
    private bool OnZoomInOrOut(int mouseWheelDelta, MPoint currentMousePosition)
    {
        Map.Navigator.MouseWheelZoom(mouseWheelDelta, currentMousePosition);

        return true;
    }

    /// <summary>
    /// Called, when mouse/finger/pen flinged over map
    /// </summary>
    /// <param name="velocityX">Velocity in x direction in pixel/second</param>
    /// <param name="velocityY">Velocity in y direction in pixel/second</param>
    private bool OnFlinged(double velocityX, double velocityY)
    {
        Map.Navigator.Fling(velocityX, velocityY, 1000);

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

        if (touchPoints.Count == 2)
        {
            _mode = TouchMode.Zooming;
            _pinchTracker.Restart(touchPoints);    
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
    private bool OnTouchEnd(List<MPoint> touchPoints)
    {
        // Last touch released
        if (touchPoints.Count == 0)
        {
            _mode = TouchMode.None;
            if (Map.Navigator.Viewport.ToExtent() is not null)
            {
                Map?.RefreshData(new FetchInfo(Map.Navigator.Viewport.ToSection(), Map?.CRS, ChangeType.Discrete));
            }
        }

        return false;
    }

    /// <summary>
    /// Called when touch exits map
    /// </summary>
    /// <param name="touchPoints">List of all touched points</param>
    /// <param name="releasedPoint">Released point, which was touched before</param>
    private bool OnTouchExited(List<MPoint> touchPoints)
    {
        // Last touch released
        if (touchPoints.Count == 0)
        {
            _mode = TouchMode.None;
            if (Map.Navigator.Viewport.ToExtent() is not null)
            {
                Map?.RefreshData(new FetchInfo(Map.Navigator.Viewport.ToSection(), Map?.CRS, ChangeType.Discrete));
            }
        }
        return false;
    }

    /// <summary>
    /// Called, when mouse/finger/pen moves over map
    /// </summary>
    /// <param name="touchPoints">List of all touched points</param>
    protected virtual bool OnTouchMove(List<MPoint> touchPoints)
    {
        switch (_mode)
        {
            case TouchMode.Dragging:
                {
                    if (touchPoints.Count != 1)
                        return false;

                    var touchPosition = touchPoints.First();

                    if (_previousCenter != null)
                    {
                        Map.Navigator.Drag(touchPosition, _previousCenter);
                    }

                    _previousCenter = touchPosition;
                }
                break;
            case TouchMode.Zooming:
                {
                    if (touchPoints.Count != 2)
                        return false;

                    _pinchTracker.Update(touchPoints);
                    Map.Navigator.Pinch(_pinchTracker.GetPinchManipulation());

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
    protected virtual bool OnDoubleTapped(MPoint screenPosition, int numOfTaps)
    {
        var eventReturn = CreateMapInfoEventArgs(screenPosition, screenPosition, numOfTaps);

        if (eventReturn?.Handled == true)
            return true;

        // Double tap as zoom
        return OnZoomInOrOut(1, screenPosition); // mouseWheelDelta > 0 to zoom in
    }

    /// <summary>
    /// Called, when mouse/finger/pen tapped on map one time
    /// </summary>
    /// <param name="screenPosition">Clicked/touched position on screen</param>
    /// <returns>True, if the event is handled</returns>
    protected virtual bool OnSingleTapped(MPoint screenPosition)
    {
        var infoToInvoke = CreateMapInfoEventArgs(screenPosition, screenPosition, 1);

        if (infoToInvoke?.Handled == true)
            return true;

        OnInfo(infoToInvoke);
        return infoToInvoke?.Handled ?? false;
    }

    /// <summary>
    /// Public functions
    /// </summary>

    public void OpenBrowser(string url)
    {
        Launcher.OpenAsync(new Uri(url));
    }

    /// <summary>
    /// Clears the Touch State
    /// </summary>
    public void ClearTouchState()
    {
        _touches.Clear();
    }

    protected void RunOnUIThread(Action action)
    {
        Dispatcher.Dispatch(() => Catch.Exceptions(action));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        var weakReference = _listeners?.FirstOrDefault(f => f.TryGetTarget(out var control) && control == this);
        if (weakReference != null)
        {
            _listeners?.Remove(weakReference);
        }

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

    private double GetPixelDensity()
    {
        if (Width <= 0) return 0;

        return UseGPU
            ? _glView!.CanvasSize.Width / Width
            : _canvasView!.CanvasSize.Width / Width;
    }
}
