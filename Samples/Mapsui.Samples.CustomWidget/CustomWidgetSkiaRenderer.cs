using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.SkiaWidgets;
using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Samples.CustomWidget;

public class CustomWidgetSkiaRenderer : ISkiaWidgetRenderer
{
    public void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IWidget widget, float layerOpacity)
    {
        // Cast to custom widget to be able to access the specific CustomWidget fields
        var customWidget = (CustomWidget)widget;

        // Update the envelope so the MapControl can do hit detection
        widget.Envelope = ToEnvelope(customWidget);

        // Use the envelope to draw
        using var skPaint = new SKPaint { Color = customWidget.Color.ToSkia(0.5f) };
        canvas.DrawRect(widget.Envelope.ToSkia(), skPaint);
    }

    private static MRect ToEnvelope(CustomWidget customWidget)
    {
        // A better implementation would take into account widget alignment
        return new MRect(customWidget.MarginX, customWidget.MarginY,
            customWidget.MarginX + customWidget.Width,
            customWidget.MarginY + customWidget.Height);
    }
}
