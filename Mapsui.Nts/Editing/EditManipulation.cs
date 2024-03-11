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
    Tapped,
    Dragging, // Moving with pointer down
    Hovering, // Moving with pointer up. Will not occur on touch which should not be a problem because it is only used as preview.
    Up
}

public class EditManipulation
{
    public static int MinPixelsMovedForDrag { get; set; } = 4;

    public bool Manipulate(PointerState mouseState, ScreenPosition screenPosition,
        EditManager editManager, IMapControl mapControl, WidgetEventArgs e)
    {
        return mouseState switch
        {
            PointerState.Tapped => OnTapped(screenPosition, editManager, mapControl, e),
            PointerState.Down => OnPointerPressed(screenPosition, editManager, mapControl),
            PointerState.Dragging => OnPointerMoved(screenPosition, editManager, mapControl, false),
            PointerState.Hovering => OnPointerMoved(screenPosition, editManager, mapControl, true),
            PointerState.Up => OnPointerReleased(editManager),
            _ => throw new Exception(),
        };
    }

    private static bool OnPointerReleased(EditManager editManager)
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

    private static bool OnPointerMoved(ScreenPosition screenPosition, EditManager editManager, IMapControl mapControl, bool isHovering)
    {
        if (isHovering)
        {
            editManager.HoveringVertex(mapControl.GetMapInfo(screenPosition));
            return false;
        }

        var args = mapControl.GetMapInfo(screenPosition);
        if (editManager.EditMode == EditMode.Modify)
            return editManager.Dragging(args?.WorldPosition?.ToPoint());
        if (editManager.EditMode == EditMode.Rotate)
            return editManager.Rotating(args?.WorldPosition?.ToPoint());
        if (editManager.EditMode == EditMode.Scale)
            return editManager.Scaling(args?.WorldPosition?.ToPoint());
        return false;
    }

    private static bool OnPointerPressed(ScreenPosition screenPosition, EditManager editManager, IMapControl mapControl)
    {
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

    private static bool OnTapped(ScreenPosition screenPosition, EditManager editManager, IMapControl mapControl, WidgetEventArgs e)
    {
        if (editManager.EditMode == EditMode.Modify)
            editManager.StopDragging();
        if (editManager.EditMode == EditMode.Rotate)
            editManager.StopRotating();
        if (editManager.EditMode == EditMode.Scale)
            editManager.StopScaling();

        if (editManager.EditMode == EditMode.Modify)
        {
            if (e.Shift || e.TapType == TapType.Double)
            {
                return editManager.TryDeleteCoordinate(
                    mapControl.GetMapInfo(screenPosition, editManager.VertexRadius), editManager.VertexRadius);
            }
            return editManager.TryInsertCoordinate(
                mapControl.GetMapInfo(screenPosition, editManager.VertexRadius));
        }
        else if (editManager.EditMode == EditMode.DrawingPolygon || editManager.EditMode == EditMode.DrawingLine)
        {
            if (e.Shift || e.TapType == TapType.Double)
            {
                return editManager.EndEdit();
            }
        }
        return editManager.AddVertex(mapControl.Map.Navigator.Viewport.ScreenToWorld(screenPosition).ToCoordinate());
    }
}
