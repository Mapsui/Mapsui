using Mapsui.Nts.Editing;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps.Editing;

public class EditingModifySample : IMapControlSample
{
    public string Name => "Editing Modify";
    public string Category => "Editing";
    public void Setup(IMapControl mapControl)
    {
        var editManager = EditingSample.InitEditMode(mapControl, EditMode.Modify);
        mapControl.Map.Navigator.ZoomToBox(editManager.GetGrownExtent());
    }
}
