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
            MapControl.Map.Navigator.PanLock = EditManipulation.Manipulate(MouseState.Up,
                position, EditManager, MapControl, args.Shift);

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

        if (args.ClickCount > 1)
        {
            MapControl.Map.Navigator.PanLock = EditManipulation.Manipulate(MouseState.DoubleClick,
                position, EditManager, MapControl, args.Shift);
            return true;
        }

        MapControl.Map.Navigator.PanLock = EditManipulation.Manipulate(MouseState.Down,
            position, EditManager, MapControl, args.Shift);

        return false;
    }

    public bool HandleWidgetMoving(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        var screenPosition = position;

        if (args.LeftButton)
        {
            EditManipulation.Manipulate(MouseState.Dragging, screenPosition,
                EditManager, MapControl, args.Shift);
        }
        else
        {
            EditManipulation.Manipulate(MouseState.Moving, screenPosition,
                EditManager, MapControl, args.Shift);
        }

        return false;
    }
}
