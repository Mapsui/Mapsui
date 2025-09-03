using Microsoft.UI;
using SkiaSharp;

#if __UNO_SKIA__
using Windows.Foundation;
using Uno.WinUI.Graphics2DSK;
#endif

namespace Mapsui.UI.WinUI;

abstract partial class RenderControl : Microsoft.UI.Xaml.Controls.UserControl
{
    protected MapControl Owner { get; }
    protected System.Action<SKCanvas> RenderCallback { get; }

    protected RenderControl(MapControl owner, System.Action<SKCanvas> renderCallback)
    {
        Owner = owner;
        RenderCallback = renderCallback;
    }

    public static RenderControl CreateControl(MapControl owner, System.Action<SKCanvas> renderCallback)
    {
#if __UNO_SKIA__
        return new SKCanvasElementRenderControl(owner, renderCallback);
#else
        // GPU does not work currently on Windows
        bool useGPU = System.OperatingSystem.IsBrowser() || System.OperatingSystem.IsAndroid(); // Works not on iPhone Mini;
        return useGPU
            ? new SKSwapChainPanelRenderControl(owner, renderCallback)
            : new SKXamlCanvasRenderControl(owner, renderCallback);
#endif
    }

    public abstract void Invalidate();

    public abstract float? GetPixelDensity();
}

partial class SKXamlCanvasRenderControl : RenderControl
{
#pragma warning disable IDISP006
    private readonly SKXamlCanvas _skXamlCanvas;
#pragma warning restore IDISP006

    public SKXamlCanvasRenderControl(MapControl owner, System.Action<SKCanvas> renderCallback) : base(owner, renderCallback)
    {
        Content = _skXamlCanvas = new SKXamlCanvas
        {
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch,
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
            Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.Transparent)
        };
        _skXamlCanvas.PaintSurface += SKXamlCanvasOnPaintSurface;
    }

    private void SKXamlCanvasOnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (GetPixelDensity() is { } pixelDensity)
        {
            var canvas = e.Surface.Canvas;
            canvas.Scale(pixelDensity, pixelDensity);
            RenderCallback(canvas);
        }
    }

    public override void Invalidate() => _skXamlCanvas.Invalidate();

    public override float? GetPixelDensity()
    {
        var canvasWidth = _skXamlCanvas.CanvasSize.Width;
        var canvasActualWidth = _skXamlCanvas.ActualWidth;
        if (canvasWidth <= 0 || canvasActualWidth <= 0)
        {
            return null;
        }
        return (float)(canvasWidth / canvasActualWidth);
    }
}

partial class SKSwapChainPanelRenderControl : RenderControl
{
#pragma warning disable IDISP006
    private readonly SKSwapChainPanel _swapChainPanel;
#pragma warning restore IDISP006

    public SKSwapChainPanelRenderControl(MapControl owner, System.Action<SKCanvas> renderCallback) : base(owner, renderCallback)
    {
        Content = _swapChainPanel = new SKSwapChainPanel
        {
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch,
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
            Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.Transparent)
        };
        _swapChainPanel.PaintSurface += SwapChainPanelOnPaintSurface;
    }

    private void SwapChainPanelOnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
    {
        if (GetPixelDensity() is { } pixelDensity)
        {
            var canvas = e.Surface.Canvas;
            canvas.Scale(pixelDensity, pixelDensity);
            RenderCallback(canvas);
        }
    }

    public override void Invalidate() => _swapChainPanel.Invalidate();

    public override float? GetPixelDensity()
    {
        var canvasWidth = _swapChainPanel.CanvasSize.Width;
        var canvasActualWidth = _swapChainPanel.ActualWidth;
        if (canvasWidth <= 0 || canvasActualWidth <= 0)
        {
            return null;
        }
        return (float)(canvasWidth / canvasActualWidth);
    }
}

#if __UNO_SKIA__
partial class SKCanvasElementRenderControl : RenderControl
{
#pragma warning disable IDISP006
    private readonly MapControlSKCanvasElement _skCanvasElement;
#pragma warning restore IDISP006

    public SKCanvasElementRenderControl(MapControl owner, Action<SKCanvas> renderCallback) : base(owner, renderCallback)
    {
        Content = _skCanvasElement = new MapControlSKCanvasElement(this);
    }

    public override void Invalidate()
    {
        _skCanvasElement.Invalidate();
    }

    private class MapControlSKCanvasElement(SKCanvasElementRenderControl parent) : SKCanvasElement
    {
        protected override void RenderOverride(SKCanvas canvas, Size area)
        {
            if (parent.GetPixelDensity() is { } pixelDensity)
            {
                canvas.Scale(pixelDensity);
            }
            parent.RenderCallback(canvas);
        }
    }

    public override float? GetPixelDensity() => 1;
}
#endif
