using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Manipulations;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Diagnostics;

namespace Mapsui.UI.WindowsForms;

public partial class MapControl : UserControl, IMapControl, IDisposable
{
    public static bool UseGPU = true;

    private readonly SKGLControl? _glView;
    private readonly SKControl? _canvasView;
    private readonly ManipulationTracker _manipulationTracker = new();
    private bool _disposed;

    public MapControl()
    {
        SharedConstructor();

        Control view;

        Dock = DockStyle.Fill;
        AutoSize = true;
        BackColor = Color.White;
        Resize += MapControlResize;

        if (UseGPU)
        {
            // Use GPU backend
            _glView = new SKGLControl();
            // Events
            _invalidate = () =>
            {
                if (!_glView.IsHandleCreated)
                    return;

                Invoke(() => _glView.Invalidate());
            };
            _glView.PaintSurface += OnGLPaintSurface;
            view = _glView;
        }
        else
        {
            // Use CPU backend
            _canvasView = new SKControl();
            // Events
            _invalidate = () =>
            {
                if (!_canvasView.IsHandleCreated)
                    return;

                Invoke(() => _canvasView.Invalidate());
            };
            _canvasView.PaintSurface += OnPaintSurface;
            view = _canvasView;
        }

        // Common events
        view.MouseDown += MapControlMouseDown;
        view.MouseMove += MapControlMouseMove;
        view.MouseUp += MapControlMouseUp;
        view.MouseWheel += MapControlMouseWheel;

        view.Dock = DockStyle.Fill;

        Controls.Add(view);
    }

    private void MapControlResize(object? sender, EventArgs e)
    {
        SetViewportSize();
    }

    private double ViewportWidth => Width;
    private double ViewportHeight => Height;

    private void OnGLPaintSurface(object? sender, SKPaintGLSurfaceEventArgs args)
    {
        if (_glView?.GRContext is null)
        {
            // Could this be null before Home is called? If so we should change the logic.
            Logger.Log(LogLevel.Warning, "Refresh can not be called because GRContext is null");
            return;
        }

        // Called on UI thread
        PaintSurface(args.Surface.Canvas);
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
    {
        // Called on UI thread
        PaintSurface(args.Surface.Canvas);
    }

    private void PaintSurface(SKCanvas canvas)
    {
        if (PixelDensity <= 0)
            return;

        canvas.Scale(PixelDensity, PixelDensity);

        CommonDrawControl(canvas);
    }

    private void MapControlMouseDown(object? sender, MouseEventArgs e)
    {
        var position = GetScreenPosition(e.Location);
        _manipulationTracker.Restart([position]);

        OnMapPointerPressed([position]);
    }

    private void MapControlMouseMove(object? sender, MouseEventArgs e)
    {
        var isHovering = IsHovering(e);
        var position = GetScreenPosition(e.Location);

        if (OnMapPointerMoved([position], isHovering))
            return;

        if (!isHovering)
            _manipulationTracker.Manipulate([position], Map.Navigator.Manipulate);
    }

    private void MapControlMouseUp(object? sender, MouseEventArgs e)
    {
        var position = GetScreenPosition(e.Location);
        OnMapPointerReleased([position]);
    }

    private void MapControlMouseWheel(object? sender, MouseEventArgs e)
    {
        var mouseWheelDelta = e.Delta;
        var mousePosition = GetScreenPosition(e.Location);
        Map.Navigator.MouseWheelZoom(mouseWheelDelta, mousePosition);
    }

    private static bool IsHovering(MouseEventArgs e)
    {
        return e.Button != MouseButtons.Left;
    }

    private static bool GetShiftPressed()
    {
        return (ModifierKeys & Keys.Shift) == Keys.Shift;
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

    public ScreenPosition GetScreenPosition(Point position)
    {
        return new ScreenPosition(position.X, position.Y);
    }

    public double GetPixelDensity()
    {
        return (UseGPU ? _glView!.CanvasSize.Width : _canvasView!.CanvasSize.Width) / Width;
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (disposing)
        {
            CommonDispose(disposing);

            _glView?.Dispose();
            _canvasView?.Dispose();
        }

        base.Dispose(disposing);
    }
}
