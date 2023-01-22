using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Mapsui.Logging;
using Mapsui.UI.Android.Extensions;
using Mapsui.Utilities;
using SkiaSharp.Views.Android;
using Math = System.Math;

#nullable enable

namespace Mapsui.UI.Android;

public enum SkiaRenderMode
{
    Hardware,
    Software
}

internal class MapControlGestureListener : GestureDetector.SimpleOnGestureListener
{
    public EventHandler<GestureDetector.FlingEventArgs>? Fling;

    public override bool OnFling(MotionEvent? e1, MotionEvent? e2, float velocityX, float velocityY)
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
    private double _virtualRotation;
    private GestureDetector? _gestureDetector;
    private double _previousAngle;
    private double _previousRadius = 1f;
    private TouchMode _mode = TouchMode.None;
    private Handler? _mainLooperHandler;
    /// <summary>
    /// Saver for center before last pinch movement
    /// </summary>
    private MPoint _previousTouch = new MPoint();
    private SkiaRenderMode _renderMode = SkiaRenderMode.Hardware;

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

        Touch += MapView_Touch;

        var listener = new MapControlGestureListener();

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

    private void OnDoubleTapped(object? sender, GestureDetector.DoubleTapEventArgs e)
    {
        if (e.Event == null)
            return;

        var position = GetScreenPosition(e.Event, this);
        OnInfo(InvokeInfo(position, position, 2));
    }

    private void OnSingleTapped(object? sender, GestureDetector.SingleTapConfirmedEventArgs e)
    {
        if (e.Event == null)
            return;

        var position = GetScreenPosition(e.Event, this);
        OnInfo(InvokeInfo(position, position, 1));
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
        Navigator?.FlingWith(args.VelocityX / 10, args.VelocityY / 10, 1000);
    }

    public void MapView_Touch(object? sender, TouchEventArgs args)
    {
        if (_gestureDetector?.OnTouchEvent(args.Event) ?? false)
            return;

        var touchPoints = GetScreenPositions(args.Event, this);

        switch (args.Event?.Action)
        {
            case MotionEventActions.Up:
                Refresh();
                _mode = TouchMode.None;
                break;
            case MotionEventActions.Down:
            case MotionEventActions.Pointer1Down:
            case MotionEventActions.Pointer2Down:
            case MotionEventActions.Pointer3Down:
                if (touchPoints.Count >= 2)
                {
                    (_previousTouch, _previousRadius, _previousAngle) = GetPinchValues(touchPoints);
                    _mode = TouchMode.Zooming;
                    _virtualRotation = Viewport.Rotation;
                }
                else
                {
                    _mode = TouchMode.Dragging;
                    _previousTouch = touchPoints.First();
                }
                break;
            case MotionEventActions.Pointer1Up:
            case MotionEventActions.Pointer2Up:
            case MotionEventActions.Pointer3Up:
                // Remove the touchPoint that was released from the locations to reset the
                // starting points of the move and rotation
                touchPoints.RemoveAt(args.Event.ActionIndex);

                if (touchPoints.Count >= 2)
                {
                    (_previousTouch, _previousRadius, _previousAngle) = GetPinchValues(touchPoints);
                    _mode = TouchMode.Zooming;
                    _virtualRotation = Viewport.Rotation;
                }
                else
                {
                    _mode = TouchMode.Dragging;
                    _previousTouch = touchPoints.First();
                }
                Refresh();
                break;
            case MotionEventActions.Move:
                switch (_mode)
                {
                    case TouchMode.Dragging:
                        {
                            if (touchPoints.Count != 1)
                                return;

                            var touch = touchPoints.First();
                            if (_previousTouch != null)
                            {
                                _viewport.Transform(touch, _previousTouch);
                                RefreshGraphics();
                            }
                            _previousTouch = touch;
                        }
                        break;
                    case TouchMode.Zooming:
                        {
                            if (touchPoints.Count < 2)
                                return;

                            var (previousTouch, previousRadius, previousAngle) = (_previousTouch, _previousRadius, _previousAngle);
                            var (touch, radius, angle) = GetPinchValues(touchPoints);

                            double rotationDelta = 0;

                            if (Map?.RotationLock == false)
                            {
                                _virtualRotation += angle - previousAngle;

                                rotationDelta = RotationCalculations.CalculateRotationDeltaWithSnapping(
                                    _virtualRotation, _viewport.Rotation, _unSnapRotationDegrees, _reSnapRotationDegrees);
                            }

                            _viewport.Transform(touch, previousTouch, radius / previousRadius, rotationDelta);
                            RefreshGraphics();

                            (_previousTouch, _previousRadius, _previousAngle) = (touch, radius, angle);


                        }
                        break;
                }
                break;
        }
    }

    /// <summary>
    /// Gets the screen position in device independent units relative to the MapControl.
    /// </summary>
    /// <param name="motionEvent"></param>
    /// <param name="view"></param>
    /// <returns></returns>
    private List<MPoint> GetScreenPositions(MotionEvent? motionEvent, View view)
    {
        if (motionEvent == null)
            return new List<MPoint>();

        var result = new List<MPoint>();
        for (var i = 0; i < motionEvent.PointerCount; i++)
        {
            var pixelCoordinate = new MPoint(motionEvent.GetX(i) - view.Left, motionEvent.GetY(i) - view.Top);
            result.Add(pixelCoordinate.ToDeviceIndependentUnits(PixelDensity));
        }
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
            // What seems to be happening. The Activity is Disposed. Appently it's children get Disposed
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

    public void OpenBrowser(string url)
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

    private static (MPoint centre, double radius, double angle) GetPinchValues(List<MPoint> locations)
    {
        if (locations.Count < 2)
            throw new ArgumentException();

        double centerX = 0;
        double centerY = 0;

        foreach (var location in locations)
        {
            centerX += location.X;
            centerY += location.Y;
        }

        centerX = centerX / locations.Count;
        centerY = centerY / locations.Count;

        var radius = Algorithms.Distance(centerX, centerY, locations[0].X, locations[0].Y);

        var angle = Math.Atan2(locations[1].Y - locations[0].Y, locations[1].X - locations[0].X) * 180.0 / Math.PI;

        return (new MPoint(centerX, centerY), radius, angle);
    }

    private float ViewportWidth => ToDeviceIndependentUnits(Width);
    private float ViewportHeight => ToDeviceIndependentUnits(Height);

    /// <summary>
    /// In native Android touch positions are in pixels whereas the canvas needs
    /// to be drawn in device independent units (otherwise labels on raster tiles will be unreadable
    /// and symbols will be too small). This method converts pixels to device independent units.
    /// </summary>
    /// <returns>The pixels given as input translated to device independent units.</returns>
    private float ToDeviceIndependentUnits(float pixelCoordinate)
    {
        return pixelCoordinate / PixelDensity;
    }

    private View StartSoftwareRenderMode()
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

    private View StartHardwareRenderMode()
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

    private float GetPixelDensity()
    {
        return Resources?.DisplayMetrics?.Density ?? 0;
    }
}
