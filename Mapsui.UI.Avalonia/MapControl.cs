using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using Mapsui.Extensions;
using Mapsui.UI.Avalonia.Extensions;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Mapsui.UI.Avalonia;

public partial class MapControl : UserControl, IMapControl, IDisposable
{
    private MPoint? _mousePosition;
    private MapsuiCustomDrawOp? _drawOp;
    private MPoint? _pointerDownPosition;
    private double _mouseWheelPos = 0.0;
    private readonly ConcurrentDictionary<long, MPoint> _touches = new();
    private bool _shiftPressed;
    private readonly TouchTracker _touchTracker = new ();

    public MapControl()
    {
        ClipToBounds = true;
        CommonInitialize();
        Initialize();
    }

    public static readonly DirectProperty<MapControl, Map> MapProperty =
    AvaloniaProperty.RegisterDirect<MapControl, Map>(nameof(Map), o => o.Map, (o, v) => o.Map = v);

    /// <summary> Clears the Touch State </summary>
    public void ClearTouchState()
    {
        _touches.Clear();
    }

    private void Initialize()
    {
        _invalidate = () => { RunOnUIThread(InvalidateVisual); };

        Initialized += MapControlInitialized;

        // Pointer events
        PointerPressed += MapControl_PointerPressed;
        PointerReleased += MapControl_PointerReleased;
        PointerMoved += MapControl_PointerMoved;
        PointerExited += MapControl_PointerExited;
        PointerCaptureLost += MapControl_PointerCaptureLost;
        PointerWheelChanged += MapControl_PointerWheelChanged;

        Tapped += MapControl_Tapped;
        DoubleTapped += MapControl_DoubleTapped;


        // Needed to track the state of _shiftPressed because DoubleTapped does not have KeyModifiers.
        KeyDown += (s, e) => _shiftPressed = GetShiftPressed(e.KeyModifiers);
        KeyUp += (s, e) => _shiftPressed = GetShiftPressed(e.KeyModifiers);
    }

    private void MapControl_Tapped(object? sender, TappedEventArgs e)
    {        
        var tapPosition = e.GetPosition(this).ToMapsui();
        if (tapPosition != null && HandleTouchingTouched(tapPosition, _pointerDownPosition, true, 2, _shiftPressed))
        {
            e.Handled = true;
            return;
        }
        OnInfo(CreateMapInfoEventArgs(tapPosition, tapPosition, 2));
    }

    private static bool GetShiftPressed(KeyModifiers keyModifiers)
    {
        return (keyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift;
    }

    private void MapControl_PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        ClearTouchState();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        switch (change.Property.Name)
        {
            case nameof(Bounds):
                // Size changed
                MapControlSizeChanged();
                break;
        }
    }

    private void MapControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _pointerDownPosition = e.GetPosition(this).ToMapsui();
        var mouseDown = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        _touches[e.Pointer.Id] = _pointerDownPosition;
        
        _touchTracker.Restart(_touches.Select(t => t.Value).ToArray());
        Map.Navigator.Pinch(_touchTracker.GetTouchManipulation());

        if (HandleWidgetPointerDown(_pointerDownPosition, mouseDown, e.ClickCount, _shiftPressed))
        {
            e.Handled = true;
            return;
        }

        if (mouseDown)
        {
            e.Pointer.Capture(this);
        }
    }

    private void MapControl_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        // In Avalonia the touchpad can trigger the mouse wheel event. In that case there are more events and the Delta.Y is a double value, 
        // which is usually smaller than 1.0. In the code below the deltas are accumulated until they are larger than 1.0. Only then 
        // MouseWheelZoom is called.
        _mouseWheelPos += e.Delta.Y;
        if (Math.Abs(_mouseWheelPos) < 1.0) return; // Ignore the mouse wheel event if the accumulated delta is still too small
        int delta = Math.Sign(_mouseWheelPos);
        _mouseWheelPos -= delta;

        Map.Navigator.MouseWheelZoom(delta, e.GetPosition(this).ToMapsui());
    }

    private void MapControl_PointerExited(object? sender, PointerEventArgs e)
    {
        ClearTouchState();
    }

    private void MapControl_PointerMoved(object? sender, PointerEventArgs e)
    {
        _mousePosition = e.GetPosition(this).ToMapsui();
        if (HandleWidgetPointerMove(_mousePosition, true, 0, _shiftPressed))
            e.Handled = true;

        if (e.Pointer.Type == PointerType.Mouse && !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _touches[e.Pointer.Id] = e.GetPosition(this).ToMapsui();

        _touchTracker.Update(_touches.Select(t => t.Value).ToArray());
        Map.Navigator.Pinch(_touchTracker.GetTouchManipulation());
        RefreshGraphics();
    }

    private void MapControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _touches.TryRemove(e.Pointer.Id, out _);
        e.Pointer.Capture(null);
    }


    private void MapControl_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        var tapPosition = _mousePosition;
        if (tapPosition != null && HandleTouchingTouched(tapPosition, _pointerDownPosition, true, 2, _shiftPressed))
        {
            e.Handled = true;
            return;
        }
        OnInfo(CreateMapInfoEventArgs(tapPosition, tapPosition, 2));
    }

    public override void Render(DrawingContext context)
    {
        _drawOp ??= new MapsuiCustomDrawOp(new Rect(0, 0, Bounds.Width, Bounds.Height), this);
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

    private static void RunOnUIThread(Action action)
    {
        Catch.TaskRun(() => Dispatcher.UIThread.InvokeAsync(action));
    }

    public void OpenBrowser(string url)
    {
        using (Process.Start(new ProcessStartInfo
        {
            FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? url : "open",
            Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"-e {url}" : "",
            CreateNoWindow = true,
            UseShellExecute = !RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
        })) { }
    }

    private double ViewportWidth => Bounds.Width;
    private double ViewportHeight => Bounds.Height;

    private double GetPixelDensity()
    {
        return VisualRoot?.RenderScaling ?? 1d;
    }
    private sealed class MapsuiCustomDrawOp(Rect bounds, MapControl mapControl) : ICustomDrawOperation
    {
        public void Dispose()
        {
            // No-op
        }

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null)
                return;
            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;
            canvas.Save();
            mapControl.CommonDrawControl(canvas);
            canvas.Restore();
        }

        public Rect Bounds { get; set; } = bounds;

        public bool HitTest(Point p)
        {
            return true;
        }

        public bool Equals(ICustomDrawOperation? other)
        {
            return false;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _drawOp?.Dispose();
            Map?.Dispose();
        }

        CommonDispose(disposing);
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
