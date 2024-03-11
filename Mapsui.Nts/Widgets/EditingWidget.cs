using Mapsui.Nts.Editing;
using Mapsui.UI;
using Mapsui.Widgets;

namespace Mapsui.Nts.Widgets;
public class EditingWidget : InputOnlyWidget // Derived from InputOnlyWidget because the EditingWidget does not need to draw anything
{
    public IMapControl MapControl { get; }
    public EditManager EditManager { get; }
    public EditManipulation EditManipulation { get; }

    public EditingWidget(IMapControl mapControl, EditManager editManager, EditManipulation editManipulation)
    {
        InputAreaType = InputAreaType.Map;
        MapControl = mapControl;
        EditManager = editManager;
        EditManipulation = editManipulation;
    }

    public override bool OnTapped(Navigator navigator, WidgetEventArgs e)
    {
        if (!e.LeftButton)
            return false;

        if (MapControl.Map != null)
            MapControl.Map.Navigator.PanLock = EditManipulation.Manipulate(
                PointerState.Tapped, e.Position, EditManager, MapControl, e);

        if (EditManager.SelectMode)
        {
            var infoArgs = MapControl.GetMapInfo(e.Position);
            if (infoArgs?.Feature != null)
            {
                var currentValue = (bool?)infoArgs.Feature["Selected"] == true;
                infoArgs.Feature["Selected"] = !currentValue; // invert current value
            }
        }

        return false;
    }

    public override bool OnPointerPressed(Navigator navigator, WidgetEventArgs e)
    {
        if (!e.LeftButton)
            return false;

        if (MapControl.Map == null)
            return false;

        return EditManipulation.Manipulate(
            PointerState.Down, e.Position, EditManager, MapControl, e);
    }

    public override bool OnPointerMoved(Navigator navigator, WidgetEventArgs e)
    {
        if (e.LeftButton)
            return EditManipulation.Manipulate(PointerState.Dragging, e.Position, EditManager, MapControl, e);
        else
            return EditManipulation.Manipulate(PointerState.Hovering, e.Position, EditManager, MapControl, e);
    }

    public override bool OnPointerReleased(Navigator navigator, WidgetEventArgs e)
    {
        return EditManipulation.Manipulate(PointerState.Up, e.Position, EditManager, MapControl, e);
    }
}
