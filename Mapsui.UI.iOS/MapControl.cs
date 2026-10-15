using CoreFoundation;
using Mapsui.Extensions;
using Mapsui.Manipulations;
using SkiaSharp.Views.iOS;
using System.ComponentModel;

namespace Mapsui.UI.iOS;

[Register("MapControl"), DesignTimeVisible(true)]
public partial class MapControl : UIView, IMapControl
{
    private SKMetalView? _metalCanvas;
    private SKCanvasView? _canvas;
    private bool _canvasInitialized;
    private readonly ManipulationTracker _manipulationTracker = new();
    private readonly List<UITouch> _activeTouches = []; // In the order in which the fingers touched down.
    public static bool UseGPU { get; set; } = true;

    public MapControl(CGRect frame)
        : base(frame)
    {
        LocalConstructor();
        SharedConstructor();
    }

    [Preserve]
    public MapControl(IntPtr handle) : base(handle) // Used when initialized from storyboard
    {
        LocalConstructor();
        SharedConstructor();
    }

    public void InvalidateCanvas()
    {
        RunOnUIThread(() =>
        {
            SetNeedsDisplay();
            _metalCanvas?.SetNeedsDisplay();
        });
    }

    private void InitializeCanvas()
    {
        if (!_canvasInitialized)
        {
            _canvasInitialized = true;
            if (UseGPU)
            {
                _metalCanvas?.Dispose();
                _metalCanvas = [];
            }
            else
            {
                _canvas?.Dispose();
                _canvas = [];
            }
        }
    }

    private void LocalConstructor()
    {
        InitializeCanvas();



        BackgroundColor = UIColor.White;

        if (UseGPU)
        {
            _metalCanvas!.TranslatesAutoresizingMaskIntoConstraints = false;
            _metalCanvas.MultipleTouchEnabled = true;
            _metalCanvas.PaintSurface += OnPaintSurface;
            AddSubview(_metalCanvas);

            AddConstraints(
            [
                NSLayoutConstraint.Create(this, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _metalCanvas,
                    NSLayoutAttribute.Leading, 1.0f, 0.0f),
                NSLayoutConstraint.Create(this, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _metalCanvas,
                    NSLayoutAttribute.Trailing, 1.0f, 0.0f),
                NSLayoutConstraint.Create(this, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _metalCanvas,
                    NSLayoutAttribute.Top, 1.0f, 0.0f),
                NSLayoutConstraint.Create(this, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _metalCanvas,
                    NSLayoutAttribute.Bottom, 1.0f, 0.0f)
            ]);
        }
        else
        {
            _canvas!.TranslatesAutoresizingMaskIntoConstraints = false;
            _canvas.MultipleTouchEnabled = true;
            _canvas.PaintSurface += OnPaintSurface;
            AddSubview(_canvas);

            AddConstraints(
            [
                NSLayoutConstraint.Create(this, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _canvas,
                    NSLayoutAttribute.Leading, 1.0f, 0.0f),
                NSLayoutConstraint.Create(this, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _canvas,
                    NSLayoutAttribute.Trailing, 1.0f, 0.0f),
                NSLayoutConstraint.Create(this, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _canvas,
                    NSLayoutAttribute.Top, 1.0f, 0.0f),
                NSLayoutConstraint.Create(this, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _canvas,
                    NSLayoutAttribute.Bottom, 1.0f, 0.0f)
            ]);
        }

        ClipsToBounds = true;
        MultipleTouchEnabled = true;
        UserInteractionEnabled = true;

        SharedOnSizeChanged(GetWidth(), GetHeight());
    }

    private void OnPaintSurface(object? sender, SKPaintMetalSurfaceEventArgs args)
    {
        // Keep the GPU context current so the renderer can create GPU-backed surfaces.
        Map.RenderService.GpuContext = args.Surface.Context;
        _renderController?.Render(args.Surface.Canvas, GetPixelDensity());
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        => _renderController?.Render(args.Surface.Canvas, GetPixelDensity());

    public override void TouchesBegan(NSSet touches, UIEvent? e)
    {
        Catch.Exceptions(() =>
        {
            base.TouchesBegan(touches, e);
            RemoveTouchesNotIn(e);
            AddTouches(touches);
            var positions = GetScreenPositions();

            if (positions.Length == 1)
                _manipulationTracker.Restart(positions);

            if (OnPointerPressed(positions))
                return;
        });
    }

    public override void TouchesMoved(NSSet touches, UIEvent? e)
    {
        Catch.Exceptions(() =>
        {
            base.TouchesMoved(touches, e);
            var positions = GetScreenPositions();

            if (OnPointerMoved(positions, false))
                return;

            _manipulationTracker.Manipulate(positions, Map.Navigator.Manipulate);
        });
    }

    public override void TouchesEnded(NSSet touches, UIEvent? e)
    {
        Catch.Exceptions(() =>
        {
            base.TouchesEnded(touches, e);
            // The lifted touches are still included here because OnPointerReleased only handles
            // the release of the last finger, which it recognizes by a single position.
            var positions = GetScreenPositions();
            RemoveTouches(touches);
            OnPointerReleased(positions);
        });
    }

    public override void TouchesCancelled(NSSet touches, UIEvent? e)
    {
        Catch.Exceptions(() =>
        {
            base.TouchesCancelled(touches, e);
            // No OnPointerReleased call, because a cancelled touch, for instance by an incoming call,
            // should not result in a tap or fling.
            RemoveTouches(touches);
        });
    }

    private ScreenPosition[] GetScreenPositions()
    {
        // The positions are taken from _activeTouches instead of UIEvent.AllTouches because AllTouches is an
        // unordered NSSet. The ManipulationTracker derives the rotation from the first two positions, so if
        // two fingers swap places between events the map rotates 180 degrees. See issue #2738.
        var positions = new ScreenPosition[_activeTouches.Count];
        for (var i = 0; i < _activeTouches.Count; i++)
        {
            var location = _activeTouches[i].LocationInView(this);
            positions[i] = new ScreenPosition(location.X, location.Y);
        }
        return positions;
    }

    private void AddTouches(NSSet touches)
    {
        // UIKit reuses the same UITouch instance for a finger during the entire gesture,
        // so it can be used to identify that finger.
        foreach (var touch in touches.OfType<UITouch>())
            if (IndexOfTouch(touch) < 0)
                _activeTouches.Add(touch);
    }

    private void RemoveTouches(NSSet touches)
    {
        foreach (var touch in touches.OfType<UITouch>())
        {
            var index = IndexOfTouch(touch);
            if (index >= 0)
                _activeTouches.RemoveAt(index);
        }
    }

    private void RemoveTouchesNotIn(UIEvent? e)
    {
        // Safeguard against touches that never received an ended or cancelled callback,
        // for instance because the view was removed from the window during a gesture.
        var allTouches = e?.AllTouches?.OfType<UITouch>().Select(t => t.Handle).ToHashSet() ?? [];
        _activeTouches.RemoveAll(t => !allTouches.Contains(t.Handle));
    }

    private int IndexOfTouch(UITouch touch) => _activeTouches.FindIndex(t => t.Handle == touch.Handle);

    private static void RunOnUIThread(Action action)
    {
        DispatchQueue.MainQueue.DispatchAsync(action);
    }

    public override CGRect Frame
    {
        get => base.Frame;
        set
        {
            InitializeCanvas();
            if (UseGPU)
            {
                _metalCanvas!.Frame = value;
            }
            else
            {
                _canvas!.Frame = value;
            }

            base.Frame = value;
            SharedOnSizeChanged(GetWidth(), GetHeight());
            OnPropertyChanged();
        }
    }

    public override void LayoutMarginsDidChange()
    {
        InitializeCanvas();
        if (_metalCanvas == null || _canvas == null) return;

        base.LayoutMarginsDidChange();
        SharedOnSizeChanged(GetWidth(), GetHeight());
    }

    public void OpenInBrowser(string url)
    {
        Catch.TaskRun(async () =>
        {
            using var nsUrl = new NSUrl(url);
            await UIApplication.SharedApplication.OpenUrlAsync(nsUrl, new UIApplicationOpenUrlOptions());
        });
    }

    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected new virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _map?.Dispose();
            Unsubscribe();
            _metalCanvas?.Dispose();
            _canvas?.Dispose();
            base.Dispose(disposing);
        }

        SharedDispose(disposing);
    }

    private double GetWidth()
    {
        InitializeCanvas();
        return UseGPU
            ? _metalCanvas!.Frame.Width
            : _canvas!.Frame.Width;
    }

    private double GetHeight()
    {
        InitializeCanvas();
        return UseGPU
            ? _metalCanvas!.Frame.Height
            : _canvas!.Frame.Height;
    }

    public float? GetPixelDensity()
    {
        InitializeCanvas();
        return UseGPU
            ? (float)_metalCanvas!.ContentScaleFactor
            : (float)_canvas!.ContentScaleFactor;
    }

    private static bool GetShiftPressed() => false;
}
