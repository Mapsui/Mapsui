// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Android.Views;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using NativePaintEventArgs = SkiaSharp.Views.Android.SKPaintGLSurfaceEventArgs;
using View = Microsoft.Maui.Controls.View;
using Window = Microsoft.Maui.Controls.Window;

namespace Mapsui.UI.Maui.Drawing;

internal sealed class AndroidSurfaceMapDrawingView : MapDrawingView
{
    private int _canvasWidth;
    private int _canvasHeight;
    private SurfaceMapView SurfaceView => (SurfaceMapView)View;

    public AndroidSurfaceMapDrawingView() : base(new SurfaceMapView())
    {
        SurfaceView.Paint = PaintSurface;
        SurfaceView.Touch = args => OnTouch(this, args);
    }

    public override SKSize CanvasSize => new(_canvasWidth, _canvasHeight);

    public override void Connect(IMauiContext context)
    {
        if (SurfaceView.Handler is SurfaceMapViewHandler { Connected: true } handler && handler.MauiContext == context)
            return;

        var newHandler = new SurfaceMapViewHandler();
        newHandler.SetMauiContext(context);
        SurfaceView.Handler = newHandler;
    }

    public override void Invalidate() => (SurfaceView.Handler as SurfaceMapViewHandler)?.RequestRender();

    private void PaintSurface(NativePaintEventArgs args, object? context)
    {
        _canvasWidth = args.Info.Width;
        _canvasHeight = args.Info.Height;
        OnPaint(args.Surface.Canvas, context);
    }

    protected override void Detach()
    {
        SurfaceView.Paint = null;
        SurfaceView.Touch = null;
        SurfaceView.DetachWindow();
    }
}

internal sealed class SurfaceMapView : View
{
    private Window? _window;
    public Action<NativePaintEventArgs, object?>? Paint { get; set; }
    public Action<SKTouchEventArgs>? Touch { get; set; }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(Window))
        {
            DetachWindow();
            _window = Window;
            if (_window != null)
            {
                _window.Stopped += OnStopped;
                _window.Resumed += OnResumed;
            }
        }
    }

    private void OnStopped(object? sender, EventArgs args) => (Handler as SurfaceMapViewHandler)?.SetSuspended(true);
    private void OnResumed(object? sender, EventArgs args) => (Handler as SurfaceMapViewHandler)?.SetSuspended(false);

    public void DetachWindow()
    {
        if (_window == null)
            return;
        _window.Stopped -= OnStopped;
        _window.Resumed -= OnResumed;
        _window = null;
    }
}

internal sealed class SurfaceMapViewHandler : ViewHandler<SurfaceMapView, AndroidMapSurfaceView>
{
    public bool Connected { get; private set; }
    public SurfaceMapViewHandler() : base(ViewMapper) { }
    protected override AndroidMapSurfaceView CreatePlatformView() => new(Context);

    protected override void ConnectHandler(AndroidMapSurfaceView platformView)
    {
        base.ConnectHandler(platformView);
        Connected = true;
        platformView.PaintSurface += OnPaintSurface;
        platformView.Touch += OnTouch;
        platformView.GenericMotion += OnGenericMotion;
    }

    protected override void DisconnectHandler(AndroidMapSurfaceView platformView)
    {
        Connected = false;
        platformView.PaintSurface -= OnPaintSurface;
        platformView.Touch -= OnTouch;
        platformView.GenericMotion -= OnGenericMotion;
        platformView.Stop();
        base.DisconnectHandler(platformView);
#pragma warning disable IDISP007 // This handler creates and owns the native view in CreatePlatformView.
        platformView.Dispose();
#pragma warning restore IDISP007
    }

    public void RequestRender()
    {
        if (Connected)
            PlatformView.RequestRender();
    }

    public void SetSuspended(bool suspended)
    {
        if (Connected)
            PlatformView.SetSuspended(suspended);
    }

    private void OnPaintSurface(object? sender, NativePaintEventArgs args)
    {
        if (Connected)
            VirtualView?.Paint?.Invoke(args, PlatformView.GRContext);
    }

    private void OnTouch(object? sender, global::Android.Views.View.TouchEventArgs args) => args.Handled = DispatchMotion(args.Event);
    private void OnGenericMotion(object? sender, global::Android.Views.View.GenericMotionEventArgs args) => args.Handled = DispatchMotion(args.Event);

    private bool DispatchMotion(MotionEvent? motion)
    {
        if (!Connected || VirtualView?.Touch is not { } touch || motion is null)
            return false;

        var action = motion.ActionMasked switch
        {
            MotionEventActions.Down or MotionEventActions.PointerDown => SKTouchAction.Pressed,
            MotionEventActions.Move or MotionEventActions.HoverMove => SKTouchAction.Moved,
            MotionEventActions.Up or MotionEventActions.PointerUp => SKTouchAction.Released,
            MotionEventActions.Cancel => SKTouchAction.Cancelled,
            MotionEventActions.HoverEnter => SKTouchAction.Entered,
            MotionEventActions.HoverExit => SKTouchAction.Exited,
            MotionEventActions.Scroll => SKTouchAction.WheelChanged,
            _ => (SKTouchAction?)null
        };
        if (action is null)
            return false;

        var handled = false;
        var first = motion.ActionMasked == MotionEventActions.Move ? 0 : motion.ActionIndex;
        var end = motion.ActionMasked == MotionEventActions.Move ? motion.PointerCount : first + 1;
        for (var pointer = first; pointer < end; pointer++)
        {
            var tool = motion.GetToolType(pointer);
            var device = tool switch
            {
                MotionEventToolType.Mouse => SKTouchDeviceType.Mouse,
                MotionEventToolType.Stylus or MotionEventToolType.Eraser => SKTouchDeviceType.Pen,
                _ => SKTouchDeviceType.Touch
            };
            var secondary = (motion.ButtonState & MotionEventButtonState.Secondary) != 0
                || (OperatingSystem.IsAndroidVersionAtLeast(23) && (motion.ButtonState & MotionEventButtonState.StylusSecondary) != 0);
            var button = tool == MotionEventToolType.Eraser ? SKMouseButton.Middle : secondary
                    ? SKMouseButton.Right : SKMouseButton.Left;
            var inContact = motion.ActionMasked is MotionEventActions.Down or MotionEventActions.PointerDown or MotionEventActions.Move;
            var wheelDelta = action == SKTouchAction.WheelChanged ? (int)(motion.GetAxisValue(Axis.Vscroll) * 120) : 0;
            // Keep native pixels and pointer IDs intact. MapControl applies density once for every backend.
            var args = new SKTouchEventArgs(motion.GetPointerId(pointer), action.Value, button, device,
                new SKPoint(motion.GetX(pointer), motion.GetY(pointer)), inContact, wheelDelta, motion.GetPressure(pointer));
            touch(args);
            handled |= args.Handled;
        }
        return handled;
    }
}
