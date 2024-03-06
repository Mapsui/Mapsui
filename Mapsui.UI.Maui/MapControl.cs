using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Manipulations;
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

    private readonly SKGLView? _glView;
    private readonly SKCanvasView? _canvasView;
    private readonly ConcurrentDictionary<long, MPoint> _touches = new();
    private readonly FlingTracker _flingTracker = new();
    private Size _oldSize;
    private static List<WeakReference<MapControl>>? _listeners;
    private readonly ManipulationTracker _manipulationTracker = new();
    private MPoint? _downLocation;

    public MapControl()
    {
        SharedConstructor();

        View view;


        BackgroundColor = KnownColor.White;
        InitTouchesReset(this);


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
            _invalidate = () =>
            {
                // The line below sometimes has a null reference exception on application close.
                RunOnUIThread(() => _glView.InvalidateSurface());
            };
            _glView.PaintSurface += OnGLPaintSurface;
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
            _invalidate = () => { RunOnUIThread(() => _canvasView.InvalidateSurface()); };
            _canvasView.PaintSurface += OnPaintSurface;
            view = _canvasView;
        }
        view.PropertyChanged += View_PropertyChanged;
        Content = view;
    }

    public bool UseDoubleTap { get; set; } = true;
    public bool UseFling { get; set; } = true;

    // See http://grepcode.com/file/repository.grepcode.com/java/ext/com.google.android/android/4.0.4_r2.1/android/view/ViewConfiguration.java#ViewConfiguration.0PRESSED_STATE_DURATION for values
    // If a finger touches down and up it counts as a tap if the distance
    // between the down and up location is smaller then the touch distance.
    // The distance is initialized at 8. How did we get to 8? Well you could
    // read the discussion here: https://github.com/Mapsui/Mapsui/issues/602
    // We basically copied it from the Java source code: https://android.googlesource.com/platform/frameworks/base/+/master/core/java/android/view/ViewConfiguration.java#162
    /// <summary>
    /// The movement allowed between a touch down and touch up in a touch gestures in device independent pixels.
    /// </summary>
    public int MaxTapGestureMovement { get; set; } = 8;
    private double ViewportWidth => Width; // Used in shared code
    private double ViewportHeight => Height; // Used in shared code

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

    private void View_PropertyChanged(object? sender, PropertyChangedEventArgs e)
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

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            e.Handled = true;
            var location = GetScreenPosition(e.Location);

            if (e.ActionType == SKTouchAction.Pressed)
            {
                _touches[e.Id] = location;
                if (_touches.Count == 1)
                    _downLocation = location;

                if (OnWidgetPointerPressed(location, false))
                    return;

                _flingTracker.Clear();

                _manipulationTracker.Restart(_touches.Values.ToArray());
            }
            else if (e.ActionType == SKTouchAction.Moved)
            {
                var isHovering = !e.InContact;

                if (OnWidgetPointerMoved(location, !isHovering, false))
                    return;

                if (isHovering)
                    return;

                _touches[e.Id] = location;

                _flingTracker.AddEvent(e.Id, location, DateTime.Now.Ticks);

                _manipulationTracker.Manipulate(_touches.Values.ToArray(), Map.Navigator.Pinch);

                RefreshGraphics();
            }
            else if (e.ActionType == SKTouchAction.Released)
            {
                // Delete e.Id from _touches, because finger is released
                _touches.Remove(e.Id, out var releasedTouch);

                if (UseFling)
                    _flingTracker.IfFling(e.Id, (vX, vY) => Map.Navigator.Fling(vX, vY, 1000));
                _flingTracker.RemoveId(e.Id);

                if (IsTap(releasedTouch, _downLocation))
                {
                    if (OnWidgetTapped(location, 1, false))
                        return;
                    OnInfo(CreateMapInfoEventArgs(location, location, 1));
                    return;
                }

                _manipulationTracker.Manipulate(_touches.Values.ToArray(), Map.Navigator.Pinch);

                Refresh();
            }
            else if (e.ActionType == SKTouchAction.Cancelled)
            {
                if (!e.InContact)
                    return;

                _touches.Clear();
                Refresh();
            }
            else if (e.ActionType == SKTouchAction.Exited)
            {
                if (!e.InContact)
                    return;

                _touches.Remove(e.Id, out var exitedTouch); // Why not clear?
                Refresh();
            }
            else if (e.ActionType == SKTouchAction.WheelChanged)
            {
                OnZoomInOrOut(e.WheelDelta, location);
            }
        });
    }

    private bool IsTap(MPoint? releasePosition, MPoint? pressedPosition)
    {
        // It is not possible to use the MAUI gesture because it is not triggered when OnTouch is used.
        if (releasePosition == null) return false;
        if (pressedPosition == null) return false;

        // While tapping on screen, there could be a small movement of the finger
        // (especially on Samsung). So check, if touch start location isn't more 
        // than a number of pixels away from touch end location.
        var maxTapGestureMovementInRawPixels = MaxTapGestureMovement * PixelDensity;

        return Algorithms.Distance(releasePosition, pressedPosition) < maxTapGestureMovementInRawPixels;
    }

    private void OnGLPaintSurface(object? sender, SKPaintGLSurfaceEventArgs args)
    {
        if (_glView?.GRContext is null)
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

    private MPoint GetScreenPosition(SKPoint point) => new MPoint(point.X / PixelDensity, point.Y / PixelDensity);

    /// <summary>
    /// Called, when map should zoom in or out
    /// </summary>
    /// <param name="currentMousePosition">Center of zoom out event</param>
    private void OnZoomInOrOut(int mouseWheelDelta, MPoint currentMousePosition)
        => Map.Navigator.MouseWheelZoom(mouseWheelDelta, currentMousePosition);

    /// <summary>
    /// Called, when mouse/finger/pen tapped on map 2 or more times
    /// </summary>
    /// <param name="screenPosition">First clicked/touched position on screen</param>
    protected virtual void OnDoubleTapped(MPoint screenPosition)
    {
        // Zoom in on double tap
        OnZoomInOrOut(1, screenPosition); // mouseWheelDelta > 0 to zoom in
    }

    /// <summary>
    /// Called, when mouse/finger/pen tapped on map one time
    /// </summary>
    /// <param name="screenPosition">Clicked/touched position on screen</param>
    /// <returns>True, if the event is handled</returns>
    protected virtual void OnSingleTapped(MPoint screenPosition)
    {
        OnInfo(CreateMapInfoEventArgs(screenPosition, screenPosition, 1));
    }

    /// <summary>
    /// Public functions
    /// </summary>

    public void OpenInBrowser(string url)
    {
        Catch.TaskRun(() => _ = Launcher.OpenAsync(new Uri(url)));
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
