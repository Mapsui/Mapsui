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

    public MapControl(CGRect frame)
        : base(frame)
    {
        SharedConstructor();
        LocalConstructor();
    }

    [Preserve]
    public MapControl(IntPtr handle) : base(handle) // Used when initialized from storyboard
    {
        SharedConstructor();
        LocalConstructor();
    }

    public static bool UseGPU { get; set; } = true;


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

        _invalidate = () =>
        {
            RunOnUIThread(() =>
            {
                SetNeedsDisplay();
                _metalCanvas?.SetNeedsDisplay();
            });
        };

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

        Map.Navigator.SetSize(ViewportWidth, ViewportHeight);
    }

    private void OnPaintSurface(object? sender, SKPaintMetalSurfaceEventArgs args)
    {
        if (PixelDensity <= 0)
            return;

        var canvas = args.Surface.Canvas;
        canvas.Scale(PixelDensity, PixelDensity);
        CommonDrawControl(canvas);
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
    {
        if (PixelDensity <= 0)
            return;

        var canvas = args.Surface.Canvas;
        canvas.Scale(PixelDensity, PixelDensity);
        CommonDrawControl(canvas);
    }

    public override void TouchesBegan(NSSet touches, UIEvent? e)
    {
        Catch.Exceptions(() =>
        {
            base.TouchesBegan(touches, e);
            var positions = GetScreenPositions(e, this);

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
            var positions = GetScreenPositions(e, this);

            if (OnMapPointerMoved(positions))
                return;

            _manipulationTracker.Manipulate(positions, Map.Navigator.Manipulate);
        });
    }

    public override void TouchesEnded(NSSet touches, UIEvent? e)
    {
        Catch.Exceptions(() =>
        {
            base.TouchesEnded(touches, e);
            var positions = GetScreenPositions(e, this);
            OnPointerReleased(positions);
        });
    }

    private static ReadOnlySpan<ScreenPosition> GetScreenPositions(UIEvent? uiEvent, UIView uiView)
    {
        if (uiEvent is null)
            return [];
        return uiEvent.AllTouches.Select(t => ((UITouch)t).LocationInView(uiView)).Select(p => new ScreenPosition(p.X, p.Y)).ToArray();
    }

    /// <summary>
    /// Gets screen position in device independent units (or DIP or DP).
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private static MPoint GetScreenPosition(CGPoint point)
    {
        return new MPoint(point.X, point.Y);
    }

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
            SetViewportSize();
            OnPropertyChanged();
        }
    }

    public override void LayoutMarginsDidChange()
    {
        InitializeCanvas();
        if (_metalCanvas == null || _canvas == null) return;

        base.LayoutMarginsDidChange();
        SetViewportSize();
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

        CommonDispose(disposing);
    }

    private double ViewportWidth
    {
        get
        {
            InitializeCanvas();
            return UseGPU
                ? _metalCanvas!.Frame.Width
                : _canvas!.Frame.Width;
        }
    }

    private double ViewportHeight
    {
        get
        {
            InitializeCanvas();
            return UseGPU
                ? _metalCanvas!.Frame.Height
                : _canvas!.Frame.Height;
        }
    }

    private double GetPixelDensity()
    {
        InitializeCanvas();
        return UseGPU
            ? (double)_metalCanvas!.ContentScaleFactor
            : (double)_canvas!.ContentScaleFactor;
    }

    private static bool GetShiftPressed() => false;
}
