// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Paul den Dulk (Geodan) as part of SharpMap

#pragma warning disable IDISP001 // Dispose created.
#pragma warning disable IDISP002 // Dispose member.

using System;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.System;
using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Utilities;
using Mapsui.UI.WinUI.Extensions;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using SkiaSharp.Views.Windows;
using HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;

namespace Mapsui.UI.WinUI;

public partial class MapControl : Grid, IMapControl, IDisposable
{
    private readonly Rectangle _selectRectangle = CreateSelectRectangle();
    private readonly SKXamlCanvas _canvas = CreateRenderTarget();
    private double _virtualRotation;

    public MapControl()
    {
        CommonInitialize();
        Initialize();
    }

    private void Initialize()
    {
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

        PointerWheelChanged += MapControl_PointerWheelChanged;

        ManipulationMode = ManipulationModes.Scale | ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Rotate;
        ManipulationStarted += OnManipulationStarted;
        ManipulationDelta += OnManipulationDelta;
        ManipulationCompleted += OnManipulationCompleted;

        ManipulationInertiaStarting += OnManipulationInertiaStarting;

        Tapped += OnSingleTapped;
        DoubleTapped += OnDoubleTapped;
        PointerMoved += MapControl_PointerMoved;
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
            this.ShiftPressed = true;
        }
    }

    private void MapControl_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Shift)
        {
            this.ShiftPressed = false;
        }
    }

    private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        RefreshData();
        Console.WriteLine(Guid.NewGuid());
    }

    private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
    {
        _virtualRotation = Map.Navigator.Viewport.Rotation;
    }

    private void MapControl_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var position = e.GetCurrentPoint(this).Position.ToMapsui();
        if (HandleMoving(position, true, 0, e.KeyModifiers == VirtualKeyModifiers.Shift))
            e.Handled = true;
    }

    private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        var tapPosition = e.GetPosition(this).ToMapsui();
        if (HandleTouchingTouched(tapPosition, true, 2, ShiftPressed))
        {
            e.Handled = true;
            return;
        }

        OnInfo(CreateMapInfoEventArgs(tapPosition, tapPosition, 2));
    }

    public bool ShiftPressed { get; set; }

    private void OnSingleTapped(object sender, TappedRoutedEventArgs e)
    {
        var tabPosition = e.GetPosition(this).ToMapsui();
        if (HandleTouchingTouched(tabPosition, true, 1, ShiftPressed))
        {
            e.Handled = true;
            return;
        }

        OnInfo(CreateMapInfoEventArgs(tabPosition, tabPosition, 1));
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
            StrokeDashArray = new DoubleCollection { 3.0 },
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
        var currentPoint = e.GetCurrentPoint(this);
        var currentMousePosition = new MPoint(currentPoint.Position.X, currentPoint.Position.Y);
        var mouseWheelDelta = currentPoint.Properties.MouseWheelDelta;

        Map.Navigator.MouseWheelZoom(mouseWheelDelta, currentMousePosition);

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
        var center = e.Position.ToMapsui();
        var radius = e.Delta.Scale;
        var rotation = e.Delta.Rotation;

        var previousCenter = e.Position.ToMapsui().Offset(-e.Delta.Translation.X, -e.Delta.Translation.Y);
        var previousRadius = 1f;

        double rotationDelta = 0;

        if (Map.Navigator.RotationLock == false)
        {
            _virtualRotation += rotation;

            rotationDelta = RotationCalculations.CalculateRotationDeltaWithSnapping(
                _virtualRotation, Map.Navigator.Viewport.Rotation, _unSnapRotationDegrees, _reSnapRotationDegrees);
        }

        Map.Navigator.Pinch(center, previousCenter, radius / previousRadius, rotationDelta);
        e.Handled = true;
    }

    public void OpenBrowser(string url)
    {
        Catch.TaskRun(async () => await Launcher.LaunchUriAsync(new Uri(url)));
    }

    private float ViewportWidth => (float)ActualWidth;
    private float ViewportHeight => (float)ActualHeight;

    private float GetPixelDensity()
    {
        return (float)(XamlRoot?.RasterizationScale ?? 1f);
    }

#pragma warning disable IDISP023 // Don't use reference types in finalizer context
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
