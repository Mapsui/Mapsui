using System;
using System.Windows.Input;
using Mapsui.Nts.Extensions;
using Mapsui.Samples.Wpf.Editing.Editing;
using Mapsui.UI.Wpf;

namespace Mapsui.Samples.Wpf.Editing;

public enum MouseState
{
    Down,
    Up,
    Dragging, // moving with mouse down
    Moving, // moving with mouse up
    DoubleClick
}

public class EditManipulation
{
    private MPoint? _mouseDownPosition;
    private bool _inDoubleClick;

    public static int MinPixelsMovedForDrag { get; set; } = 4;

    public bool Manipulate(MouseState mouseState, MPoint screenPosition,
        EditManager editManager, MapControl mapControl)
    {
        switch (mouseState)
        {
            case MouseState.Up:
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
                        if (IsShiftDown())
                        {
                            return editManager.TryDeleteCoordinate(
                                mapControl.GetMapInfo(screenPosition, editManager.VertexRadius), editManager.VertexRadius);
                        }
                        return editManager.TryInsertCoordinate(
                            mapControl.GetMapInfo(screenPosition, editManager.VertexRadius));
                    }
                    return editManager.AddVertex(mapControl.Viewport.ScreenToWorld(screenPosition).ToCoordinate());
                }
                return false;
            case MouseState.Down:
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
            case MouseState.Dragging:
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
            case MouseState.Moving:
                editManager.HoveringVertex(mapControl.GetMapInfo(screenPosition));
                return false;
            case MouseState.DoubleClick:
                _inDoubleClick = true;
                if (editManager.EditMode != EditMode.Modify)
                    return editManager.EndEdit();
                return false;
            default:
                throw new Exception();
        }
    }

    private static bool IsShiftDown()
    {
        return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
    }

    private static bool IsClick(MPoint? screenPosition, MPoint? mouseDownScreenPosition)
    {
        if (mouseDownScreenPosition == null || screenPosition == null)
            return false;
        return mouseDownScreenPosition.Distance(screenPosition) < MinPixelsMovedForDrag;
    }
}
