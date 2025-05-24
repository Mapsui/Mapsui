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
    private float? _pixelDensityFromInterop;
    private BoundingClientRect _clientRect = new();
    private MapsuiJsInterop? _interop;
    private readonly ManipulationTracker _manipulationTracker = new();
    public ScreenPosition? _lastMovePosition; // Workaround for missing touch position on touch-up.

    [Inject]
    private IJSRuntime? JsRuntime { get; set; }
    public static bool UseGPU { get; set; } = true;
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
    }

    public void InvalidateCanvas()
    {
        if (!OperatingSystem.IsBrowser())
            throw new InvalidOperationException("Only browser is supported");

        if (_viewCpu != null)
            _viewCpu.Invalidate();
        else if (_viewGpu != null)
            _viewGpu?.Invalidate();
        else
            throw new InvalidOperationException("Both _viewCpu and _viewGpu are null");
    }

    /// <summary>
    /// This enables an alternative mouse wheel method where the step size on each mouse wheel event can be configured
    /// by setting the ContinuousMouseWheelZoomStepSize.
    /// </summary>
    public bool UseContinuousMouseWheelZoom { get; set; } = false;
    /// <summary>
    /// The size of the mouse wheel steps used when UseContinuousMouseWheelZoom = true. The default is 0.1. A step 
    /// size of 1 would doubling or halving the scale of the map on each event.    
    /// </summary>
    public double ContinuousMouseWheelZoomStepSize { get; set; } = 0.1;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        RefreshGraphics();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
            await InitializingInteropAsync();
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
            OnSizeChanged(info);
        }

        _renderController?.Render(canvas);
    }

    private void OnLoadComplete()
    {
        Catch.Exceptions(async () =>
        {
            SharedOnSizeChanged(_canvasSize?.Width ?? 0, _canvasSize?.Height ?? 0);
            await InitializingInteropAsync();
        });
    }

    protected void OnMouseWheel(WheelEventArgs e)
    {
        if (UseContinuousMouseWheelZoom)
        {
            var stepSize = ContinuousMouseWheelZoomStepSize;
            var scaleFactor = Math.Pow(2, e.DeltaY > 0 ? stepSize : -stepSize);
            Map.Navigator.MouseWheelZoomContinuous(scaleFactor, e.ToScreenPosition(_clientRect));
        }
        else
        {
            var mouseWheelDelta = (int)e.DeltaY * -1; // so that it zooms like on windows
            var mousePosition = e.ToScreenPosition(_clientRect);
            Map.Navigator.MouseWheelZoom(mouseWheelDelta, mousePosition);
        }
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

        try
        {
            await Interop.DisableMouseWheelAsync(_elementId);
            await Interop.DisableTouchAsync(_elementId);
            _pixelDensityFromInterop = (float)await Interop.GetPixelDensityAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred in InitializingInteropAsync: {ex}");
        }
    }

    private void OnSizeChanged(SKImageInfo skImageInfo)
    {
        SharedOnSizeChanged(skImageInfo.Width, skImageInfo.Height);
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

    protected void OnMouseDown(MouseEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            // The client rect needs updating for scrolling. I would rather do that on the onscroll event but it does not fire on this element.
            _ = UpdateBoundingRectAsync();
            var position = e.ToScreenPosition(_clientRect);

            _manipulationTracker.Restart([position]);

            if (OnPointerPressed([position]))
                return;
        });
    }

    protected void OnMouseMove(MouseEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            var isHovering = !IsMouseButtonPressed(e);
            var position = e.ToScreenPosition(_clientRect);

            if (OnPointerMoved([position], isHovering))
                return;

            if (!isHovering)
                _manipulationTracker.Manipulate([position], Map.Navigator.Manipulate);
        });
    }

    private static bool IsMouseButtonPressed(MouseEventArgs e) => e.Buttons == 1;

    protected void OnMouseUp(MouseEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            var position = e.ToScreenPosition(_clientRect);
            OnPointerReleased([position]);
        });
    }

    public float? GetPixelDensity()
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
            SharedDispose(true);
        }
    }

    public string? Cursor { get; set; }

    public void OpenInBrowser(string url)
    {
        Catch.TaskRun(() =>
        {
            if (JsRuntime != null)
                _ = JsRuntime.InvokeAsync<object>("open", [url, "_blank"]);
        });
    }

    public void OnTouchStart(TouchEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            // The client rect needs updating for scrolling. I would rather do that on the onscroll event but it does not fire on this element.
            _ = UpdateBoundingRectAsync();
            var positions = e.TargetTouches.ToScreenPositions(_clientRect);
            _manipulationTracker.Restart(positions);

            if (OnPointerPressed(positions))
                return;
        });
    }

    public void OnTouchMove(TouchEventArgs e)
    {
        Catch.Exceptions(() =>
        {
            var positions = e.TargetTouches.ToScreenPositions(_clientRect);
            if (positions.Length == 1)
                _lastMovePosition = positions[0]; // Workaround for missing touch-up location.

            if (OnPointerMoved(positions, false))
                return;


            _manipulationTracker.Manipulate(positions.ToArray(), Map.Navigator.Manipulate);
        });
    }

    public void OnTouchEnd(TouchEventArgs _)
    {
        Catch.Exceptions(() =>
        {
            if (_lastMovePosition is null)
                return;
            var position = _lastMovePosition.Value;
            OnPointerReleased([position]);
        });
    }

    private bool GetShiftPressed() => false; // Could not get keydown/up to work. Please try to get this to work if possible. 
}
