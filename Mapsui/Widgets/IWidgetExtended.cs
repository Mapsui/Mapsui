namespace Mapsui.Widgets;
public interface IWidgetExtended : IWidget, IWidgetTouchable
{
    public bool HandleWidgetMoving(Navigator navigator, MPoint position, WidgetArgs args);
    public bool HandleWidgetTouching(Navigator navigator, MPoint position, WidgetArgs args);
    public bool HandleWidgetTouched(Navigator navigator, MPoint position, WidgetArgs args);
}
