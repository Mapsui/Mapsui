using Mapsui.Widgets.BoxWidgets;
using System;

namespace Mapsui.Widgets.ButtonWidgets;

public class HyperlinkWidget : TextBoxWidget, ITouchableWidget
{
    public string? Url { get; set; }

    public TouchableAreaType TouchableArea => TouchableAreaType.Widget;

    public event EventHandler<HyperlinkWidgetArguments>? Touched;

    public bool HandleWidgetTouched(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        var arguments = new HyperlinkWidgetArguments();

        Touched?.Invoke(this, arguments);

        return args.Handled;
    }

    public bool HandleWidgetTouching(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        return false;
    }

    public bool HandleWidgetMoving(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        return false;
    }
}

public class HyperlinkWidgetArguments
{
    public bool Handled = false;
}
