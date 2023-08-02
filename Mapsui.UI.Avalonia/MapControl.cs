using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
using Mapsui.Layers;
using Mapsui.UI.Avalonia.Extensions;

namespace Mapsui.UI.Avalonia;

public partial class MapControl : UserControl, IMapControl, IDisposable
{
    private MPoint? _mousePosition;
    private MapsuiCustomDrawOp? _drawOp;
    private MPoint? _currentMousePosition;
    private MPoint? _downMousePosition;
    private bool _mouseDown;
    private MPoint? _previousMousePosition;
    private double _mouseWheelPos = 0.0;

    public event EventHandler<FeatureInfoEventArgs>? FeatureInfo;

    public MapControl()
    {
        ClipToBounds = true;
        CommonInitialize();
        Initialize();
    }

    private void Initialize()
    {
        _invalidate = () => { RunOnUIThread(InvalidateVisual); };

        Initialized += MapControlInitialized;

        PointerPressed += MapControl_PointerPressed;
        PointerReleased += MapControl_PointerReleased;
        PointerMoved += MapControlMouseMove;
        PointerExited += MapControlMouseLeave;
        PointerCaptureLost += MapControlPointerCaptureLost;

        PointerWheelChanged += MapControlMouseWheel;

        DoubleTapped += OnDoubleTapped;

        KeyDown += MapControl_KeyDown;
        KeyUp += MapControl_KeyUp;
    }

    private void MapControl_KeyUp(object? sender, KeyEventArgs e)
    {
        ShiftPressed = (e.KeyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift;
    }

    public bool ShiftPressed { get; set; }

    private void MapControl_KeyDown(object? sender, KeyEventArgs e)
    {
        ShiftPressed = (e.KeyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift;
    }

    private void MapControlPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        _previousMousePosition = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        switch (change.Property.Name)
        {
            case nameof(Bounds):
                // size changed
                MapControlSizeChanged();
                break;
        } 
    }

    private void MapControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var leftButtonPressed = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        if (HandleTouching(e.GetPosition(this).ToMapsui(), leftButtonPressed, e.ClickCount, ShiftPressed))
        {
            e.Handled = true;
            return;
        }

        if (leftButtonPressed)
        {
            MapControlMouseLeftButtonDown(e);
        }
    }

    private void MapControlMouseWheel(object? sender, PointerWheelEventArgs e)
    {
        // In Avalonia the touchpad can trigger the mousewheel event. In that case there are more events and the Delta.Y is a double value, 
        // which is usually smaller than 1.0. In the code below the deltas are accumelated until they are larger than 1.0. Only then 
        // MouseWheelZoom is called.
        _mouseWheelPos += e.Delta.Y;
        if (Math.Abs(_mouseWheelPos) < 1.0) return; // Ignore the mouse wheel event if the accumulated delta is still too small
        int delta = Math.Sign(_mouseWheelPos);
        _mouseWheelPos -= delta;

        _currentMousePosition = e.GetPosition(this).ToMapsui();
        Map.Navigator.MouseWheelZoom(delta, _currentMousePosition);
    }

    private void MapControlMouseLeftButtonDown(PointerPressedEventArgs e)
    {
        var touchPosition = e.GetPosition(this).ToMapsui();
        _previousMousePosition = touchPosition;
        _downMousePosition = touchPosition;
        _mouseDown = true;
        e.Pointer.Capture(this);
    }

    private void HandleFeatureInfo(PointerReleasedEventArgs e)
    {
        if (FeatureInfo == null) return; // don't fetch if you the call back is not set.

        if (Map != null && _downMousePosition == e.GetPosition(this).ToMapsui())
            foreach (var layer in Map.Layers)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                (layer as IFeatureInfo)?.GetFeatureInfo(Map.Navigator.Viewport, _downMousePosition.X, _downMousePosition.Y,
                    OnFeatureInfo);
            }
    }

    private void OnFeatureInfo(IDictionary<string, IEnumerable<IFeature>> features)
    {
        FeatureInfo?.Invoke(this, new FeatureInfoEventArgs { FeatureInfo = features });
    }

    private void MapControlMouseLeave(object? sender, PointerEventArgs e)
    {
        _previousMousePosition = null;
    }

    private void MapControlMouseMove(object? sender, PointerEventArgs e)
    {
        _currentMousePosition = e.GetPosition(this).ToMapsui(); // Needed for both MouseMove and MouseWheel event

        if (_mouseDown)
        {
            if (_previousMousePosition == null)
            {
                // Usually MapControlMouseLeftButton down initializes _previousMousePosition but in some
                // situations it can be null. So far I could only reproduce this in debug mode when putting
                // a breakpoint and continuing.
                return;
            }

            Map.Navigator.Drag(_currentMousePosition, _previousMousePosition);
            _previousMousePosition = _currentMousePosition;
        }
    }

    private void MapControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var leftButtonPressed = e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased;
        if (HandleTouched(e.GetPosition(this).ToMapsui(), leftButtonPressed, 1, ShiftPressed))
        {
            e.Handled = true;
            return;
        }

        if (leftButtonPressed)
        {
            MapControlMouseLeftButtonUp(e);
        }
    }

    private void MapControlMouseLeftButtonUp(PointerReleasedEventArgs e)
    {
        RefreshData();
        _mouseDown = false;
        _previousMousePosition = null;
        e.Pointer.Capture(null);

        if (IsClick(_currentMousePosition, _downMousePosition))
        {
            HandleFeatureInfo(e);
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
        if (HandleMoving(_mousePosition, true, 0, ShiftPressed))
            e.Handled = true;
    }

    private void OnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        // We have a new interaction with the screen, so stop all navigator animations
        var tapPosition = _mousePosition;
        if (tapPosition != null && HandleTouchingTouched(tapPosition, true, 2, ShiftPressed))
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

    private void RunOnUIThread(Action action)
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
            UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        })) { }
    }

    private float ViewportWidth => Convert.ToSingle(Bounds.Width);
    private float ViewportHeight => Convert.ToSingle(Bounds.Height);

    private float GetPixelDensity()
    {
        if (VisualRoot != null)
        {
            return Convert.ToSingle(VisualRoot.RenderScaling);
        }

        return 1f;
    }

    private sealed class MapsuiCustomDrawOp : ICustomDrawOperation
    {
        private readonly MapControl _mapControl;

        public MapsuiCustomDrawOp(Rect bounds, MapControl mapControl)
        {
            Bounds = bounds;
            _mapControl = mapControl;
        }

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
            _mapControl.CommonDrawControl(canvas);
            canvas.Restore();
        }

        public Rect Bounds { get; set; }

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
