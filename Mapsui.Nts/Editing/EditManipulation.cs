using System;
using Mapsui.Extensions;
using Mapsui.Manipulations;
using Mapsui.Nts.Extensions;
using Mapsui.UI;
using Mapsui.Widgets;

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
    private ScreenPosition? _mouseDownPosition;
    private bool _inDoubleClick;

    public static int MinPixelsMovedForDrag { get; set; } = 4;

    public bool Manipulate(PointerState mouseState, ScreenPosition screenPosition,
        EditManager editManager, IMapControl mapControl, WidgetEventArgs e)
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

                if (IsTap(screenPosition, _mouseDownPosition))
                {
                    if (editManager.EditMode == EditMode.Modify)
                    {
                        if (e.Shift || e.TapCount == 2)
                        {
                            return editManager.TryDeleteCoordinate(
                                mapControl.GetMapInfo(screenPosition, editManager.VertexRadius), editManager.VertexRadius);
                        }
                        return editManager.TryInsertCoordinate(
                            mapControl.GetMapInfo(screenPosition, editManager.VertexRadius));
                    }
                    else if (editManager.EditMode == EditMode.DrawingPolygon || editManager.EditMode == EditMode.DrawingLine)
                    {
                        if (e.Shift || e.TapCount == 2)
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
                // There is a lot to improve in the edit logic. When scaling or rotating a 
                // geometry the editing widget captures the pointer. It does this by setting
                // the state of info classes. Resetting it releases the capture. We call this
                // method not on hover, which it is also a bit weird, PointerRelease would be
                // more logical but we don't have that event yet.
                editManager.ResetManipulations();
                return false;
            default:
                throw new Exception();
        }
    }

    private static bool IsTap(ScreenPosition? screenPosition, ScreenPosition? mouseDownScreenPosition)
    {
        if (mouseDownScreenPosition is null || screenPosition is null)
            return false;
        return mouseDownScreenPosition.Value.Distance(screenPosition.Value) < MinPixelsMovedForDrag;
    }
}
