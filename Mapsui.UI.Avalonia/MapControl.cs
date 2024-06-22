using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using Mapsui.Extensions;
using Mapsui.Manipulations;
using Mapsui.UI.Avalonia.Extensions;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Mapsui.UI.Avalonia;

public partial class MapControl : UserControl, IMapControl, IDisposable
{
    private MapsuiCustomDrawOperation? _drawOperation;
    private double _mouseWheelPos = 0.0;
    private readonly ConcurrentDictionary<long, ScreenPosition> _positions = new();
    private bool _shiftPressed;
    private readonly ManipulationTracker _manipulationTracker = new();

    public MapControl()
    {
        SharedConstructor();

        _invalidate = () => { RunOnUIThread(InvalidateVisual); };

        Initialized += MapControlInitialized;

        // Pointer events
        PointerPressed += MapControl_PointerPressed;
        PointerReleased += MapControl_PointerReleased;
        PointerMoved += MapControl_PointerMoved;
        PointerExited += MapControl_PointerExited;
        PointerCaptureLost += MapControl_PointerCaptureLost;
        PointerWheelChanged += MapControl_PointerWheelChanged;

        // Needed to track the state of _shiftPressed because DoubleTapped does not have KeyModifiers.
        KeyDown += (s, e) => _shiftPressed = GetShiftPressed(e.KeyModifiers);
        KeyUp += (s, e) => _shiftPressed = GetShiftPressed(e.KeyModifiers);

        ClipToBounds = true;
    }

    public static readonly DirectProperty<MapControl, Map> MapProperty =
    AvaloniaProperty.RegisterDirect<MapControl, Map>(nameof(Map), o => o.Map, (o, v) => o.Map = v);

    /// <summary> Clears the Touch State. Should only be called if the touch state seems out of sync 
    /// in a certain situation.</summary>

    // Todo: Figure out if we need to clear the entire state, or only remove a specific pointer.
    public void ClearTouchState() => _positions.Clear();

    private static bool GetShiftPressed(KeyModifiers keyModifiers)
        => (keyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift;

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
        if (IsHovering(e))
            return;

        var position = e.GetPosition(this).ToScreenPosition();
        _positions[e.Pointer.Id] = position;

        if (_positions.Count == 1) // Not sure if this check is necessary.
            _manipulationTracker.Restart(_positions.Values.ToArray());

        if (OnMapPointerPressed(_positions.Values.ToArray()))
            return;

        e.Pointer.Capture(this);
    }

    private void MapControl_PointerMoved(object? sender, PointerEventArgs e)
    {
        var isHovering = IsHovering(e);

        var position = e.GetPosition(this).ToScreenPosition();
        if (isHovering)
        {
            if (OnMapPointerMoved([position], isHovering))
                return;
        }
        else
        {
            _positions[e.Pointer.Id] = position;
            if (OnMapPointerMoved(_positions.Values.ToArray(), isHovering))
                return;
            _manipulationTracker.Manipulate(_positions.Values.ToArray(), Map.Navigator.Manipulate);
        }
        RefreshGraphics();
    }

    private void MapControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _positions.TryRemove(e.Pointer.Id, out _);
        var position = e.GetPosition(this).ToScreenPosition();

        OnMapPointerReleased([position]);

        e.Pointer.Capture(null);
    }

    private bool IsHovering(PointerEventArgs e)
    {
        return e.Pointer.Type == PointerType.Mouse && !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
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

        Map.Navigator.MouseWheelZoom(delta, e.GetPosition(this).ToScreenPosition());
    }

    private void MapControl_PointerExited(object? sender, PointerEventArgs e)
    {
        ClearTouchState();
    }

    private void MapControl_PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        ClearTouchState();
    }

    public override void Render(DrawingContext context)
    {
        _drawOperation ??= new MapsuiCustomDrawOperation(new Rect(0, 0, Bounds.Width, Bounds.Height), this);
        _drawOperation.Bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        context.Custom(_drawOperation);
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

    public void OpenInBrowser(string url)
    {
        Catch.TaskRun(() =>
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? url : "open",
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"-e {url}" : "",
                CreateNoWindow = true,
                UseShellExecute = !RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            });
        });
    }

    private double ViewportWidth => Bounds.Width;
    private double ViewportHeight => Bounds.Height;

    private double GetPixelDensity()
    {
        return VisualRoot?.RenderScaling ?? 1d;
    }

    private sealed class MapsuiCustomDrawOperation(Rect bounds, MapControl mapControl) : ICustomDrawOperation
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
            _drawOperation?.Dispose();
            Map?.Dispose();
        }

        CommonDispose(disposing);
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private bool GetShiftPressed() => _shiftPressed;
}
