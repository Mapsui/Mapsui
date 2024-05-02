using Mapsui.Extensions;
using Mapsui.Logging;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Diagnostics;

namespace Mapsui.UI.WindowsForms;

public partial class MapControl : UserControl, IMapControl, IDisposable
{
    public static bool UseGPU = true;

    private readonly SKGLControl? _glView;
    private readonly SKControl? _canvasView;
    private bool _disposed;

    public MapControl()
    {
        SharedConstructor();

        Control view;

        BackColor = Color.White;
        Resize += MapControl_Resize;

        if (UseGPU)
        {
            // Use GPU backend
            _glView = new SKGLControl
            {
            };
            // Events
            _glView.MouseClick += HandleClick;
            _glView.DoubleClick += HandleDoubleClick;
            _invalidate = () =>
            {
                // The line below sometimes has a null reference exception on application close.
                Invoke(() => _glView.Invalidate());
            };
            _glView.PaintSurface += OnGLPaintSurface;
            view = _glView;
        }
        else
        {
            // Use CPU backend
            _canvasView = new SKControl
            {
            };
            // Events
            _canvasView.Click += HandleClick;
            _canvasView.DoubleClick += HandleDoubleClick;
            _invalidate = () => { Invoke(() => _canvasView.Invalidate()); };
            _canvasView.PaintSurface += OnPaintSurface;
            view = _canvasView;
        }

        view.Dock = DockStyle.Fill;

        Controls.Add(view);
    }

    private void MapControl_Resize(object? sender, EventArgs e)
    {
        SetViewportSize();
    }

    private void HandleClick(object? sender, EventArgs e)
    {
        if (e is MouseEventArgs mouseEventArgs)
        {
            var point = mouseEventArgs.Location;

        }
        throw new NotImplementedException();
    }

    private void HandleDoubleClick(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private void HandleClick(object? sender, MouseEventArgs e)
    {
        throw new NotImplementedException();
    }

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

    private double ViewportWidth => Width;
    private double ViewportHeight => Height;

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

    public double GetPixelDensity()
    {
        // TODO
        return 1; // DeviceDpi;
    }

    public bool GetShiftPressed()
    {
        // TODO
        return false;
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

    protected virtual void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
