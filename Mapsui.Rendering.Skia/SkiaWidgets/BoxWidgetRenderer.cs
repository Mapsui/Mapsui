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
        // The textRect has an offset which can be confusing. 
        // This is because DrawText's origin is the baseline of the text, not the bottom.
        // Read more here: https://developer.xamarin.com/guides/xamarin-forms/advanced/skiasharp/basics/text/

        canvas.DrawRoundRect(boxWidget.Envelope?.ToSkia() ?? new SKRect(0, 0, (float)boxWidget.Width, (float)boxWidget.Height), boxWidget.CornerRadius, boxWidget.CornerRadius, backPaint);
    }
}
