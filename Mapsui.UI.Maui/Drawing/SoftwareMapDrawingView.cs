// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Mapsui.UI.Maui.Drawing;

internal sealed class SoftwareMapDrawingView : MapDrawingView
{
    private SKCanvasView CanvasView => (SKCanvasView)View;

    public SoftwareMapDrawingView() : base(new SKCanvasView { EnableTouchEvents = true })
    {
        CanvasView.PaintSurface += OnPaintSurface;
        CanvasView.Touch += OnTouch;
    }

    public override SKSize CanvasSize => CanvasView.CanvasSize;
    public override void Invalidate() => CanvasView.InvalidateSurface();
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs args) => OnPaint(args.Surface.Canvas, null);

    protected override void Detach()
    {
        CanvasView.PaintSurface -= OnPaintSurface;
        CanvasView.Touch -= OnTouch;
    }
}
