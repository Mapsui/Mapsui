using Mapsui.Nts.Editing;
using Mapsui.UI;
using Mapsui.Widgets;

namespace Mapsui.Nts.Widgets;

public class EditingWidget : Widget, IWidgetExtended
{
    public IMapControl MapControl { get; }
    public EditManager EditManager { get; }
    public EditManipulation EditManipulation { get; }

    public EditingWidget(IMapControl mapControl, EditManager editManager, EditManipulation editManipulation)
    {
        MapControl = mapControl;
        EditManager = editManager;
        EditManipulation = editManipulation;
    }

    public override bool HandleWidgetTouched(Navigator navigator, MPoint position)
    {
        return false;
    }

    public bool HandleWidgetMoving(Navigator navigator, MPoint position, WidgetArgs args)
    {
        var screenPosition = position;

        if (args.LeftButton)
        {
            EditManipulation.Manipulate(PointerState.Dragging, screenPosition,
                EditManager, MapControl, args.Shift);
        }
        else
        {
            EditManipulation.Manipulate(PointerState.Hovering, screenPosition,
                EditManager, MapControl, args.Shift);
        }

        return false;
    }

    public bool HandleWidgetTouching(Navigator navigator, MPoint position, WidgetArgs args)
    {
        if (!args.LeftButton)
            return false;

        if (MapControl.Map == null)
            return false;

        MapControl.Map.Navigator.PanLock = EditManipulation.Manipulate(PointerState.Down,
            position, EditManager, MapControl, args.Shift);

        return false;
    }

    public bool HandleWidgetTouched(Navigator navigator, MPoint position, WidgetArgs args)
    {
        if (!args.LeftButton)
            return false;

        if (MapControl.Map != null)
            MapControl.Map.Navigator.PanLock = EditManipulation.Manipulate(PointerState.Up,
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
}
