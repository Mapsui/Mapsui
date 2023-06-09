using Mapsui.Extensions;
using Mapsui.UI;

namespace Mapsui.Nts.Editing;

public class EditConnector
{
    public EditConnector(IMapControlEdit mapControl, EditManager editManager, EditManipulation editManipulation)
    {
        EditManager = editManager;
        MapControl = mapControl;
        EditManipulation = editManipulation;
        MapControl.EditMouseMove += MapControlOnMouseMove;
        MapControl.EditMouseLeftButtonDown += MapControlOnMouseLeftButtonDown;
        MapControl.EditMouseLeftButtonUp += MapControlOnMouseLeftButtonUp;
    }
    
    public IMapControlEdit MapControl { get; }
    
    public EditManager EditManager { get; }
    
    public EditManipulation EditManipulation { get; }
    
    private void MapControlOnMouseMove(object sender, EditMouseArgs args)
    {
        var screenPosition = args.ScreenPosition;

        if (args.LeftButton)
        {
            EditManipulation.Manipulate(MouseState.Dragging, screenPosition,
                EditManager, MapControl);
        }
        else
        {
            EditManipulation.Manipulate(MouseState.Moving, screenPosition,
                EditManager, MapControl);
        }
    }

    private void MapControlOnMouseLeftButtonUp(object sender, EditMouseArgs args)
    {
        if (MapControl.Map != null)
            MapControl.Map.Navigator.PanLock = EditManipulation.Manipulate(MouseState.Up,
                args.ScreenPosition, EditManager, MapControl);

        if (EditManager.SelectMode)
        {
            var infoArgs = MapControl.GetMapInfo(args.ScreenPosition);
            if (infoArgs?.Feature != null)
            {
                var currentValue = (bool?)infoArgs.Feature["Selected"] == true;
                infoArgs.Feature["Selected"] = !currentValue; // invert current value
            }
        }
    }

    private void MapControlOnMouseLeftButtonDown(object sender, EditMouseArgs args)
    {
        if (MapControl.Map == null)
            return;

        if (args.ClickCount > 1)
        {
            MapControl.Map.Navigator.PanLock = EditManipulation.Manipulate(MouseState.DoubleClick,
                args.ScreenPosition, EditManager, MapControl);
            args.Handled = true;
        }
        else
        {
            MapControl.Map.Navigator.PanLock = EditManipulation.Manipulate(MouseState.Down,
                args.ScreenPosition, EditManager, MapControl);
        }
    }
}
