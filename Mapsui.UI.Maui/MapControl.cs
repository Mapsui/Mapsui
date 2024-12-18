using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Manipulations;
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
    public static bool UseGPU = true;

    private readonly SKGLView? _glView;
    private readonly SKCanvasView? _canvasView;
    private readonly ConcurrentDictionary<long, ScreenPosition> _positions = new();
    private Size _oldSize;
    private static List<WeakReference<MapControl>>? _listeners;
    private readonly ManipulationTracker _manipulationTracker = new();
    private Page? _page;
    private Element? _element;

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
            var position = GetScreenPosition(e.Location);

            if (e.ActionType == SKTouchAction.Pressed)
            {
                _positions[e.Id] = position;
                if (_positions.Count == 1) // Not sure if this check is necessary.
                    _manipulationTracker.Restart(_positions.Values.ToArray());

                if (OnMapPointerPressed(_positions.Values.ToArray()))
                    return;
            }
            else if (e.ActionType == SKTouchAction.Moved)
            {
                var isHovering = !e.InContact;

                if (isHovering)
                {
                    // In case of hovering we need to send the current position which added to the _positions array
                    if (OnMapPointerMoved([position], isHovering))
                        return;
                }
                else
                {
                    _positions[e.Id] = position;

                    if (OnMapPointerMoved(_positions.Values.ToArray(), isHovering))
                        return;

                    _manipulationTracker.Manipulate(_positions.Values.ToArray(), Map.Navigator.Manipulate);
                }

                RefreshGraphics();
            }
            else if (e.ActionType == SKTouchAction.Released)
            {
                // Delete e.Id from _touches, because finger is released
                _positions.Remove(e.Id, out var releasedTouch);
                OnMapPointerReleased([position]);
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

    private ScreenPosition GetScreenPosition(SKPoint point) => new ScreenPosition(point.X / PixelDensity, point.Y / PixelDensity);

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
}
