using Mapsui.Extensions;
using Mapsui.Manipulations;
using Mapsui.UI.Blazor.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SkiaSharp;
using SkiaSharp.Views.Blazor;
using Microsoft.AspNetCore.Components;

namespace Mapsui.UI.Blazor;

public partial class MapControl : ComponentBase, IMapControl
{
    protected SKCanvasView? _viewCpu;
    protected SKGLView? _viewGpu;
    protected readonly string _elementId = Guid.NewGuid().ToString("N");
    private SKImageInfo? _canvasSize;
    private bool _onLoaded;
    private readonly HashSet<string> _pressedKeys = [];
    private double _pixelDensityFromInterop = 1;
    private BoundingClientRect _clientRect = new();
    private MapsuiJsInterop? _interop;
    private readonly ManipulationTracker _manipulationTracker = new();
    private readonly TapGestureTracker _tapGestureTracker = new();

    [Inject]
    private IJSRuntime? JsRuntime { get; set; }
    public static bool UseGPU { get; set; } = false;
    public string MoveCursor { get; set; } = Cursors.Move;
    public int MoveButton { get; set; } = MouseButtons.Primary;
    public int MoveModifier { get; set; } = Keys.None;
    public int ZoomButton { get; set; } = MouseButtons.Primary;
    public int ZoomModifier { get; set; } = Keys.Control;
    public string ElementId => _elementId;
    private MapsuiJsInterop? Interop =>
            _interop == null && JsRuntime != null
                ? _interop ??= new MapsuiJsInterop(JsRuntime)
                : _interop;

    public MapControl()
    {
        SharedConstructor();

        _invalidate = () =>
        {
            if (_viewCpu != null)
                _viewCpu?.Invalidate();
            else
                _viewGpu?.Invalidate();
        };
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        RefreshGraphics();
    }

    protected void OnKeyDown(KeyboardEventArgs e)
    {
        _pressedKeys.Add(e.Code);
    }

    protected void OnKeyUp(KeyboardEventArgs e)
    {
        _pressedKeys.Remove(e.Code);
    }

    protected void OnPaintSurfaceCPU(SKPaintSurfaceEventArgs e)
    {
        // the the canvas and properties
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        OnPaintSurface(canvas, info);
    }

    protected void OnPaintSurfaceGPU(SKPaintGLSurfaceEventArgs e)
    {
        // the the canvas and properties
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        OnPaintSurface(canvas, info);
    }

    protected void OnPaintSurface(SKCanvas canvas, SKImageInfo info)
    {
        // On Loaded Workaround
        if (!_onLoaded)
        {
            _onLoaded = true;
            OnLoadComplete();
        }

        // Size changed Workaround
        if (_canvasSize?.Width != info.Width || _canvasSize?.Height != info.Height)
        {
            _canvasSize = info;
            OnSizeChanged();
        }

        CommonDrawControl(canvas);
    }

    private void OnLoadComplete()
    {
        Catch.Exceptions(async () =>
        {
            SetViewportSize();
            await InitializingInteropAsync();
        });
    }

    protected void OnMouseWheel(WheelEventArgs e)
    {
        var mouseWheelDelta = (int)e.DeltaY * -1; // so that it zooms like on windows
        var mouseLocation = e.ToLocation(_clientRect);
        Map.Navigator.MouseWheelZoom(mouseWheelDelta, mouseLocation);
    }

    private async Task<BoundingClientRect> BoundingClientRectAsync()
    {
        if (Interop == null)
        {
            throw new ArgumentException("Interop is null");
        }

        return await Interop.BoundingClientRectAsync(_elementId);
    }

    private async Task InitializingInteropAsync()
    {
        if (Interop == null)
        {
            throw new ArgumentException("Interop is null");
        }

        await Interop.DisableMouseWheelAsync(_elementId);
        await Interop.DisableTouchAsync(_elementId);
        _pixelDensityFromInterop = await Interop.GetPixelDensityAsync();
    }

    private void OnSizeChanged()
    {
        SetViewportSize();
        _ = UpdateBoundingRectAsync();
    }

    private async Task UpdateBoundingRectAsync()
    {
        _clientRect = await BoundingClientRectAsync();
    }

    private protected static void RunOnUIThread(Action action)
    {
        // Only one thread is active in WebAssembly.
        action();
    }

    protected void OnDblClick(MouseEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            var location = e.ToLocation(_clientRect);
            if (OnWidgetTapped(location, 2, GetShiftPressed()))
                return;
            OnInfo(CreateMapInfoEventArgs(location, location, 1));
        });
    }

    protected void OnMouseDown(MouseEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            // The client rect needs updating for scrolling. I would rather do that on the onscroll event but it does not fire on this element.
            _ = UpdateBoundingRectAsync();

            var location = e.ToLocation(_clientRect);
            _tapGestureTracker.SetDownPosition(location);
            _manipulationTracker.Restart([]);

            if (OnWidgetPointerPressed(location, GetShiftPressed()))
                return;
        });
    }

    protected void OnMouseMove(MouseEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            var isHovering = !IsMouseButtonPressed(e);
            var position = e.ToLocation(_clientRect);
            _tapGestureTracker.SetLastMovePosition(position);

            if (OnWidgetPointerMoved(position, !isHovering, GetShiftPressed()))
                return;

            if (isHovering)
                return;

            _manipulationTracker.Manipulate([position], Map.Navigator.Manipulate);
        });
    }

    private static bool IsMouseButtonPressed(MouseEventArgs e) => e.Buttons == 1;

    protected void OnMouseUp(MouseEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            var location = e.ToLocation(_clientRect);

            _tapGestureTracker.IfTap(MaxTapGestureMovement * PixelDensity, (p, c) =>
            {
                if (OnWidgetTapped(p, c, GetShiftPressed()))
                    return;
                OnInfo(CreateMapInfoEventArgs(p, p, 1));

            });

            _manipulationTracker.Manipulate([e.ToLocation(_clientRect)], Map.Navigator.Manipulate);
            RefreshData();
        });
    }

    private double GetPixelDensity()
    {
        return _pixelDensityFromInterop;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            CommonDispose(true);
        }
    }

    private double ViewportWidth => _canvasSize?.Width ?? 0;
    private double ViewportHeight => _canvasSize?.Height ?? 0;

    public string? Cursor { get; set; }

    public void OpenInBrowser(string url)
    {
        Catch.TaskRun(() =>
        {
            if (JsRuntime != null)
                _ = JsRuntime.InvokeAsync<object>("open", [url, "_blank"]);
        });
    }

    private bool GetShiftPressed()
    {
        return _pressedKeys.Contains("ShiftLeft") || _pressedKeys.Contains("ShiftRight") || _pressedKeys.Contains("Shift");
    }

    public void OnTouchStart(TouchEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            // The client rect needs updating for scrolling. I would rather do that on the onscroll event but it does not fire on this element.
            _ = UpdateBoundingRectAsync();

            var locations = e.TargetTouches.ToTouchLocations(_clientRect);
            if (OnWidgetPointerPressed(locations[0], GetShiftPressed()))
                return;
            _tapGestureTracker.SetDownPosition(locations[0]);
            _manipulationTracker.Restart(locations);
        });
    }

    public void OnTouchMove(TouchEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            var locations = e.TargetTouches.ToTouchLocations(_clientRect);
            _tapGestureTracker.SetLastMovePosition(locations[0]);
            if (OnWidgetPointerMoved(locations[0], true, GetShiftPressed()))
                return;
            _manipulationTracker.Manipulate(locations.ToArray(), Map.Navigator.Manipulate);
        });
    }

    public void OnTouchEnd(TouchEventArgs _)
    {
        Catch.Exceptions(() =>
        {
            _tapGestureTracker.IfTap(MaxTapGestureMovement * PixelDensity, (p, c) =>
            {
                if (OnWidgetTapped(p, c, GetShiftPressed()))
                    return;
                OnInfo(CreateMapInfoEventArgs(p, p, c));
            });

            RefreshData();
        });
    }
}
