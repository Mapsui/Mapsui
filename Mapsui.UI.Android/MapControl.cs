using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Manipulations;
using Mapsui.UI.Android.Extensions;
using SkiaSharp.Views.Android;

namespace Mapsui.UI.Android;

public enum SkiaRenderMode
{
    Hardware,
    Software
}

public partial class MapControl : ViewGroup, IMapControl
{
    private View? _canvas;
    private Handler? _mainLooperHandler;
    private SkiaRenderMode _renderMode = SkiaRenderMode.Hardware;
    private readonly ManipulationTracker _manipulationTracker = new();

    public MapControl(Context context, IAttributeSet attrs) :
        base(context, attrs)
    {
        SharedConstructor();
        LocalConstructor();
    }

    public MapControl(Context context, IAttributeSet attrs, int defStyle) :
        base(context, attrs, defStyle)
    {
        SharedConstructor();
        LocalConstructor();
    }

    private void LocalConstructor()
    {
        _invalidate = () => RunOnUIThread(RefreshGraphicsWithTryCatch);

        SetBackgroundColor(Color.Transparent);
        _canvas?.Dispose();
        _canvas = RenderMode == SkiaRenderMode.Software ? StartSoftwareRenderMode() : StartHardwareRenderMode();
        _mainLooperHandler?.Dispose();
        _mainLooperHandler = new Handler(Looper.MainLooper!);

        // Pointer events
        Touch += MapControl_Touch;
    }

    private void CanvasOnPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
    {
        if (GetPixelDensity() is not float pixelDensity)
            return;

        var canvas = args.Surface.Canvas;

        canvas.Scale(pixelDensity, pixelDensity);

        CommonDrawControl(canvas);
    }

    public SkiaRenderMode RenderMode
    {
        get => _renderMode;
        set
        {
            if (_renderMode == value) return;

            _renderMode = value;
            if (_renderMode == SkiaRenderMode.Hardware)
            {
                StopSoftwareRenderMode(_canvas);
                _canvas?.Dispose();
                _canvas = StartHardwareRenderMode();
            }
            else
            {
                StopHardwareRenderMode(_canvas);
                _canvas?.Dispose();
                _canvas = StartSoftwareRenderMode();
            }
            RefreshGraphics();
            OnPropertyChanged();
        }
    }


    protected override void OnSizeChanged(int width, int height, int oldWidth, int oldHeight)
    {
        base.OnSizeChanged(width, height, oldWidth, oldHeight);
        if (GetPixelDensity() is float pixelDensity) // In android the width and height are in pixels. The MapControl needs the size in device independent units. That is why we need PixelDensity here
        {
            SharedOnSizeChanged(
                ToDeviceIndependentUnits(width, pixelDensity),
                ToDeviceIndependentUnits(height, pixelDensity));
        }
        Refresh();
    }

    private void RunOnUIThread(Action action)
    {
        if (SynchronizationContext.Current == null)
            _mainLooperHandler?.Post(action);
        else
            action();
    }

    private void CanvasOnPaintSurfaceGL(object? sender, SKPaintGLSurfaceEventArgs args)
    {
        if (GetPixelDensity() is not float pixelDensity)
            return;

        var canvas = args.Surface.Canvas;

        canvas.Scale(pixelDensity, pixelDensity);

        CommonDrawControl(canvas);
    }

    public void MapControl_Touch(object? sender, TouchEventArgs args)
    {
        if (args.Event is null)
            return;
        if (GetPixelDensity() is not float pixelDensity)
            return;

        var positions = GetScreenPositions(args.Event, this, pixelDensity);

        switch (args.Event.Action)
        {
            case MotionEventActions.Down:
                _manipulationTracker.Restart(positions);
                if (OnPointerPressed(positions))
                    return;
                break;
            case MotionEventActions.Move:
                if (OnPointerMoved(positions, false))
                    return;
                _manipulationTracker.Manipulate(positions, Map.Navigator.Manipulate);
                break;
            case MotionEventActions.Up:
                OnPointerReleased(positions);
                break;
        }
    }

    /// <summary>
    /// Gets the screen position in device independent units relative to the MapControl.
    /// </summary>
    /// <param name="motionEvent"></param>
    /// <param name="view"></param>
    /// <returns></returns>
    private static ReadOnlySpan<ScreenPosition> GetScreenPositions(MotionEvent motionEvent, View view, double pixelDensity)
    {
        var result = new ScreenPosition[motionEvent.PointerCount];
        for (var i = 0; i < motionEvent.PointerCount; i++)
            result[i] = new ScreenPosition(motionEvent.GetX(i) - view.Left, motionEvent.GetY(i) - view.Top)
                .ToDeviceIndependentUnits(pixelDensity);
        return result;
    }

    private void RefreshGraphicsWithTryCatch()
    {
        try
        {
            // Both Invalidate and _canvas.Invalidate are necessary in different scenarios.
            Invalidate();
            _canvas?.Invalidate();
        }
        catch (ObjectDisposedException e)
        {
            // See issue: https://github.com/Mapsui/Mapsui/issues/433
            // What seems to be happening. The Activity is Disposed. Apparently it's children get Disposed
            // explicitly by something in Xamarin. During this Dispose the MessageCenter, which is itself
            // not disposed gets another notification to call RefreshGraphics.
            Logger.Log(LogLevel.Warning, "This can happen when the parent Activity is disposing.", e);
        }
    }

    protected override void OnLayout(bool changed, int l, int t, int r, int b)
    {
        SetBounds(_canvas, l, t, r, b);
    }

    private void SetBounds(View? view, int l, int t, int r, int b)
    {
        if (view == null)
            return;

        view.Top = t;
        view.Bottom = b;
        view.Left = l;
        view.Right = r;

        if (GetPixelDensity() is float pixelDensity)  // In android the width and height are in pixels. The MapControl needs the size in device independent units. That is why we need PixelDensity here
        {
            SharedOnSizeChanged(
                ToDeviceIndependentUnits(view.Width, pixelDensity),
                ToDeviceIndependentUnits(view.Height, pixelDensity));
        }
        Refresh();
    }

    public void OpenInBrowser(string url)
    {
        Catch.TaskRun(() =>
        {
            var uri = global::Android.Net.Uri.Parse(url);
            using var intent = new Intent(Intent.ActionView);
            intent.SetData(uri);

            using var chooser = Intent.CreateChooser(intent, "Open with");
            Context?.StartActivity(chooser);
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _map?.Dispose();
            _mainLooperHandler?.Dispose();
            _canvas?.Dispose();
        }
        CommonDispose(disposing);

        base.Dispose(disposing);
    }

    /// <summary>
    /// In native Android touch positions are in pixels whereas the canvas needs
    /// to be drawn in device independent units (otherwise labels on raster tiles will be unreadable
    /// and symbols will be too small). This method converts pixels to device independent units.
    /// </summary>
    /// <returns>The pixels given as input translated to device independent units.</returns>
    private static double ToDeviceIndependentUnits(int pixelCoordinate, float pixelDensity) =>
        pixelCoordinate / pixelDensity;

    private SKCanvasView StartSoftwareRenderMode()
    {
        var canvas = new SKCanvasView(Context);
        canvas.PaintSurface += CanvasOnPaintSurface;
        AddView(canvas);
        return canvas;
    }

    private void StopSoftwareRenderMode(View? canvas)
    {
        if (canvas is SKCanvasView canvasView)
        {
            canvasView.PaintSurface -= CanvasOnPaintSurface;
            RemoveView(canvasView);
            // Let's not dispose. The Paint callback might still be busy.
        }
    }

    private SKGLSurfaceView StartHardwareRenderMode()
    {
        var canvas = new SKGLSurfaceView(Context);
        canvas.PaintSurface += CanvasOnPaintSurfaceGL;
        AddView(canvas);
        return canvas;
    }

    private void StopHardwareRenderMode(View? canvas)
    {
        if (canvas is SKGLSurfaceView surfaceView)
        {
            surfaceView.PaintSurface -= CanvasOnPaintSurfaceGL;
            RemoveView(surfaceView);
            // Let's not dispose. The Paint callback might still be busy.
        }
    }

    public float? GetPixelDensity()
    {
        return Resources?.DisplayMetrics?.Density;
    }

    private static bool GetShiftPressed() => false;
}
