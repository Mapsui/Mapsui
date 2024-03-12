using Mapsui.Nts.Editing;
using Mapsui.UI;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Samples.Common.Maps.Editing;

public class EditingAddPolygonSample : IMapControlSample
{
    public string Name => "Editing Add Polygon";
    public string Category => "Editing";
    public void Setup(IMapControl mapControl)
    {
        var editManager = EditingSample.InitEditMode(mapControl, EditMode.AddPolygon);
        mapControl.Map.Navigator.ZoomToBox(editManager.GetGrownExtent());
    }
}
