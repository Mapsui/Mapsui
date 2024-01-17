using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Widgets;
using Mapsui.Widgets.BoxWidgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class BoxWidgetRenderer : ISkiaWidgetRenderer
{
    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        var boxWidget = (BoxWidget)widget;

        boxWidget.UpdateEnvelope(boxWidget.Width, boxWidget.Height, viewport.Width, viewport.Height);

        using var backPaint = new SKPaint { Color = boxWidget.BackColor.ToSkia(layerOpacity), IsAntialias = true };

        canvas.DrawRoundRect(boxWidget.Envelope?.ToSkia() ?? new SKRect(0, 0, (float)boxWidget.Width, (float)boxWidget.Height), (float)boxWidget.CornerRadius, (float)boxWidget.CornerRadius, backPaint);
    }
}
