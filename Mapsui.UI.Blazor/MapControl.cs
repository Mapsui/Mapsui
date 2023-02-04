using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using SkiaSharp;
using Microsoft.JSInterop;
using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.UI.Blazor.Extensions;
using Microsoft.AspNetCore.Components.Web;
using SkiaSharp.Views.Blazor;
using Mapsui.Utilities;

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
    private string? _defaultCursor = Cursors.Default;
    private readonly HashSet<string> _pressedKeys = new();
    private bool _isInBoxZoomMode;
    public string MoveCursor { get; set; } = Cursors.Move;
    public int MoveButton { get; set; } = MouseButtons.Primary;
    public int MoveModifier { get; set; } = Keys.None;
    public int ZoomButton { get; set; } = MouseButtons.Primary;
    public int ZoomModifier { get; set; } = Keys.Control;
    public MouseWheelAnimation MouseWheelAnimation { get; } = new();
    protected readonly string _elementId = Guid.NewGuid().ToString("N");
    private MapsuiJsInterop? _interop;

    public string ElementId => _elementId;

    protected MapsuiJsInterop? Interop
    {
        get
        {
            if (_interop == null && JsRuntime != null)
            {
                _interop ??= new MapsuiJsInterop(JsRuntime);
            }

            return _interop;
        }
    }

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

    private void OnLoadComplete()
    {
        SetViewportSize();
    }

    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
    protected async void OnMouseWheel(WheelEventArgs e)
    {
        try
        {
            if (Map?.ZoomLock ?? true) return;
            if (!Viewport.HasSize()) return;

            var delta = e.DeltaY;
            var resolution = MouseWheelAnimation.GetResolution((int)delta, _viewport, Map);

            // Limit target resolution before animation to avoid an animation that is stuck on the max resolution, which would cause a needless delay
            resolution = Map.Limiter.LimitResolution(resolution, Viewport.Width, Viewport.Height, Map.Resolutions,
                Map.Extent);
            Navigator?.ZoomTo(resolution, e.Location(await BoundingClientRectAsync()).ToMapsui(), MouseWheelAnimation.Duration,
                MouseWheelAnimation.Easing);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
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

    private void OnSizeChanged()
    {
        SetViewportSize();
    }

    private protected void RunOnUIThread(Action action)
    {
        // Only one thread is active in WebAssembly.
        action();
    }

    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
    protected async void OnMouseDown(MouseEventArgs e)
    {
        try
        {
            IsInBoxZoomMode = e.Button == ZoomButton && (ZoomModifier == Keys.None || ModifierPressed(ZoomModifier));

            bool moveMode = e.Button == MoveButton && (MoveModifier == Keys.None || ModifierPressed(MoveModifier));

            if (moveMode)
                _defaultCursor = Cursor;

            if (moveMode || IsInBoxZoomMode)
                _downMousePosition = e.Location(await BoundingClientRectAsync());
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

    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
    protected async void OnMouseUp(MouseEventArgs e)
    {
        try
        {
            if (IsInBoxZoomMode)
            {
                if (_selectRectangle != null)
                {
                    var previous = Viewport.ScreenToWorld(_selectRectangle.TopLeft.X, _selectRectangle.TopLeft.Y);
                    var current = Viewport.ScreenToWorld(_selectRectangle.BottomRight.X,
                        _selectRectangle.BottomRight.Y);
                    ZoomToBox(previous, current);
                }
            }
            else if (_downMousePosition != null)
            {
                if (IsClick(e.Location(await BoundingClientRectAsync()), _downMousePosition))
                    OnInfo(InvokeInfo(e.Location(await BoundingClientRectAsync()).ToMapsui(), _downMousePosition.ToMapsui(), 1));
            }

            _downMousePosition = null;

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

    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
    protected async void OnMouseMove(MouseEventArgs e)
    {
        try
        {
            if (_downMousePosition != null)
            {
                if (IsInBoxZoomMode)
                {
                    var x = e.Location(await BoundingClientRectAsync());
                    var y = _downMousePosition;
                    _selectRectangle = new MRect(Math.Min(x.X, y.X), Math.Min(x.Y, y.Y), Math.Max(x.X, y.X),
                        Math.Max(x.Y, y.Y));
                    if (_invalidate != null)
                        _invalidate();
                }
                else // drag/pan - mode
                {
                    Cursor = MoveCursor;

                    _viewport.Transform(e.Location(await BoundingClientRectAsync()).ToMapsui(), _downMousePosition.ToMapsui());

                    RefreshGraphics();

                    _downMousePosition = e.Location(await BoundingClientRectAsync());
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }
    }

    public void ZoomToBox(MPoint beginPoint, MPoint endPoint)
    {
        var width = Math.Abs(endPoint.X - beginPoint.X);
        var height = Math.Abs(endPoint.Y - beginPoint.Y);
        if (width <= 0) return;
        if (height <= 0) return;

        ZoomHelper.ZoomToBoudingbox(beginPoint.X, beginPoint.Y, endPoint.X, endPoint.Y,
            ViewportWidth, ViewportHeight, out var x, out var y, out var resolution);

        Navigator?.NavigateTo(new MPoint(x, y), resolution, 384);

        RefreshData();
        RefreshGraphics();
        ClearBBoxDrawing();
    }

    private void ClearBBoxDrawing()
    {
        RunOnUIThread(() => IsInBoxZoomMode = false);
    }

    private protected float GetPixelDensity()
    {
        return 1;
        // TODO: Ask for the Real Pixel size.
        // var center = PointToScreen(Location + Size / 2);
        // return Screen.FromPoint(center).LogicalPixelSize;
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
}
