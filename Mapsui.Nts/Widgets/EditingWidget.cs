using Mapsui.Manipulations;
using Mapsui.Nts.Editing;
using Mapsui.UI;
using Mapsui.Widgets;

namespace Mapsui.Nts.Widgets;
public class EditingWidget : Widget, ITouchableWidget
{
    public IMapControl MapControl { get; }
    public EditManager EditManager { get; }
    public EditManipulation EditManipulation { get; }

    public TouchableAreaType TouchableArea => TouchableAreaType.Widget;

    public EditingWidget(IMapControl mapControl, EditManager editManager, EditManipulation editManipulation)
    {
        MapControl = mapControl;
        EditManager = editManager;
        EditManipulation = editManipulation;
    }

    public bool OnTapped(Navigator navigator, ScreenPosition position, WidgetEventArgs e)
    {
        if (!e.LeftButton)
            return false;

        if (MapControl.Map != null)
            MapControl.Map.Navigator.PanLock = EditManipulation.Manipulate(
                PointerState.Up, position, EditManager, MapControl, e);

        if (EditManager.SelectMode)
        {
            var infoArgs = MapControl.GetMapInfo(position);
            if (infoArgs?.Feature != null)
            {
                var currentValue = (bool?)infoArgs.Feature["Selected"] == true;
                infoArgs.Feature["Selected"] = !currentValue; // invert current value
            }
        }

        return false;
    }

    public bool OnPointerPressed(Navigator navigator, ScreenPosition position, WidgetEventArgs e)
    {
        if (!e.LeftButton)
            return false;

        if (MapControl.Map == null)
            return false;

        return EditManipulation.Manipulate(
            PointerState.Down, position, EditManager, MapControl, e);
    }

    public bool OnPointerMoved(Navigator navigator, ScreenPosition position, WidgetEventArgs e)
    {
        if (e.LeftButton)
            return EditManipulation.Manipulate(PointerState.Dragging, position, EditManager, MapControl, e);
        else
            return EditManipulation.Manipulate(PointerState.Hovering, position, EditManager, MapControl, e);
    }
}
