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

    public bool HandleWidgetTouched(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        if (!args.LeftButton)
            return false;

        if (MapControl.Map != null)
            MapControl.Map.Navigator.PanLock = EditManipulation.Manipulate(
                PointerState.Up, position, EditManager, MapControl, args.Shift);

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

    public bool HandleWidgetTouching(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        if (!args.LeftButton)
            return false;

        if (MapControl.Map == null)
            return false;

        return EditManipulation.Manipulate(
            PointerState.Down, position, EditManager, MapControl, args.Shift);
    }

    public bool HandleWidgetMoving(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        var screenPosition = position;

        if (args.LeftButton)
        {
            EditManipulation.Manipulate(
                PointerState.Dragging, screenPosition, EditManager, MapControl, args.Shift);
        }
        else
        {
            EditManipulation.Manipulate(
                PointerState.Hovering, screenPosition, EditManager, MapControl, args.Shift);
        }

        return false;
    }
}
