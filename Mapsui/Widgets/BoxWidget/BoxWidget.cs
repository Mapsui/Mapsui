using System;
using System.Collections.Generic;
using System.Text;
using Mapsui.Styles;

namespace Mapsui.Widgets.BoxWidget;
public class BoxWidget : Widget
{
    public int CornerRadius { get; set; } = 8;
    public Color? BackColor { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public override bool HandleWidgetTouched(Navigator navigator, MPoint position)
    {
        return false;
    }
}
