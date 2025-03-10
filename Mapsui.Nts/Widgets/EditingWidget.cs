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

    public override bool OnPointerPressed(WidgetEventArgs e)
        => EditManipulation.OnPointerPressed(e, _editManager);

    public override bool OnPointerMoved(WidgetEventArgs e) =>
        EditManipulation.OnPointerMoved(e, _editManager);

    public override bool OnPointerReleased(WidgetEventArgs e) =>
        EditManipulation.OnPointerReleased(_editManager);

    public override bool OnTapped(WidgetEventArgs e) =>
        EditManipulation.OnTapped(e, _editManager);
}
