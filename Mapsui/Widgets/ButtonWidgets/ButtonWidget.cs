﻿using Mapsui.Manipulations;
using Mapsui.Widgets.BoxWidgets;
using System;

namespace Mapsui.Widgets.ButtonWidgets;

public class ButtonWidget : TextBoxWidget, ITouchableWidget
{
    /// <summary>
    /// Event which is called if widget is touched
    /// </summary>
    public Func<ButtonWidget, WidgetEventArgs, bool> Tapped = (s, e) => false;

    /// <summary>
    /// Type of area to use for touch events
    /// </summary>
    public TouchableAreaType TouchableArea => TouchableAreaType.Widget;

    /// <summary>
    /// Handle touch to Widget
    /// </summary>
    /// <param name="navigator">Navigator used by map</param>
    /// <param name="position">Position of touch</param>
    /// <param name="e">Arguments for widget event</param>
    /// <returns>True, if touch is handled</returns>
    public virtual bool OnTapped(Navigator navigator, ScreenPosition position, WidgetEventArgs e)
    {
        return Tapped(this, e);
    }

    public virtual bool OnPointerPressed(Navigator navigator, ScreenPosition position, WidgetEventArgs e)
    {
        return false;
    }

    public virtual bool OnPointerMoved(Navigator navigator, ScreenPosition position, WidgetEventArgs e)
    {
        return false;
    }
}
