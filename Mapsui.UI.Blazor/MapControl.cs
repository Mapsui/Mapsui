using Mapsui.Extensions;
using Mapsui.Manipulations;
using Mapsui.UI.Blazor.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SkiaSharp;
using SkiaSharp.Views.Blazor;
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.UI.Blazor;

public partial class MapControl : ComponentBase, IMapControl
{
    protected SKCanvasView? _viewCpu;
    protected SKGLView? _viewGpu;
    protected readonly string _elementId = Guid.NewGuid().ToString("N");

    private SKImageInfo? _canvasSize;
    private bool _onLoaded;
    private MRect? _selectRectangle;
    private readonly HashSet<string> _pressedKeys = [];
    private bool _isInBoxZoomMode;
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
    /// <summary>
    /// The movement allowed between a touch down and touch up in a touch gestures in device independent pixels.
    /// </summary>
    public int MaxTapGestureMovement { get; set; } = 8;
    private MapsuiJsInterop? Interop =>
            _interop == null && JsRuntime != null
                ? _interop ??= new MapsuiJsInterop(JsRuntime)
                : _interop;


    protected override void OnInitialized()
    {
        CommonInitialize();
        ControlInitialize();
        base.OnInitialized();
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

    protected void ControlInitialize()
    {
        _invalidate = () =>
        {
            if (_viewCpu != null)
                _viewCpu?.Invalidate();
            else
                _viewGpu?.Invalidate();
        };

        // Mapsui.Rendering.Skia use Mapsui.Nts where GetDbaseLanguageDriver need encoding providers
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        RefreshGraphics();
    }

    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
    private async void OnLoadComplete()
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

    protected void OnClick(MouseEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            var location = e.ToLocation(_clientRect);
            if (OnWidgetTapped(location, 1, GetShiftPressed()))
                return;
            OnInfo(CreateMapInfoEventArgs(location, location, 1));
        });
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
            _manipulationTracker.Restart([location]);

            if (OnWidgetPointerPressed(location, GetShiftPressed()))
                return;
        });
    }

    protected void OnMouseMove(MouseEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            var isHovering = !IsMouseButtonPressed(e);
            if (OnWidgetPointerMoved(e.ToLocation(_clientRect), !isHovering, 0, GetShiftPressed()))
                return;

            if (isHovering)
                return;

            _manipulationTracker.Manipulate([e.ToLocation(_clientRect)], Map.Navigator.Pinch);
        });
    }

    private static bool IsMouseButtonPressed(MouseEventArgs e) => e.Buttons == 1;

    protected void OnMouseUp(MouseEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            _manipulationTracker.Manipulate([e.ToLocation(_clientRect)], Map.Navigator.Pinch);

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
    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
    public async void OpenBrowser(string url)
    {
        Catch.Exceptions(async () =>
        {
            if (JsRuntime != null)
                await JsRuntime.InvokeAsync<object>("open", [url, "_blank"]);
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
            _tapGestureTracker.Start(locations[0]);
            _manipulationTracker.Restart(locations);
        });
    }

    public void OnTouchMove(TouchEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            var locations = e.TargetTouches.ToTouchLocations(_clientRect);
            if (OnWidgetPointerMoved(locations[0], true, 0, GetShiftPressed()))
                return;
            _tapGestureTracker.Move(locations[0]);
            _manipulationTracker.Manipulate(locations.ToArray(), Map.Navigator.Pinch);
        });
    }

    public void OnTouchEnd(TouchEventArgs _)
    {
        Catch.Exceptions(() =>
        {
            _tapGestureTracker.IfTap((position) =>
            {
                if (OnWidgetTapped(position, 1, GetShiftPressed()))
                    return;
                OnInfo(CreateMapInfoEventArgs(position, position, 1));
            }, MaxTapGestureMovement * PixelDensity);

            RefreshData();
        });
    }
}
