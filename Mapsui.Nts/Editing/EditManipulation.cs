using Mapsui.Extensions;
using Mapsui.Manipulations;
using Mapsui.Nts.Extensions;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Nts.Editing;

public static class EditManipulation
{
    public static bool OnPointerPressed(WidgetEventArgs e, EditManager editManager)
    {
        var editLayer = editManager.Layer;
        if (editLayer == null)
            return false;

        var mapInfo = e.GetMapInfo([editLayer]);
        if (editManager.EditMode == EditMode.Modify && mapInfo.Feature != null)
            return editManager.StartDragging(mapInfo, editManager.VertexRadius);
        if (editManager.EditMode == EditMode.Rotate && mapInfo.Feature != null)
            return editManager.StartRotating(mapInfo);
        if (editManager.EditMode == EditMode.Scale && mapInfo.Feature != null)
            return editManager.StartScaling(mapInfo);
        return false;
    }

    public static bool OnPointerMoved(WidgetEventArgs e, EditManager editManager)
    {
        var editLayer = editManager.Layer;
        if (editLayer == null)
            return false;

        var result = false;

        if (e.TapType == TapType.Hover)
        {
            editManager.HoveringVertex(e.GetMapInfo([]));
            result = false;
        }
        else
        {
            var args = e.GetMapInfo([editLayer]);
            if (editManager.EditMode == EditMode.Modify)
                result = editManager.Dragging(args?.WorldPosition?.ToPoint());
            if (editManager.EditMode == EditMode.Rotate)
                result = editManager.Rotating(args?.WorldPosition?.ToPoint());
            if (editManager.EditMode == EditMode.Scale)
                result = editManager.Scaling(args?.WorldPosition?.ToPoint());
        }
        editLayer.DataHasChanged();
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

    public static bool OnTapped(Navigator navigator, WidgetEventArgs e, EditManager editManager)
    {
        var editLayer = editManager.Layer;
        if (editLayer == null)
            return false;

        if (editManager.EditMode == EditMode.Modify)
            editManager.StopDragging();
        if (editManager.EditMode == EditMode.Rotate)
            editManager.StopRotating();
        if (editManager.EditMode == EditMode.Scale)
            editManager.StopScaling();

        if (editManager.EditMode == EditMode.Modify)
        {
            var mapInfo = e.GetMapInfo([editLayer]);
            if (e.ShiftPressed || e.TapType == TapType.DoubleTap || e.TapType == TapType.LongPress)
            {
                return editManager.TryDeleteCoordinate(
                    mapInfo, editManager.VertexRadius);
            }
            else if (mapInfo.MapInfoRecord?.Style is not SymbolStyle) // Do not add a vertex when tapping on a vertex because that is not usually what you want.
            {
                return editManager.TryInsertCoordinate(
                    e.GetMapInfo([editLayer]));
            }
        }
        else if (editManager.EditMode is EditMode.DrawingPolygon or EditMode.DrawingLine)
        {
            if (e.ShiftPressed || e.TapType == TapType.DoubleTap || e.TapType == TapType.LongPress)
            {
                if (e.TapType != TapType.DoubleTap) // Add last vertex but not on a double tap because it is preceded by a single tap.
                    editManager.AddVertex(navigator.Viewport.ScreenToWorld(e.ScreenPosition).ToCoordinate());
                return editManager.EndEdit();
            }
            else
                editManager.AddVertex(navigator.Viewport.ScreenToWorld(e.ScreenPosition).ToCoordinate());
        }
        else if (editManager.EditMode is EditMode.AddPoint or EditMode.AddLine or EditMode.AddPolygon)
            if (e.TapType == TapType.SingleTap)
                editManager.AddVertex(navigator.Viewport.ScreenToWorld(e.ScreenPosition).ToCoordinate());

        if (editManager.SelectMode)
        {
            var mapInfo = e.GetMapInfo([editLayer]);
            if (mapInfo.Feature != null)
            {
                var currentValue = (bool?)mapInfo.Feature["Selected"] == true;
                mapInfo.Feature["Selected"] = !currentValue; // invert current value
            }
        }

        return false;
    }
}
