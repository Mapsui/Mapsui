// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Paul den Dulk (Geodan) as part of SharpMap

using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Manipulations;
using Mapsui.UI.WinUI.Extensions;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using SkiaSharp.Views.Windows;
using System;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.System;

namespace Mapsui.UI.WinUI;

public partial class MapControl : Grid, IMapControl, IDisposable
{
    private readonly Rectangle _selectRectangle = CreateSelectRectangle();
    private readonly SKXamlCanvas _canvas = CreateRenderTarget();
    bool _shiftPressed;

    public MapControl()
    {
        SharedConstructor();

        _invalidate = () =>
        {
            // The commented out code crashes the app when MouseWheelAnimation.Duration > 0. Could be a bug in SKXamlCanvas
            //if (Dispatcher.HasThreadAccess) _canvas?.Invalidate();
            //else RunOnUIThread(() => _canvas?.Invalidate());
            RunOnUIThread(() => _canvas?.Invalidate());
        };

        Background = new SolidColorBrush(Colors.White); // DON'T REMOVE! Touch events do not work without a background

        Children.Add(_canvas);
        Children.Add(_selectRectangle);

        _canvas.PaintSurface += Canvas_PaintSurface;

        Loaded += MapControlLoaded;

        SizeChanged += MapControlSizeChanged;

        ManipulationMode = ManipulationModes.Scale | ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Rotate;

        ManipulationInertiaStarting += OnManipulationInertiaStarting;
        ManipulationDelta += OnManipulationDelta;
        ManipulationCompleted += OnManipulationCompleted;

        PointerPressed += MapControl_PointerPressed;
        PointerMoved += MapControl_PointerMoved;
        PointerReleased += MapControl_PointerReleased;

        PointerWheelChanged += MapControl_PointerWheelChanged;

        KeyDown += MapControl_KeyDown;
        KeyUp += MapControl_KeyUp;

        var orientationSensor = SimpleOrientationSensor.GetDefault();
        if (orientationSensor != null)
            orientationSensor.OrientationChanged += (sender, args) => RunOnUIThread(() => Refresh());
    }

    private void MapControl_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Shift)
        {
            _shiftPressed = true;
        }
    }

    private void MapControl_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Shift)
        {
            _shiftPressed = false;
        }
    }

    private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        RefreshData();
    }

    private void MapControl_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var position = e.GetCurrentPoint(this).Position.ToScreenPosition();

        if (OnMapPointerPressed([position]))
            return;
    }

    private void MapControl_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        // This is a bit weird. The OnManipulationDelta event fires on both touch and mouse events
        // and deals with both properly, except for mouse hover events. This handler only deals with
        // hover events.
        if (!IsHovering(e))
            return;
        var position = e.GetCurrentPoint(this).Position.ToScreenPosition();

        if (OnMapPointerMoved([position], true)) // Only for hover events
            return;

        RefreshGraphics();
    }

    private void MapControl_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        var position = e.GetCurrentPoint(this).Position.ToScreenPosition();
        OnMapPointerReleased([position]);
    }

    private bool IsHovering(PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType == PointerDeviceType.Touch)
            return false;
        return !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
    }

    private static Rectangle CreateSelectRectangle()
    {
        return new Rectangle
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
    }

    private static SKXamlCanvas CreateRenderTarget()
    {
        return new SKXamlCanvas
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = new SolidColorBrush(Colors.Transparent)
        };
    }

    private void MapControl_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var mousePointerPoint = e.GetCurrentPoint(this);
        var mouseWheelDelta = mousePointerPoint.Properties.MouseWheelDelta;

        Map.Navigator.MouseWheelZoom(mouseWheelDelta, mousePointerPoint.ToScreenPosition());

        e.Handled = true;
    }

    private void MapControlLoaded(object sender, RoutedEventArgs e)
    {
        SetViewportSize();
    }

    private void MapControlSizeChanged(object sender, SizeChangedEventArgs e)
    {
        Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };
        SetViewportSize();
    }

    private void RunOnUIThread(Action action)
    {
        Catch.TaskRun(() => DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
            }
        }));
    }

    private void Canvas_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (PixelDensity <= 0)
            return;

        var canvas = e.Surface.Canvas;

        canvas.Scale(PixelDensity, PixelDensity);

        CommonDrawControl(canvas);
    }

    private static void OnManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
    {
        e.TranslationBehavior.DesiredDeceleration = 25 * 96.0 / (1000.0 * 1000.0);
    }

    private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
        var manipulation = ToManipulation(e);

        if (OnMapPointerMoved([manipulation.Center]))
            return;

        Map.Navigator.Manipulate(ToManipulation(e));
        RefreshGraphics();
    }

    private Manipulation ToManipulation(ManipulationDeltaRoutedEventArgs e)
    {
        var previousCenter = TransformToVisual(this).Inverse.TransformPoint(e.Position).ToScreenPosition();
        var center = previousCenter.Offset(e.Delta.Translation.X, e.Delta.Translation.Y);
        return new Manipulation(center, previousCenter, e.Delta.Scale, e.Delta.Rotation, e.Cumulative.Rotation);
    }

    public void OpenInBrowser(string url)
    {
        Catch.TaskRun(async () => await Launcher.LaunchUriAsync(new Uri(url)));
    }

    private double ViewportWidth => ActualWidth;
    private double ViewportHeight => ActualHeight;

    private double GetPixelDensity() => XamlRoot?.RasterizationScale ?? 1d;

    private bool GetShiftPressed() => _shiftPressed;

#if __ANDROID__
    protected override void Dispose(bool disposing)
#elif __IOS__ || __MACOS__
    protected new virtual void Dispose(bool disposing)
#else
    protected virtual void Dispose(bool disposing)
#endif
    {
        if (disposing)
        {
#if HAS_UNO   
#if  __WINUI__
#pragma warning disable IDISP023 // Don't use reference types in finalizer context
#endif

            _canvas?.Dispose();
            _selectRectangle?.Dispose();
#endif
#if HAS_UNO || __WINUI__
            _invalidateTimer?.Dispose();
#endif
            _map?.Dispose();

        }
        CommonDispose(disposing);

#if __ANDROID__ || __IOS__ || __MACOS__
        base.Dispose(disposing);
#endif
    }

#if !(__ANDROID__ )
#if __IOS__ || __MACOS__ || HAS_UNO
    public new void Dispose()
#else 
    public void Dispose()
#endif
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
#endif
}
