// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Mapsui.UI.Maui.Drawing;

internal sealed class GpuMapDrawingView : MapDrawingView
{
    private SKGLView GlView => (SKGLView)View;

    public GpuMapDrawingView() : base(new SKGLView { HasRenderLoop = false, EnableTouchEvents = true })
    {
        GlView.PaintSurface += OnPaintSurface;
        GlView.Touch += OnTouch;
    }

    public override SKSize CanvasSize => GlView.CanvasSize;
    public override void Invalidate() => GlView.InvalidateSurface();
    private void OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs args) => OnPaint(args.Surface.Canvas, GlView.GRContext);

    protected override void Detach()
    {
        GlView.PaintSurface -= OnPaintSurface;
        GlView.Touch -= OnTouch;
    }
}
