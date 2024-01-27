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
using Mapsui.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Mapsui.UI.Avalonia;

public partial class MapControl : UserControl, IMapControl, IDisposable
{
    private MPoint? _mousePosition;
    private MapsuiCustomDrawOp? _drawOp;
    private MPoint? _currentMousePosition;
    private MPoint? _pointerDownPosition;
    private bool _mouseDown;
    private MPoint? _previousMousePosition;
    private double _mouseWheelPos = 0.0;

    /// <summary> Virtual Rotation </summary>
    private double _virtualRotation;
    /// <summary> Previous Center for Pinch </summary>
    private MPoint? _previousCenter;
    /// <summary> Saver for angle before last pinch movement </summary>
    private double _previousAngle;
    /// <summary> Saver for radius before last pinch movement </summary>
    private double _previousRadius = 1f;

    // Touch Handling
    private readonly ConcurrentDictionary<long, MPoint> _touches = new();

    [Obsolete("Use Info and ILayerFeatureInfo", true)]
    public event EventHandler<FeatureInfoEventArgs>? FeatureInfo;

    private bool _shiftPressed;

    public MapControl()
    {
        ClipToBounds = true;
        CommonInitialize();
        Initialize();
    }

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
        PointerMoved += MapControlMouseMove;
        PointerExited += MapControlMouseLeave;
        PointerCaptureLost += MapControlPointerCaptureLost;
        PointerWheelChanged += MapControlMouseWheel;
        DoubleTapped += OnDoubleTapped;

        // Needed to track the state of _shiftPressed because DoubleTapped does not have KeyModifiers.
        KeyDown += (s, e) => _shiftPressed = GetShiftPressed(e.KeyModifiers);
        KeyUp += (s, e) => _shiftPressed = GetShiftPressed(e.KeyModifiers);
    }


    private static bool GetShiftPressed(KeyModifiers keyModifiers)
    {
        

        return (keyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift;
    }

    private void MapControlPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        _previousMousePosition = null;
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
        _mouseDown = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        _touches[e.Pointer.Id] = _pointerDownPosition;
        OnPinchStart(_touches.Select(t => t.Value).ToList());

        if (HandleWidgetPointerDown(_pointerDownPosition, _mouseDown, e.ClickCount, _shiftPressed))
        {
            e.Handled = true;
            return;
        }

        if (_mouseDown)
        {
            _previousMousePosition = _pointerDownPosition;
            e.Pointer.Capture(this);
        }
    }

    private void MapControlMouseWheel(object? sender, PointerWheelEventArgs e)
    {
        // In Avalonia the touchpad can trigger the mouse wheel event. In that case there are more events and the Delta.Y is a double value, 
        // which is usually smaller than 1.0. In the code below the deltas are accumulated until they are larger than 1.0. Only then 
        // MouseWheelZoom is called.
        _mouseWheelPos += e.Delta.Y;
        if (Math.Abs(_mouseWheelPos) < 1.0) return; // Ignore the mouse wheel event if the accumulated delta is still too small
        int delta = Math.Sign(_mouseWheelPos);
        _mouseWheelPos -= delta;

        _currentMousePosition = e.GetPosition(this).ToMapsui();
        Map.Navigator.MouseWheelZoom(delta, _currentMousePosition);
    }

    private void MapControlMouseLeave(object? sender, PointerEventArgs e)
    {
        _previousMousePosition = null;
        ClearTouchState();
    }

    private void MapControlMouseMove(object? sender, PointerEventArgs e)
    {
        _currentMousePosition = e.GetPosition(this).ToMapsui(); // Needed for both MouseMove and MouseWheel event
        _touches[e.Pointer.Id] = _currentMousePosition;

        if (_previousMousePosition is null)
            return;

        if (!_mouseDown)
            return;

        if (OnPinchMove(_touches.Select(t => t.Value).ToList()))
        {
            e.Handled = true;
            return;
        }

        Map.Navigator.Drag(_currentMousePosition, _previousMousePosition);
        _previousMousePosition = _currentMousePosition;
    }

    private void MapControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _touches.TryRemove(e.Pointer.Id, out _);

        var leftButtonPressed = e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased;
        if (HandleWidgetPointerUp(e.GetPosition(this).ToMapsui(), _pointerDownPosition, leftButtonPressed, 1, _shiftPressed))
        {
            e.Handled = true;
            return;
        }

        if (leftButtonPressed)
        {
            MapControlMouseLeftButtonUp(e);
        }
        _mouseDown = false;
        _previousMousePosition = null;
        e.Pointer.Capture(null);
    }

    private void MapControlMouseLeftButtonUp(PointerReleasedEventArgs e)
    {
        RefreshData();

        if (IsClick(_currentMousePosition, _pointerDownPosition))
        {
            OnInfo(CreateMapInfoEventArgs(_mousePosition, _mousePosition, 1));
        }
    }

    private static bool IsClick(MPoint? currentPosition, MPoint? previousPosition)
    {
        if (currentPosition == null || previousPosition == null)
            return false;

        return
            Math.Abs(currentPosition.X - previousPosition.X) < 1 &&
            Math.Abs(currentPosition.Y - previousPosition.Y) < 1;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        _mousePosition = e.GetPosition(this).ToMapsui();
        if (HandleWidgetPointerMove(_mousePosition, true, 0, _shiftPressed))
            e.Handled = true;
    }

    private void OnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        // We have a new interaction with the screen, so stop all navigator animations
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

    private bool OnPinchMove(List<MPoint> touchPoints)
    {
        if (touchPoints.Count != 2)
            return false;

        var (prevCenter, prevRadius, prevAngle) = (_previousCenter, _previousRadius, _previousAngle);
        var (center, radius, angle) = GetPinchValues(touchPoints);

        double rotationDelta = 0;

        if (Map.Navigator.RotationLock == false)
        {
            var deltaRotation = angle - prevAngle;
            _virtualRotation += deltaRotation;

            rotationDelta = RotationCalculations.CalculateRotationDeltaWithSnapping(
                _virtualRotation, Map.Navigator.Viewport.Rotation, _unSnapRotationDegrees, _reSnapRotationDegrees);
        }

        if (prevCenter != null)
            Map.Navigator.Pinch(center, prevCenter, radius / prevRadius, rotationDelta);

        (_previousCenter, _previousRadius, _previousAngle) = (center, radius, angle);

        RefreshGraphics();
        return true;
    }

    private void OnPinchStart(List<MPoint> touchPoints)
    {
        if (touchPoints.Count == 2)
        {
            (_previousCenter, _previousRadius, _previousAngle) = GetPinchValues(touchPoints);
            _virtualRotation = Map.Navigator.Viewport.Rotation;
        }
    }

    private static (MPoint center, double radius, double angle) GetPinchValues(List<MPoint> locations)
    {
        if (locations.Count < 2)
            throw new ArgumentOutOfRangeException(nameof(locations));

        double centerX = 0;
        double centerY = 0;

        foreach (var location in locations)
        {
            centerX += location.X;
            centerY += location.Y;
        }

        centerX /= locations.Count;
        centerY /= locations.Count;

        var radius = Algorithms.Distance(centerX, centerY, locations[0].X, locations[0].Y);

        var angle = Math.Atan2(locations[1].Y - locations[0].Y, locations[1].X - locations[0].X) * 180.0 / Math.PI;

        return (new MPoint(centerX, centerY), radius, angle);
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
            _map?.Dispose();
        }

        CommonDispose(disposing);
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
