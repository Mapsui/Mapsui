using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Manipulations;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace Mapsui.UI.Maui;

/// <summary>
/// UI component that displays an interactive map 
/// </summary>
public partial class MapControl : ContentView, IMapControl, IDisposable
{
    public static bool UseGPU = !IsMaui9();

    private readonly SKGLView? _glView;
    private readonly SKCanvasView? _canvasView;
    private readonly ConcurrentDictionary<long, PointerRecording> _positions = new();
    private static List<WeakReference<MapControl>>? _listeners;
    private readonly ManipulationTracker _manipulationTracker = new();
    private Page? _page;
    private Element? _element;

    /// <summary>
    /// If finger position is not updated during the IsStaleTimeSpan period, the touch event is considered stale and is removed.
    /// Touch input is not always reliable. This could be because of bugs in SkiaSharp, WinUI, iOS or Android, MAUI, 
    /// in hardware drivers, or hardware. To work around this we remove the touch events if they did not change after 
    /// some period. Making this period too short could remove valid events, making it too long would result in a longer 
    /// period of dangling ghost touches. You might want to tweak this value to your needs.
    /// </summary>
    public TimeSpan IsStaleTimeSpan { get; set; } = TimeSpan.FromMilliseconds(500); // Even with a value of 100 I never see removal of a valid event, so I assume 500 is save. And perhaps it could be set even lower because if a valid event is removed sometimes I don't notice any change in the UI.

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
            _canvasView.PaintSurface += OnPaintSurface;
            view = _canvasView;
        }
        view.SizeChanged += View_SizeChanged;
        Content = view;
    }

    public void InvalidateCanvas()
    {
        if (_glView is SKGLView glView)
            RunOnUIThread(glView.InvalidateSurface);
        else if (_canvasView is SKCanvasView canvasView)
            RunOnUIThread(canvasView.InvalidateSurface);
        else
            throw new InvalidOperationException("Neither SKGLView nor SKCanvasView is initialized.");
    }

    private static bool IsMaui9()
    {
        var frameworkDescription = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
        return frameworkDescription.Contains(".NET 9");
    }

    private void View_SizeChanged(object? sender, EventArgs e)
    {
        ClearTouchState();
        SharedOnSizeChanged(Width, Height);
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

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            e.Handled = true;

            if (GetPixelDensity() is not float pixelDensity)
                return;
            RemoveStale(_positions, IsStaleTimeSpan.TotalMilliseconds);
            var position = GetScreenPosition(e.Location, pixelDensity);

            if (e.ActionType == SKTouchAction.Pressed)
            {
                _positions[e.Id] = new PointerRecording(position, Environment.TickCount);
                if (_positions.Count == 1) // Not sure if this check is necessary.
                    _manipulationTracker.Restart(_positions.Values.Select(p => p.ScreenPosition).ToArray());

                if (OnPointerPressed(_positions.Values.Select(p => p.ScreenPosition).ToArray()))
                    return;
            }
            else if (e.ActionType == SKTouchAction.Moved)
            {
                var isHovering = !e.InContact;

                if (isHovering)
                {
                    // In case of hovering we need to send the current position which added to the _positions array
                    if (OnPointerMoved([position], isHovering))
                        return;
                }
                else
                {
                    _positions[e.Id] = new PointerRecording(position, Environment.TickCount);

                    if (OnPointerMoved(_positions.Values.Select(p => p.ScreenPosition).ToArray(), isHovering))
                        return;

                    _manipulationTracker.Manipulate(_positions.Values.Select(p => p.ScreenPosition).ToArray(), Map.Navigator.Manipulate);
                }

                RefreshGraphics();
            }
            else if (e.ActionType == SKTouchAction.Released)
            {
                // Delete e.Id from _touches, because finger is released
                _positions.Remove(e.Id, out var releasedTouch);
                OnPointerReleased([position]);
            }
            else if (e.ActionType == SKTouchAction.Cancelled)
            {
                if (!e.InContact)
                    return;

                _positions.Clear();
                Refresh();
            }
            else if (e.ActionType == SKTouchAction.Exited)
            {
                if (!e.InContact)
                    return;

                _positions.Remove(e.Id, out var exitedTouch); // Why not clear?
                Refresh();
            }
            else if (e.ActionType == SKTouchAction.WheelChanged)
            {
                OnZoomInOrOut(e.WheelDelta, position);
            }
        });
    }

    private static void RemoveStale(ConcurrentDictionary<long, PointerRecording> positions, double totalMilliseconds)
    {
        var currentTickCount = Environment.TickCount;
        foreach (var position in positions)
        {
            if (currentTickCount - position.Value.timestamp > totalMilliseconds)
            {
                _ = positions.TryRemove(position.Key, out _);
            }
        }
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
        if (GetPixelDensity() is not float pixelDensity)
            return;

        canvas.Scale(pixelDensity, pixelDensity);

        _renderController?.Render(canvas);
    }

    private ScreenPosition GetScreenPosition(SKPoint point, float pixelDensity) =>
        new(point.X / pixelDensity, point.Y / pixelDensity);

    private void OnZoomInOrOut(int mouseWheelDelta, ScreenPosition currentMousePosition)
        => Map.Navigator.MouseWheelZoom(mouseWheelDelta, currentMousePosition);

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
        _positions.Clear();
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

        if (_element != null)
        {
            _element.ParentChanged -= Element_ParentChanged;
            _element = null;
        }

        if (_page != null)
        {
            _page.Appearing -= Page_Appearing;
            _page = null;
        }

        SharedDispose(disposing);
    }

    ~MapControl()
    {
        Dispose(false);
    }

    public float? GetPixelDensity()
    {
        if (Width <= 0)
            return null;

        if (GetCanvasWidth() <= 0)
            return null;

        return (float)(GetCanvasWidth() / Width);
    }

    private double GetCanvasWidth()
    {
        return _glView?.CanvasSize.Width ?? _canvasView!.CanvasSize.Width;
    }

    private static bool GetShiftPressed() => false; // Work in progress: https://github.com/dotnet/maui/issues/16202

    // Workaround for Android Not displaying Map on second time Display on Gpu
    // https://github.com/mono/SkiaSharp/pull/3076
    protected override void OnParentSet()
    {
        base.OnParentSet();
        AttachToOnAppearing();
    }

    private void AttachToOnAppearing()
    {
        if (UseGPU && DeviceInfo.Platform == DevicePlatform.Android)
        {
            if (Parent != null)
            {
                _page = GetPage(Parent);
                if (_page != null)
                {
                    _page.Appearing += Page_Appearing;
                }
            }
        }
    }

    private void Page_Appearing(object? sender, EventArgs e)
    {
        IsVisible = false;
        IsVisible = true;
    }

    private void Element_ParentChanged(object? sender, EventArgs e)
    {
        if (_element != null)
        {
            _element.ParentChanged -= Element_ParentChanged;
            _element = null;
        }

        AttachToOnAppearing();
    }

    private Page? GetPage(Element? element)
    {
        if (element == null)
        {
            return null;
        }

        if (element is Page page)
        {
            return page;
        }

        if (element.Parent == null)
        {
            _element = element;
            _element.ParentChanged += Element_ParentChanged;
            return null;
        }

        return GetPage(element.Parent);
    }

    private record struct PointerRecording(ScreenPosition ScreenPosition, int timestamp)
    {
    }
}
