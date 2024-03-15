using Mapsui.Extensions;
using Mapsui.Manipulations;
using Mapsui.Nts.Extensions;
using Mapsui.UI;

namespace Mapsui.Nts.Editing;

public static class EditManipulation
{
    public static bool OnPointerPressed(ScreenPosition screenPosition, EditManager editManager, IMapControl mapControl)
    {
        // Take into account VertexRadius in feature select, because the objective
        // is to select the vertex.
        var mapInfo = mapControl.GetMapInfo(screenPosition, editManager.VertexRadius);
        if (editManager.EditMode == EditMode.Modify && mapInfo.Feature != null)
            return editManager.StartDragging(mapInfo, editManager.VertexRadius);
        if (editManager.EditMode == EditMode.Rotate && mapInfo.Feature != null)
            return editManager.StartRotating(mapInfo);
        if (editManager.EditMode == EditMode.Scale && mapInfo.Feature != null)
            return editManager.StartScaling(mapInfo);
        return false;
    }

    public static bool OnPointerMoved(ScreenPosition screenPosition, EditManager editManager, IMapControl mapControl, bool isHovering)
    {
        var result = false;
        if (isHovering)
        {
            editManager.HoveringVertex(mapControl.GetMapInfo(screenPosition));
            result = false;
        }
        else
        {
            var args = mapControl.GetMapInfo(screenPosition);
            if (editManager.EditMode == EditMode.Modify)
                result = editManager.Dragging(args?.WorldPosition?.ToPoint());
            if (editManager.EditMode == EditMode.Rotate)
                result = editManager.Rotating(args?.WorldPosition?.ToPoint());
            if (editManager.EditMode == EditMode.Scale)
                result = editManager.Scaling(args?.WorldPosition?.ToPoint());
        }
        mapControl.RefreshGraphics();
        return result;
    }

    public static bool OnPointerReleased(EditManager editManager)
    {
        if (editManager.IsManipulating())
        {
            // The EditingWidget captures the pointer when scaling or rotating.
            // It does this in an implicit way, by setting
            // the state of info classes. Resetting it releases the capture. 
            editManager.ResetManipulations();
            return true;
        }
        return false;
    }

    public static bool OnTapped(ScreenPosition screenPosition, EditManager editManager, IMapControl mapControl, TapType tapType, bool shiftPressed)
    {
        if (editManager.EditMode == EditMode.Modify)
            editManager.StopDragging();
        if (editManager.EditMode == EditMode.Rotate)
            editManager.StopRotating();
        if (editManager.EditMode == EditMode.Scale)
            editManager.StopScaling();

        if (editManager.EditMode == EditMode.Modify)
        {
            if (shiftPressed || tapType == TapType.Double)
            {
                return editManager.TryDeleteCoordinate(
                    mapControl.GetMapInfo(screenPosition, editManager.VertexRadius), editManager.VertexRadius);
            }
            return editManager.TryInsertCoordinate(
                mapControl.GetMapInfo(screenPosition, editManager.VertexRadius));
        }
        else if (editManager.EditMode is EditMode.DrawingPolygon or EditMode.DrawingLine)
        {
            if (shiftPressed || tapType == TapType.Double)
                return editManager.EndEdit();
            else
                editManager.AddVertex(mapControl.Map.Navigator.Viewport.ScreenToWorld(screenPosition).ToCoordinate());
        }
        else if (editManager.EditMode is EditMode.AddPoint or EditMode.AddLine or EditMode.AddPolygon)
            if (tapType == TapType.Single)
                editManager.AddVertex(mapControl.Map.Navigator.Viewport.ScreenToWorld(screenPosition).ToCoordinate());

        if (editManager.SelectMode)
        {
            var mapInfo = mapControl.GetMapInfo(screenPosition);
            if (mapInfo.Feature != null)
            {
                var currentValue = (bool?)mapInfo.Feature["Selected"] == true;
                mapInfo.Feature["Selected"] = !currentValue; // invert current value
            }
        }

        return false;
    }
}
