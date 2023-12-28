using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Samples.CustomWidget;

public class CustomWidget : Widget, ITouchableWidget
{
    private Color? _color = Color.Orange;

    public Color? Color
    {
        get => _color;
        set
        {
            if (_color == value)
                return;
            _color = value;
            OnPropertyChanged();
        }
    }

    public TouchableAreaType TouchableArea => TouchableAreaType.Widget;

    public bool HandleWidgetTouched(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        navigator.CenterOn(0, 0);
        return true;
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
