using Mapsui.Nts.Editing;
using Mapsui.UI;
using Mapsui.Widgets;

namespace Mapsui.Nts.Widgets;

public class EditingWidget : InputOnlyWidget // Derived from InputOnlyWidget because the EditingWidget does not need to draw anything
{
    private readonly IMapControl _mapControl;
    private readonly EditManager _editManager;

    public EditingWidget(IMapControl mapControl, EditManager editManager)
    {
        _mapControl = mapControl;
        _editManager = editManager;

        InputAreaType = InputAreaType.Map;
    }

    public override bool OnPointerPressed(Navigator navigator, WidgetEventArgs e)
        => EditManipulation.OnPointerPressed(e.Position, _editManager, _mapControl);

    public override bool OnPointerMoved(Navigator navigator, WidgetEventArgs e) =>
        EditManipulation.OnPointerMoved(e.Position, _editManager, _mapControl, !e.LeftButton);

    public override bool OnPointerReleased(Navigator navigator, WidgetEventArgs e) =>
        EditManipulation.OnPointerReleased(_editManager);

    public override bool OnTapped(Navigator navigator, WidgetEventArgs e) =>
        EditManipulation.OnTapped(e.Position, _editManager, _mapControl, e.TapType, e.ShiftPressed);
}
