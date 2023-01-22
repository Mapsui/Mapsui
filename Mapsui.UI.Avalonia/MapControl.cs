using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
using Mapsui.Utilities;

namespace Mapsui.UI.Avalonia;

public partial class MapControl : Grid, IMapControl, IDisposable
{
    private MPoint? _mousePosition;
    private MapsuiCustomDrawOp? _drawOp;
    private MPoint? _currentMousePosition;
    private MPoint? _downMousePosition;
    private bool _mouseDown;
    private MPoint? _previousMousePosition;
    private double _toResolution = double.NaN;
    private double _mouseWheelPos = 0.0;

    public event EventHandler<FeatureInfoEventArgs>? FeatureInfo;
    public MouseWheelAnimation MouseWheelAnimation { get; } = new() { Duration = 0 };

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
        PointerLeave += MapControlMouseLeave;

        PointerWheelChanged += MapControlMouseWheel;

        DoubleTapped += OnDoubleTapped;
    }

    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
    {
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
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            MapControlMouseLeftButtonDown(e);
        }
    }

    private void MapControlMouseWheel(object? sender, PointerWheelEventArgs e)
    {
        if (_map?.ZoomLock ?? true) return;
        if (!Viewport.HasSize()) return;

        _currentMousePosition = e.GetPosition(this).ToMapsui();
        //Needed for both MouseMove and MouseWheel event for mousewheel event

        if (double.IsNaN(_toResolution))
            _toResolution = Viewport.Resolution;

        _mouseWheelPos += e.Delta.Y;
        int delta = (int)_mouseWheelPos;
        bool navigate = false;
        if (_mouseWheelPos >= 1.0)
        {
            _toResolution = ZoomHelper.ZoomIn(_map.Resolutions, _toResolution);
            _mouseWheelPos -= 1.0;
            navigate = true;
        }
        else if (_mouseWheelPos <= -1.0)
        {
            _toResolution = ZoomHelper.ZoomOut(_map.Resolutions, _toResolution);
            _mouseWheelPos += 1.0;
            navigate = true;
        }
        if (!navigate) return;

        var resolution = MouseWheelAnimation.GetResolution(delta, _viewport, _map);
        // Limit target resolution before animation to avoid an animation that is stuck on the max resolution, which would cause a needless delay
        resolution = _map.Limiter.LimitResolution(resolution, Viewport.Width, Viewport.Height, _map.Resolutions, _map.Extent);
        Navigator?.ZoomTo(resolution, _currentMousePosition, MouseWheelAnimation.Duration, MouseWheelAnimation.Easing);
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
                (layer as IFeatureInfo)?.GetFeatureInfo(Viewport, _downMousePosition.X, _downMousePosition.Y,
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

            _viewport.Transform(_currentMousePosition, _previousMousePosition);
            RefreshGraphics();
            _previousMousePosition = _currentMousePosition;
        }
    }

    private void MapControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
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
            OnInfo(InvokeInfo(_mousePosition, _mousePosition, 1));
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
    }

    private void OnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        // We have a new interaction with the screen, so stop all navigator animations
        var tapPosition = _mousePosition;
        OnInfo(InvokeInfo(tapPosition, tapPosition, 2));
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

        private readonly FormattedText _noSkia = new()
        {
            Text = "Current rendering API is not Skia"
        };

        public MapsuiCustomDrawOp(Rect bounds, MapControl mapControl)
        {
            Bounds = bounds;
            _mapControl = mapControl;
        }

        public void Dispose()
        {
            // No-op
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

        public void Render(IDrawingContextImpl context)
        {
            var canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;
            if (canvas == null)
                context.DrawText(Brushes.Black, new Point(), _noSkia.PlatformImpl);
            else
            {
                canvas.Save();
                _mapControl.CommonDrawControl(canvas);
                canvas.Restore();
            }
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
