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
        if (MapControl.Map != null)
            MapControl.Map.Navigator.PanLock = EditManipulation.OnTapped(e.Position, EditManager, MapControl, e.TapType, e.ShiftPressed);

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
        return EditManipulation.OnPointerPressed(e.Position, EditManager, MapControl);
    }

    public override bool OnPointerMoved(Navigator navigator, WidgetEventArgs e)
    {
        return EditManipulation.OnPointerMoved(e.Position, EditManager, MapControl, !e.LeftButton);
    }

    public override bool OnPointerReleased(Navigator navigator, WidgetEventArgs e)
    {
        return EditManipulation.OnPointerReleased(EditManager);
    }
}
