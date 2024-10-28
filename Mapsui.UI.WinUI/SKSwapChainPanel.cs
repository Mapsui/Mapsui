#if !HAS_UNO_WINUI
using SkiaSharp;
#if __WINUI__
// for fixing the Linux build this pragma disable is needed some tooling issue.
#pragma warning disable IDE0005 // Using directive is unnecessary.
#pragma warning disable CS0067 // Event is never used
using System;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
#endif

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
    public double ActualWidth => throw new NotImplementedException($"No Gpu Rendering implemented set MapControl.UseGPU to false");
    public SKSize CanvasSize => throw new NotImplementedException($"No Gpu Rendering implemented set MapControl.UseGPU to false");

    public void Invalidate()
    {
        throw new NotImplementedException($"No Gpu Rendering implemented set MapControl.UseGPU to false");
    }

    public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
}
#endif
