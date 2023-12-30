using System;
using System.Collections.Generic;
using System.Text;
using Mapsui.Styles;

namespace Mapsui.Widgets.BoxWidget;
public class BoxWidget : Widget
{
    public double CornerRadius { get; set; } = 8;
    public Color? BackColor { get; set; } = new(255, 255, 255, 128);
}
