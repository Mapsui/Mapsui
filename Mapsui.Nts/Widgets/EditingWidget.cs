using Mapsui.Nts.Editing;
using Mapsui.UI;
using Mapsui.Widgets;

namespace Mapsui.Nts.Widgets;
public class EditingWidget : InputOnlyWidget // Derived from InputOnlyWidget because the EditingWidget does not need to draw anything
{
    private IMapControl _mapControl;
    private EditManager _editManager;

    public EditingWidget(IMapControl mapControl, EditManager editManager)
    {
        InputAreaType = InputAreaType.Map;
        _mapControl = mapControl;
        _editManager = editManager;
    }

    public override bool OnTapped(Navigator navigator, WidgetEventArgs e)
    {
        _mapControl.Map.Navigator.PanLock = EditManipulation.OnTapped(e.Position, _editManager, _mapControl, e.TapType, e.ShiftPressed);

        if (_editManager.SelectMode)
        {
            var infoArgs = _mapControl.GetMapInfo(e.Position);
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
        return EditManipulation.OnPointerPressed(e.Position, _editManager, _mapControl);
    }

    public override bool OnPointerMoved(Navigator navigator, WidgetEventArgs e)
    {
        return EditManipulation.OnPointerMoved(e.Position, _editManager, _mapControl, !e.LeftButton);
    }

    public override bool OnPointerReleased(Navigator navigator, WidgetEventArgs e)
    {
        return EditManipulation.OnPointerReleased(_editManager);
    }
}
