using System;
using System.Collections.Generic;
using System.Text;

namespace Mapsui.Widgets.MousePositionWidget;

public class MousePositionWidget : TextBox, IWidgetExtended
{
    public MousePositionWidget()
    {
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Bottom;
        Text = "Mouse Position";
    }

    public bool HandleWidgetMoving(Navigator navigator, MPoint position, WidgetArgs args)
    {
        // update the Mouse position
        this.Text = $"{position.X:F0}, {position.Y:F0}";
        return false;
    }

    public bool HandleWidgetTouching(Navigator navigator, MPoint position, WidgetArgs args)
    {
        return false;
    }

    public bool HandleWidgetTouched(Navigator navigator, MPoint position, WidgetArgs args)
    {
        return false;
    }

    public bool Global => true;
}
