using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Rendering.Skia;
using Mapsui.UI.Blazor.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SkiaSharp;
using SkiaSharp.Views.Blazor;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.UI.Blazor;

public partial class MapControl : ComponentBase, IMapControl
{
    public static bool UseGPU { get; set; } = false;

    protected SKCanvasView? _viewCpu;
    protected SKGLView? _viewGpu;

    [Inject]
    private IJSRuntime? JsRuntime { get; set; }

    private SKImageInfo? _canvasSize;
    private bool _onLoaded;
    private MRect? _selectRectangle;
    private MPoint? _downMousePosition;
    private MPoint? _previousMousePosition;
    private string? _defaultCursor = Cursors.Default;
    private readonly HashSet<string> _pressedKeys = new();
    private bool _isInBoxZoomMode;
    private TouchState? _previousTouchState;
    double _pixelDensityFromInterop = 1;
    BoundingClientRect _clientRect = new BoundingClientRect();
    protected readonly string _elementId = Guid.NewGuid().ToString("N");
    private MapsuiJsInterop? _interop;

    public string MoveCursor { get; set; } = Cursors.Move;
    public int MoveButton { get; set; } = MouseButtons.Primary;
    public int MoveModifier { get; set; } = Keys.None;
    public int ZoomButton { get; set; } = MouseButtons.Primary;
    public int ZoomModifier { get; set; } = Keys.Control;
    public string ElementId => _elementId;
    protected MapsuiJsInterop? Interop =>
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

        Renderer = new MapRenderer();
        RefreshGraphics();
    }

    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
    private async void OnLoadComplete()
    {
        try
        {
            SetViewportSize();
            await InitializingInteropAsync();
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }
    }

    protected void OnMouseWheel(WheelEventArgs e)
    {

        var mouseWheelDelta = (int)e.DeltaY * -1; // so that it zooms like on windows
        var currentMousePosition = e.ToLocation(_clientRect);
        Map.Navigator.MouseWheelZoom(mouseWheelDelta, currentMousePosition);
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

    private protected void RunOnUIThread(Action action)
    {
        // Only one thread is active in WebAssembly.
        action();
    }

    protected void OnDblClick(MouseEventArgs e)
    {
        try
        {
            if (HandleTouching(e.ToLocation(_clientRect), e.Button == 0, 2, ShiftPressed))
                return;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }
    }

    protected void OnPointerDown(PointerEventArgs e)
    {
        try
        {
            // The client rect needs updating for scrolling. I would rather do that on the onscroll event but it does not fire on this element.
            _ = UpdateBoundingRectAsync();

            if (HandleTouching(e.ToLocation(_clientRect), e.Button == 0, 1, ShiftPressed))
                return;

            IsInBoxZoomMode = e.Button == ZoomButton && (ZoomModifier == Keys.None || ModifierPressed(ZoomModifier));

            bool moveMode = e.Button == MoveButton && (MoveModifier == Keys.None || ModifierPressed(MoveModifier));

            if (moveMode)
                _defaultCursor = Cursor;

            if (moveMode || IsInBoxZoomMode)
                _previousMousePosition = e.ToLocation(_clientRect);

            _downMousePosition = e.ToLocation(_clientRect);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }
    }

    private bool ModifierPressed(int modifier)
    {
        switch (modifier)
        {
            case Keys.Alt:
                return _pressedKeys.Contains("Alt");
            case Keys.Control:
                return _pressedKeys.Contains("Control");
            case Keys.ShiftLeft:
                return _pressedKeys.Contains("ShiftLeft") || _pressedKeys.Contains("ShiftRight") || _pressedKeys.Contains("Shift");
        }

        return false;
    }

    private bool IsInBoxZoomMode
    {
        get => _isInBoxZoomMode;
        set
        {
            _selectRectangle = null;
            _isInBoxZoomMode = value;
        }
    }

    protected void OnPointerUp(PointerEventArgs e)
    {
        try
        {
            if (HandleTouched(e.ToLocation(_clientRect), e.Button == 0, 1, ShiftPressed))
                return;

            if (IsInBoxZoomMode)
            {
                if (_selectRectangle != null)
                {
                    var previous = Map.Navigator.Viewport.ScreenToWorld(_selectRectangle.TopLeft.X, _selectRectangle.TopLeft.Y);
                    var current = Map.Navigator.Viewport.ScreenToWorld(_selectRectangle.BottomRight.X,
                        _selectRectangle.BottomRight.Y);
                    ZoomToBox(previous, current);
                }
            }
            else if (_downMousePosition != null)
            {
                var location = e.ToLocation(_clientRect);
                if (IsClick(location, _downMousePosition))
                    OnInfo(CreateMapInfoEventArgs(location, _downMousePosition, 1));
            }

            _downMousePosition = null;
            _previousMousePosition = null;

            Cursor = _defaultCursor;

            RefreshData();
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }
    }

    private static bool IsClick(MPoint currentPosition, MPoint previousPosition)
    {
        return Math.Abs(currentPosition.Distance(previousPosition)) < 5;
    }

    // To trigger the Info event on mobile I changed the MouseDown and MouseUp
    // into PointerDown and PointerUp. When I also changed OnMouseDown to OnPointerDown
    // the single finger drag gesture causes double speed panning. To fix this I kept
    // the OnMouseMove event as it was, but conceptually it is a bit confusing. 
    // I tested the logic on a real device and it works correctly, for pinch, pan,
    // and Info events, so I am leaving it like this.
    // An alternative would be keep mouse and touch events completely separate, and
    // don't use Pointer, but that would involve some rewriting of the touch logic to
    // support the Info event.
    protected void OnMouseMove(MouseEventArgs e)
    {
        try
        {
            if (HandleMoving(e.ToLocation(_clientRect), e.Button == 0, 0, ShiftPressed))
                return;

            if (_previousMousePosition != null)
            {
                if (IsInBoxZoomMode)
                {
                    var x = e.ToLocation(_clientRect);
                    if (_downMousePosition != null)
                    {
                        var y = _downMousePosition;
                        _selectRectangle = new MRect(Math.Min(x.X, y.X), Math.Min(x.Y, y.Y), Math.Max(x.X, y.X),
                            Math.Max(x.Y, y.Y));
                        if (_invalidate != null)
                            _invalidate();
                    }
                }
                else // drag/pan - mode
                {
                    Cursor = MoveCursor;

                    var currentPosition = e.ToLocation(_clientRect);
                    Map.Navigator.Drag(currentPosition, _previousMousePosition);
                    _previousMousePosition = e.ToLocation(_clientRect);
                }

                // cleanout down mouse position because it is now a move
                _downMousePosition = null;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }
    }

    public void ZoomToBox(MPoint beginPoint, MPoint endPoint)
    {
        var box = new MRect(beginPoint.X, beginPoint.Y, endPoint.X, endPoint.Y);
        Map.Navigator.ZoomToBox(box, duration: 300); ;
        ClearBBoxDrawing();
    }

    private void ClearBBoxDrawing()
    {
        RunOnUIThread(() => IsInBoxZoomMode = false);
    }

    private protected float GetPixelDensity()
    {
        return (float)_pixelDensityFromInterop;
    }

    public virtual void Dispose()
    {
        CommonDispose(true);
    }

    public float ViewportWidth => _canvasSize?.Width ?? 0;
    public float ViewportHeight => _canvasSize?.Height ?? 0;

    // TODO: Implement Setting of Mouse
    public string? Cursor { get; set; }
    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
    public async void OpenBrowser(string url)
    {
        try
        {
            if (JsRuntime != null)
                await JsRuntime.InvokeAsync<object>("open", new object?[] { url, "_blank" });
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }

    }

    public bool ShiftPressed => _pressedKeys.Contains("ShiftLeft") || _pressedKeys.Contains("ShiftRight") || _pressedKeys.Contains("Shift");

    public void OnTouchStart(TouchEventArgs e)
    {
        _previousTouchState = TouchState.FromLocations(e.TargetTouches.ToLocations(_clientRect));
    }

    public void OnTouchMove(TouchEventArgs e)
    {
        var touchState = TouchState.FromLocations(e.TargetTouches.ToLocations(_clientRect));

        if (_previousTouchState is { }) // Should not happen but we do not control the events of the framework so just checking.
        {
            if (touchState.Mode == TouchMode.Zooming && _previousTouchState.Mode == TouchMode.Zooming)
                Map.Navigator.Pinch(touchState.Center, _previousTouchState.Center, touchState.Radius / _previousTouchState.Radius, 0);
            else if (touchState.Mode == TouchMode.Dragging && _previousTouchState.Mode != TouchMode.None)
                Map.Navigator.Drag(touchState.Center, _previousTouchState.Center);
        }
        _previousTouchState = touchState;
   }

    public void OnTouchEnd(TouchEventArgs e)
    {
        _previousTouchState = TouchState.FromLocations(e.TargetTouches.ToLocations(_clientRect));
        RefreshData();
    }
}
