using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
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

internal class MapControlGestureListener : GestureDetector.SimpleOnGestureListener
{
    public EventHandler<GestureDetector.FlingEventArgs>? Fling;
#if NET7_0
    public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
#else
    public override bool OnFling(MotionEvent? e1, MotionEvent e2, float velocityX, float velocityY)
#endif
    {
        if (Fling != null)
        {
            Fling?.Invoke(this, new GestureDetector.FlingEventArgs(false, e1, e2, velocityX, velocityY));
            return true;
        }

        return base.OnFling(e1, e2, velocityX, velocityY);
    }
}

public partial class MapControl : ViewGroup, IMapControl
{
    private View? _canvas;
    private GestureDetector? _gestureDetector;
    private Handler? _mainLooperHandler;
    private SkiaRenderMode _renderMode = SkiaRenderMode.Hardware;
    private readonly ManipulationTracker _manipulationTracker = new();

    public MapControl(Context context, IAttributeSet attrs) :
        base(context, attrs)
    {
        CommonInitialize();
        Initialize();
    }

    public MapControl(Context context, IAttributeSet attrs, int defStyle) :
        base(context, attrs, defStyle)
    {
        CommonInitialize();
        Initialize();
    }

    private void Initialize()
    {
        _invalidate = () => { RunOnUIThread(RefreshGraphicsWithTryCatch); };

        SetBackgroundColor(Color.Transparent);
        _canvas?.Dispose();
        _canvas = RenderMode == SkiaRenderMode.Software ? StartSoftwareRenderMode() : StartHardwareRenderMode();
        _mainLooperHandler?.Dispose();
        _mainLooperHandler = new Handler(Looper.MainLooper!);

        SetViewportSize(); // todo: check if size is available, perhaps we need a load event

        // Pointer events
        Touch += MapControl_Touch;
        var listener = new MapControlGestureListener(); // Todo: Find out if/why we need this custom gesture detector. Why not the _gestureDetector?
        listener.Fling += OnFling;
        _gestureDetector?.Dispose();
        _gestureDetector = new GestureDetector(Context, listener);
        _gestureDetector.SingleTapConfirmed += OnSingleTapped;
        _gestureDetector.DoubleTap += OnDoubleTapped;
    }

    private void CanvasOnPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
    {
        if (PixelDensity <= 0)
            return;

        var canvas = args.Surface.Canvas;

        canvas.Scale(PixelDensity, PixelDensity);

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

    private void OnSingleTapped(object? sender, GestureDetector.SingleTapConfirmedEventArgs e)
    {
        if (e.Event == null)
            return;

        var position = GetScreenPosition(e.Event, this);
        if (OnWidgetTapped(position, 1, false))
            return;
        OnInfo(CreateMapInfoEventArgs(position, position, 1));
    }

    private void OnDoubleTapped(object? sender, GestureDetector.DoubleTapEventArgs e)
    {
        if (e.Event == null)
            return;

        var position = GetScreenPosition(e.Event, this);
        if (OnWidgetTapped(position, 2, false))
            return;
        OnInfo(CreateMapInfoEventArgs(position, position, 2));
    }

    protected override void OnSizeChanged(int width, int height, int oldWidth, int oldHeight)
    {
        base.OnSizeChanged(width, height, oldWidth, oldHeight);
        SetViewportSize();
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
        if (PixelDensity <= 0)
            return;

        var canvas = args.Surface.Canvas;

        canvas.Scale(PixelDensity, PixelDensity);

        CommonDrawControl(canvas);
    }

    public void OnFling(object? sender, GestureDetector.FlingEventArgs args)
    {
        Map.Navigator.Fling(args.VelocityX / 10, args.VelocityY / 10, 1000);
    }

    public void MapControl_Touch(object? sender, TouchEventArgs args)
    {
        if (args.Event is null)
            return;

        if (_gestureDetector?.OnTouchEvent(args.Event) == true)
            return;

        var locations = GetTouchLocations(args.Event, this, PixelDensity);

        switch (args.Event.Action)
        {
            case MotionEventActions.Down:
                _manipulationTracker.Restart(locations);
                if (OnWidgetPointerPressed(locations[0], false))
                    return;
                break;
            case MotionEventActions.Move:
                if (OnWidgetPointerMoved(locations[0], true, false))
                    return;
                _manipulationTracker.Manipulate(locations, Map.Navigator.Pinch);
                break;
            case MotionEventActions.Up:
                // Todo: Add HandleWidgetPointerUp
                _manipulationTracker.Manipulate(locations, Map.Navigator.Pinch);
                Refresh();
                break;
        }
    }

    /// <summary>
    /// Gets the screen position in device independent units relative to the MapControl.
    /// </summary>
    /// <param name="motionEvent"></param>
    /// <param name="view"></param>
    /// <returns></returns>
    private static ReadOnlySpan<MPoint> GetTouchLocations(MotionEvent motionEvent, View view, double pixelDensity)
    {
        var result = new MPoint[motionEvent.PointerCount];
        for (var i = 0; i < motionEvent.PointerCount; i++)
            result[i] = new MPoint(motionEvent.GetX(i) - view.Left, motionEvent.GetY(i) - view.Top)
                .ToDeviceIndependentUnits(pixelDensity);
        return result;
    }

    /// <summary>
    /// Gets the screen position in device independent units relative to the MapControl.
    /// </summary>
    /// <param name="motionEvent"></param>
    /// <param name="view"></param>
    /// <returns></returns>
    private MPoint GetScreenPosition(MotionEvent motionEvent, View view)
    {
        return GetScreenPositionInPixels(motionEvent, view).ToDeviceIndependentUnits(PixelDensity);
    }

    /// <summary>
    /// Gets the screen position in pixels relative to the MapControl.
    /// </summary>
    /// <param name="motionEvent"></param>
    /// <param name="view"></param>
    /// <returns></returns>
    private static MPoint GetScreenPositionInPixels(MotionEvent motionEvent, View view)
    {
        return new MPoint(motionEvent.GetX(0) - view.Left, motionEvent.GetY(0) - view.Top);
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

    private static void SetBounds(View? view, int l, int t, int r, int b)
    {
        if (view == null)
            return;

        view.Top = t;
        view.Bottom = b;
        view.Left = l;
        view.Right = r;
    }

    public void OpenInBrowser(string url)
    {
        var uri = global::Android.Net.Uri.Parse(url);
        var intent = new Intent(Intent.ActionView);
        intent.SetData(uri);

        var chooser = Intent.CreateChooser(intent, "Open with");
        Context?.StartActivity(chooser);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _map?.Dispose();
            _mainLooperHandler?.Dispose();
            _canvas?.Dispose();
            _gestureDetector?.Dispose();
        }
        CommonDispose(disposing);

        base.Dispose(disposing);
    }

    private double ViewportWidth => ToDeviceIndependentUnits(Width);
    private double ViewportHeight => ToDeviceIndependentUnits(Height);

    /// <summary>
    /// In native Android touch positions are in pixels whereas the canvas needs
    /// to be drawn in device independent units (otherwise labels on raster tiles will be unreadable
    /// and symbols will be too small). This method converts pixels to device independent units.
    /// </summary>
    /// <returns>The pixels given as input translated to device independent units.</returns>
    private double ToDeviceIndependentUnits(int pixelCoordinate)
    {
        return pixelCoordinate / PixelDensity;
    }

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

    private double GetPixelDensity()
    {
        return Resources?.DisplayMetrics?.Density ?? 0d;
    }
}
