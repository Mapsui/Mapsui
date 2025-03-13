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

    public override bool OnTapped(WidgetEventArgs e)
    {
        var result = base.OnTapped(e);
        if (!result)
        {
            result = _handlerTap(e);
        }

        return result;
    }
}
