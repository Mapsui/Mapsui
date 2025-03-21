using System;

namespace Mapsui.Widgets;

/// <summary> Widget for getting Tapped event on the map </summary>
public class MapTappedWidget : InputOnlyWidget // Derived from InputOnlyWidget because the EditingWidget does not need to draw anything
{
    private readonly Func<WidgetEventArgs, bool> _handlerTap;

    public MapTappedWidget(Func<WidgetEventArgs, bool> handlerTap)
    {
        _handlerTap = handlerTap;
        InputAreaType = InputAreaType.Map;
    }

    public override void OnTapped(WidgetEventArgs e)
    {
        base.OnTapped(e);
        if (e.Handled)
            return;

        if (_handlerTap(e))
            e.Handled = true;
    }
}
