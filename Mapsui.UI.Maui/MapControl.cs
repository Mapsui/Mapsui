using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Manipulations;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using Mapsui.UI.Maui.Drawing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Mapsui.UI.Maui;

/// <summary>
/// UI component that displays an interactive map 
/// </summary>
public partial class MapControl : ContentView, IMapControl, IDisposable
{
    /// <summary>Default backend for new parameterless controls. Read once during construction.</summary>
    public static RenderBackend Backend { get; set; } = RenderBackend.Gpu;

    /// <summary>
    /// Compatibility setting for <see cref="Backend"/>. Setting true selects <see cref="RenderBackend.Gpu"/>;
    /// setting false selects <see cref="RenderBackend.Software"/>. Existing controls are unaffected.
    /// </summary>
    public static bool UseGPU
    {
        get => Backend != RenderBackend.Software;
        set => Backend = value ? RenderBackend.Gpu : RenderBackend.Software;
    }

    /// <summary>
    /// The backend selected for this instance. AndroidSurfaceView maps to Gpu on other platforms.
    /// Changing the static default does not change this control or its lifecycle behavior.
    /// Native initialization errors are reported as errors, without substituting a different backend.
    /// </summary>
    public RenderBackend ActiveBackend { get; }

    private readonly IMapDrawingView _drawingView;
    private readonly Action _invalidateDrawingView;
    private bool _disposed;
    private readonly ConcurrentDictionary<long, PointerRecording> _positions = new();
    private long _nextPressOrder;
    private static List<WeakReference<MapControl>>? _listeners;
    private readonly ManipulationTracker _manipulationTracker = new();

    /// <summary>
    /// If finger position is not updated during the IsStaleTimeSpan period, the touch event is considered stale and is removed.
    /// Touch input is not always reliable. This could be because of bugs in SkiaSharp, WinUI, iOS or Android, MAUI, 
    /// in hardware drivers, or hardware. To work around this we remove the touch events if they did not change after 
    /// some period. Making this period too short could remove valid events, making it too long would result in a longer 
    /// period of dangling ghost touches. You might want to tweak this value to your needs.
    /// </summary>
    public TimeSpan IsStaleTimeSpan { get; set; } = TimeSpan.FromMilliseconds(500); // Even with a value of 100 I never see removal of a valid event, so I assume 500 is save. And perhaps it could be set even lower because if a valid event is removed sometimes I don't notice any change in the UI.
    /// <summary>Constructs a control using the current <see cref="Backend"/> default (Gpu initially).</summary>
    public MapControl() : this(Backend) { }

    /// <summary>Constructs a control with a fixed backend, independently of the static default.</summary>
    /// <param name="backend">The backend to use. AndroidSurfaceView maps to Gpu on non-Android platforms.</param>
    public MapControl(RenderBackend backend)
    {
        (ActiveBackend, _drawingView) = MapDrawingViewFactory.Create(backend);
        _invalidateDrawingView = InvalidateDrawingView;
        BackgroundColor = KnownColor.White;
        InitTouchesReset(this);
        _drawingView.Touch += OnTouch;
        _drawingView.Paint += OnPaint;
        _drawingView.SizeChanged += View_SizeChanged;
        Content = _drawingView.View;
        SharedConstructor();
        Mapsui.Utilities.PlatformUtilities.SetOpenInBrowserFunc(OpenInBrowserStatic);
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        // Connect the dedicated host before the parent's handler maps Content to a native view.
        if (!_disposed && args.NewHandler?.MauiContext is { } context)
            _drawingView.Connect(context);
        base.OnHandlerChanging(args);
    }

    public void InvalidateCanvas()
    {
        if (!_disposed)
            RunOnUIThread(_invalidateDrawingView);
    }

    private void InvalidateDrawingView()
    {
        if (!_disposed)
            _drawingView.Invalidate();
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
                _positions[e.Id] = new PointerRecording(position, Environment.TickCount64, _nextPressOrder++);
                var positions = GetScreenPositions();
                if (positions.Length == 1) // Not sure if this check is necessary.
                    _manipulationTracker.Restart(positions);

                if (OnPointerPressed(positions))
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
                    var pressOrder = _positions.TryGetValue(e.Id, out var previous) ? previous.PressOrder : _nextPressOrder++;
                    _positions[e.Id] = new PointerRecording(position, Environment.TickCount64, pressOrder);
                    var positions = GetScreenPositions();

                    if (OnPointerMoved(positions, isHovering))
                        return;

                    _manipulationTracker.Manipulate(positions, Map.Navigator.Manipulate);
                }
            }
            else if (e.ActionType == SKTouchAction.Released)
            {
                // Delete e.Id from _touches, because finger is released
                _positions.Remove(e.Id, out var releasedTouch);
                OnPointerReleased([position]);
            }
            else if (e.ActionType == SKTouchAction.Cancelled)
            {
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

    private ScreenPosition[] GetScreenPositions()
    {
        // Sorted explicitly because the enumeration order of a ConcurrentDictionary is not defined. The
        // ManipulationTracker derives the rotation from the first two positions, so if two fingers swapped places
        // between events the map would rotate 180 degrees. This happened on iOS with UIEvent.AllTouches, see #2738.
        return _positions.Values.OrderBy(p => p.PressOrder).Select(p => p.ScreenPosition).ToArray();
    }

    private static void RemoveStale(ConcurrentDictionary<long, PointerRecording> positions, double totalMilliseconds)
    {
        var currentTickCount = Environment.TickCount64;
        foreach (var position in positions)
        {
            if (currentTickCount - position.Value.Timestamp > totalMilliseconds)
            {
                _ = positions.TryRemove(position.Key, out _);
            }
        }
    }

    private void OnPaint(SKCanvas canvas, object? context)
    {
        if (_disposed)
            return;
        if (ActiveBackend != RenderBackend.Software && context is null)
        {
            _renderController?.Render(canvas, null); // Complete invalidation even before a GPU context exists.
            return;
        }

        // A replacement GL context invalidates the persistent surface owned by RenderService.
        if (!ReferenceEquals(Map.RenderService.GpuContext, context))
            _renderController?.RefreshGraphics();
        Map.RenderService.GpuContext = context;
        _renderController?.Render(canvas, GetPixelDensity());
    }

    private static void OpenInBrowserStatic(string url)
    {
        Catch.TaskRun(() => _ = Launcher.OpenAsync(new Uri(url)));
    }
    internal static ScreenPosition GetScreenPosition(SKPoint point, float pixelDensity) =>
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
        if (disposing && !_disposed)
        {
            _disposed = true;
            var weakReference = _listeners?.FirstOrDefault(f => f.TryGetTarget(out var control) && control == this);
            if (weakReference != null)
            {
                _listeners?.Remove(weakReference);
            }

            _drawingView.Touch -= OnTouch;
            _drawingView.Paint -= OnPaint;
            _drawingView.SizeChanged -= View_SizeChanged;
            _drawingView.Dispose();
            ClearTouchState();
            Map?.Dispose();

#if ANDROID
            DisposeAndroid();
#endif
            SharedDispose(disposing);

            Handler?.DisconnectHandler();
            var content = Content;
            if (content != null)
            {
                if (content is VisualElement contentElement)
                {
                    contentElement.Handler?.DisconnectHandler();
                }
                Content = null;
            }
        }
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
        return _drawingView.CanvasSize.Width;
    }

    private static bool GetShiftPressed() => false; // Work in progress: https://github.com/dotnet/maui/issues/16202

    private record struct PointerRecording(ScreenPosition ScreenPosition, long Timestamp, long PressOrder)
    {
    }

}
