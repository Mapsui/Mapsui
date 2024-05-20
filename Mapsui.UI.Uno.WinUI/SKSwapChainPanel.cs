#if !HAS_UNO_WINUI
using SkiaSharp.Views.Windows;
using WinRT;

namespace Mapsui.UI.WinUI;

internal class SKSwapChainPanel : UIElement
{
    public SKSwapChainPanel() : base(DerivedComposed.Instance)
    {
        throw new NotImplementedException($"No Gpu Rendering implemented set MapControl.UseGPU to false");
    }

    public VerticalAlignment VerticalAlignment { get; set; }
    public HorizontalAlignment HorizontalAlignment { get; set; }
    public SolidColorBrush Background { get; set; }

    public void Invalidate()
    {
    }

    public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
}
#endif
