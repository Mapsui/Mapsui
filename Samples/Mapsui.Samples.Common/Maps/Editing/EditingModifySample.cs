using Mapsui.Nts.Editing;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps.Editing;

public class EditingModifySample : IMapControlSample
{
    public string Name => "Editing Modify";
    public string Category => "Editing";
    public void Setup(IMapControl mapControl)
    {
        EditingSample.InitEditMode(mapControl, EditMode.Modify);
    }
}
