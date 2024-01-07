using System;
using Mapsui.Extensions;
using Mapsui.Nts.Extensions;
using Mapsui.UI;

namespace Mapsui.Nts.Editing;

public enum PointerState
{
    Down,
    Up,
    Dragging, // Moving with pointer down
    Hovering, // Moving with pointer up. Will not occur on touch which should not be a problem because it is only used as preview.
}

public class EditManipulation
{
    private MPoint? _mouseDownPosition;
    private bool _inDoubleClick;

    public static int MinPixelsMovedForDrag { get; set; } = 4;

    public bool Manipulate(PointerState mouseState, MPoint screenPosition,
        EditManager editManager, IMapControl mapControl, bool isShiftDown)
    {
        switch (mouseState)
        {
            case PointerState.Up:
                if (_inDoubleClick) // Workaround to prevent that after a double click the 'up' event will immediately add a new geometry.
                {
                    _inDoubleClick = false;
                    return false;
                }

                if (editManager.EditMode == EditMode.Modify)
                    editManager.StopDragging();
                if (editManager.EditMode == EditMode.Rotate)
                    editManager.StopRotating();
                if (editManager.EditMode == EditMode.Scale)
                    editManager.StopScaling();

                if (IsClick(screenPosition, _mouseDownPosition))
                {
                    if (editManager.EditMode == EditMode.Modify)
                    {
                        if (isShiftDown)
                        {
                            return editManager.TryDeleteCoordinate(
                                mapControl.GetMapInfo(screenPosition, editManager.VertexRadius), editManager.VertexRadius);
                        }
                        return editManager.TryInsertCoordinate(
                            mapControl.GetMapInfo(screenPosition, editManager.VertexRadius));
                    }
                    else if (editManager.EditMode == EditMode.DrawingPolygon || editManager.EditMode == EditMode.DrawingLine)
                    {
                        if (isShiftDown)
                        {
                            return editManager.EndEdit();
                        }
                    }
                    return editManager.AddVertex(mapControl.Map.Navigator.Viewport.ScreenToWorld(screenPosition).ToCoordinate());
                }
                return false;
            case PointerState.Down:
                {
                    _mouseDownPosition = screenPosition;
                    // Take into account VertexRadius in feature select, because the objective
                    // is to select the vertex.
                    var mapInfo = mapControl.GetMapInfo(screenPosition, editManager.VertexRadius);
                    if (editManager.EditMode == EditMode.Modify && mapInfo?.Feature != null)
                    {
                        return editManager.StartDragging(mapInfo, editManager.VertexRadius);
                    }
                    if (editManager.EditMode == EditMode.Rotate && mapInfo?.Feature != null)
                    {
                        return editManager.StartRotating(mapInfo);
                    }
                    if (editManager.EditMode == EditMode.Scale && mapInfo?.Feature != null)
                    {
                        return editManager.StartScaling(mapInfo);
                    }
                    return false;
                }
            case PointerState.Dragging:
                {
                    var args = mapControl.GetMapInfo(screenPosition);
                    if (editManager.EditMode == EditMode.Modify)
                        return editManager.Dragging(args?.WorldPosition?.ToPoint());
                    if (editManager.EditMode == EditMode.Rotate)
                        return editManager.Rotating(args?.WorldPosition?.ToPoint());
                    if (editManager.EditMode == EditMode.Scale)
                        return editManager.Scaling(args?.WorldPosition?.ToPoint());

                    return false;
                }
            case PointerState.Hovering:
                editManager.HoveringVertex(mapControl.GetMapInfo(screenPosition));
                return false;
            default:
                throw new Exception();
        }
    }

    private static bool IsClick(MPoint? screenPosition, MPoint? mouseDownScreenPosition)
    {
        if (mouseDownScreenPosition == null || screenPosition == null)
            return false;
        return mouseDownScreenPosition.Distance(screenPosition) < MinPixelsMovedForDrag;
    }
}
