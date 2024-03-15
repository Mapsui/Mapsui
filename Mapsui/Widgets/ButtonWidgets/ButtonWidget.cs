using Mapsui.Widgets.BoxWidgets;
using System;

namespace Mapsui.Widgets.ButtonWidgets;

public class ButtonWidget : TextBoxWidget
{
    /// <summary>
    /// Event which is called if widget is touched
    /// </summary>
    public Func<ButtonWidget, WidgetEventArgs, bool> Tapped = (s, e) => false;


    /// <summary>
    /// Handle touch to Widget
    /// </summary>
    /// <param name="navigator">Navigator used by map</param>
    /// <param name="e">Arguments for widget event</param>
    /// <returns>True, if touch is handled</returns>
    public override bool OnTapped(Navigator navigator, WidgetEventArgs e)
    {
        return Tapped(this, e);
    }
}
