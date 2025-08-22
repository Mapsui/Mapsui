using Mapsui.Nts.Editing;
using Mapsui.Widgets;

namespace Mapsui.Nts.Widgets;

public class EditingWidget : InputOnlyWidget // Derived from InputOnlyWidget because the EditingWidget does not need to draw anything
{
    private readonly EditManager _editManager;

    public EditingWidget(EditManager editManager)
    {
        _editManager = editManager;
        InputAreaType = InputAreaType.Map;
    }

    public override void OnPointerPressed(WidgetEventArgs e) =>
        e.Handled = EditManipulation.OnPointerPressed(e, _editManager);

    public override void OnPointerMoved(WidgetEventArgs e) =>
        e.Handled = EditManipulation.OnPointerMoved(e, _editManager);

    public override void OnPointerReleased(WidgetEventArgs e) =>
        e.Handled = EditManipulation.OnPointerReleased(_editManager);

    public override void OnTapped(WidgetEventArgs e) =>
        e.Handled = EditManipulation.OnTapped(e, _editManager);
}
