using Mapsui.Extensions;
using Mapsui.Manipulations;
using Mapsui.UI.Wpf.Extensions;
using Mapsui.Utilities;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MapsuiManipulation = Mapsui.Manipulations.Manipulation;

namespace Mapsui.UI.Wpf;

public partial class MapControl : Grid, IMapControl, IDisposable
{
    private readonly FlingTracker _flingTracker = new();
    private readonly TapGestureTracker _tapGestureTracker = new();
    private readonly ManipulationTracker _manipulationTracker = new();

    public MapControl()
    {
        SharedConstructor();

        _invalidate = () =>
        {
            if (Dispatcher.CheckAccess()) InvalidateCanvas();
            else RunOnUIThread(InvalidateCanvas);
        };

        Children.Add(SkiaCanvas);

        SkiaCanvas.PaintSurface += SKElementOnPaintSurface;
        Loaded += MapControlLoaded;
        SizeChanged += MapControlSizeChanged;

        MouseLeftButtonDown += MapControlMouseLeftButtonDown;
        MouseLeftButtonUp += MapControlMouseLeftButtonUp;

        MouseMove += MapControlMouseMove;
        MouseLeave += MapControlMouseLeave;
        MouseWheel += MapControlMouseWheel;

        ManipulationInertiaStarting += OnManipulationInertiaStarting;
        ManipulationDelta += OnManipulationDelta;
        ManipulationCompleted += OnManipulationCompleted;

        TouchDown += MapControl_TouchDown;
        TouchUp += MapControlTouchUp;

        IsManipulationEnabled = true;

        SkiaCanvas.Visibility = Visibility.Visible;
        RefreshGraphics();
    }

    private static Rectangle CreateSelectRectangle() => new()
    {
        Fill = new SolidColorBrush(Colors.Red),
        Stroke = new SolidColorBrush(Colors.Black),
        StrokeThickness = 3,
        RadiusX = 0.5,
        RadiusY = 0.5,
        StrokeDashArray = [3.0],
        Opacity = 0.3,
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalAlignment = HorizontalAlignment.Left,
        Visibility = Visibility.Collapsed
    };

    private SKElement SkiaCanvas { get; } = CreateSkiaRenderElement();

    private static SKElement CreateSkiaRenderElement() => new()
    {
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    internal void InvalidateCanvas()
    {
        SkiaCanvas.InvalidateVisual();
    }

    private void MapControlLoaded(object sender, RoutedEventArgs e)
    {
        SetViewportSize();
        Focusable = true;
    }

    private void MapControlMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var mouseWheelDelta = e.Delta;
        var mousePosition = e.GetPosition(this).ToMapsui();
        Map.Navigator.MouseWheelZoom(mouseWheelDelta, mousePosition);
    }

    private void MapControlSizeChanged(object sender, SizeChangedEventArgs e)
    {
        Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };
        SetViewportSize();
    }

    private void MapControlMouseLeave(object sender, MouseEventArgs e)
    {
        ReleaseMouseCapture();
    }

    private void RunOnUIThread(Action action)
    {
        if (!Dispatcher.CheckAccess())
            Dispatcher.BeginInvoke(action);
        else
            action();
    }

    private void MapControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var mousePosition = e.GetPosition(this).ToMapsui();
        _tapGestureTracker.Restart(mousePosition);
        _manipulationTracker.Restart([mousePosition]);
        if (OnWidgetPointerPressed(mousePosition, GetShiftPressed()))
            return;
        _flingTracker.Restart();
        CaptureMouse();
    }

    private void MapControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var position = e.GetPosition(this).ToMapsui();

        _tapGestureTracker.IfTap(position, MaxTapGestureMovement * PixelDensity, (p, c) =>
        {
            if (OnWidgetTapped(p, c, GetShiftPressed()))
                return;
            OnInfo(CreateMapInfoEventArgs(p, p, 1));
        });

        Refresh();

        _flingTracker.IfFling(1, (vX, vY) => Map.Navigator.Fling(vX, vY, 1000));
        _flingTracker.RemoveId(1);

        ReleaseMouseCapture();
    }

    private void MapControl_TouchDown(object? sender, TouchEventArgs e)
    {
        var touchDownPosition = e.GetTouchPoint(this).Position.ToMapsui();
        _tapGestureTracker.Restart(touchDownPosition);
    }

    private void MapControlTouchUp(object? sender, TouchEventArgs e)
    {
        var touchUpPosition = e.GetTouchPoint(this).Position.ToMapsui();
        _tapGestureTracker.IfTap(touchUpPosition, MaxTapGestureMovement * PixelDensity, (p, c) =>
        {
            if (OnWidgetTapped(p, c, GetShiftPressed()))
                return;
            OnInfo(CreateMapInfoEventArgs(p, p, 1));
        });
    }

    public void OpenInBrowser(string url)
    {
        Catch.TaskRun(() =>
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = url,
                // The default for this has changed in .net core, you have to explicitly set if to true for it to work.
                UseShellExecute = true
            });
        });
    }

    private void MapControlMouseMove(object sender, MouseEventArgs e)
    {
        var isHovering = IsHovering(e);
        var position = e.GetPosition(this).ToMapsui();
        if (OnWidgetPointerMoved(position, !isHovering, GetShiftPressed()))
            return;
        if (isHovering)
            return;
        _flingTracker.AddEvent(1, position, DateTime.Now.Ticks);
        _manipulationTracker.Manipulate([position], Map.Navigator.Manipulate);
    }

    private double ViewportWidth => ActualWidth;
    private double ViewportHeight => ActualHeight;

    private static void OnManipulationInertiaStarting(object? sender, ManipulationInertiaStartingEventArgs e)
    {
        e.TranslationBehavior.DesiredDeceleration = 25 * 96.0 / (1000.0 * 1000.0);
    }

    private void OnManipulationDelta(object? sender, ManipulationDeltaEventArgs e)
    {
        Map.Navigator.Manipulate(ToManipulation(e));
    }

    private static MapsuiManipulation ToManipulation(ManipulationDeltaEventArgs e)
    {
        var translation = e.DeltaManipulation.Translation;

        var previousCenter = e.ManipulationOrigin.ToMapsui();
        var center = previousCenter.Offset(translation.X, translation.Y);
        var scaleFactor = GetScaleFactor(e.DeltaManipulation.Scale);
        var rotationChange = e.DeltaManipulation.Rotation;

        return new MapsuiManipulation(center, previousCenter, scaleFactor, rotationChange, e.CumulativeManipulation.Rotation);
    }

    private static double GetScaleFactor(Vector scale)
    {
        var deltaScale = (scale.X + scale.Y) / 2;
        if (Math.Abs(deltaScale) < Constants.Epsilon)
            return 1; // If there is no scaling the deltaScale will be 0.0 in Windows Phone (while it is 1.0 in wpf)
        if (!(Math.Abs(deltaScale - 1d) > Constants.Epsilon)) return 1;
        return deltaScale;
    }

    private void OnManipulationCompleted(object? sender, ManipulationCompletedEventArgs e) => Refresh();

    private void SKElementOnPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
    {
        if (PixelDensity <= 0)
            return;
        var canvas = args.Surface.Canvas;
        canvas.Scale(PixelDensity, PixelDensity);
        CommonDrawControl(canvas);
    }

    private double GetPixelDensity()
    {
        var presentationSource = PresentationSource.FromVisual(this)
            ?? throw new Exception("PresentationSource is null");
        var compositionTarget = presentationSource.CompositionTarget
            ?? throw new Exception("CompositionTarget is null");
        var matrix = compositionTarget.TransformToDevice;

        var dpiX = matrix.M11;
        var dpiY = matrix.M22;

        if (dpiX != dpiY) throw new ArgumentException();

        return dpiX;
    }

    protected virtual void Dispose(bool disposing)
    {
        CommonDispose(disposing);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private static bool GetShiftPressed()
    {
        return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
    }

    private static bool IsHovering(MouseEventArgs e)
    {
        return e.LeftButton != MouseButtonState.Pressed;
    }
}
