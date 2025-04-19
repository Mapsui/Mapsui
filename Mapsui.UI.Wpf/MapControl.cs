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
    private readonly ManipulationTracker _manipulationTracker = new();

    public MapControl()
    {
        SharedConstructor();

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

    public void InvalidateCanvas()
    {
        if (Dispatcher.CheckAccess()) SkiaCanvas.InvalidateVisual();
        else RunOnUIThread(SkiaCanvas.InvalidateVisual);
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

    private void MapControlLoaded(object sender, RoutedEventArgs e)
    {
        Focusable = true;
    }

    private void MapControlMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var mouseWheelDelta = e.Delta;
        var mousePosition = e.GetPosition(this).ToScreenPosition();
        Map.Navigator.MouseWheelZoom(mouseWheelDelta, mousePosition);
    }

    private void MapControlSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Accessing ActualWidth and ActualHeight before size changed causes an exception, so we need to do it here.
        Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };
        SharedOnSizeChanged(ActualWidth, ActualHeight);
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
        var position = e.GetPosition(this).ToScreenPosition();
        _manipulationTracker.Restart([position]);

        if (OnPointerPressed([position]))
            return;

        CaptureMouse();
    }

    private void MapControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var position = e.GetPosition(this).ToScreenPosition();
        OnPointerReleased([position]);
        ReleaseMouseCapture();
    }

    private void MapControl_TouchDown(object? sender, TouchEventArgs e)
    {
        var position = e.GetTouchPoint(this).Position.ToScreenPosition();
        if (OnPointerPressed([position]))
            return;
    }

    private void MapControlTouchUp(object? sender, TouchEventArgs e)
    {
        var position = e.GetTouchPoint(this).Position.ToScreenPosition();
        if (OnPointerReleased([position]))
            return;
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
        var position = e.GetPosition(this).ToScreenPosition();

        if (OnPointerMoved([position], isHovering))
            return;

        if (!isHovering)
            _manipulationTracker.Manipulate([position], Map.Navigator.Manipulate);
    }

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

        var previousCenter = e.ManipulationOrigin.ToScreenPosition();
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
        if (GetPixelDensity() is not float pixelDensity)
            return;

        var canvas = args.Surface.Canvas;
        canvas.Scale(pixelDensity, pixelDensity);
        _renderController?.Render(canvas);
    }

    public float? GetPixelDensity()
    {
        if (PresentationSource.FromVisual(this) is not PresentationSource presentationSource)
            return null;
        if (presentationSource.CompositionTarget is not CompositionTarget compositionTarget)
            return null;

        var matrix = compositionTarget.TransformToDevice;

        var dpiX = matrix.M11;
        var dpiY = matrix.M22;

        if (dpiX != dpiY) throw new ArgumentException();

        return (float?)dpiX;
    }

    protected virtual void Dispose(bool disposing)
    {
        SharedDispose(disposing);
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
