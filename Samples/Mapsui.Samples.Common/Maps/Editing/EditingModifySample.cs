using Mapsui.Nts.Editing;
using Mapsui.UI;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Samples.Common.Maps.Editing;

public class ModifyScaleSample : IMapControlSample
{
    public string Name => "Editing Scale";
    public string Category => "Editing";
    public void Setup(IMapControl mapControl)
    {
        EditingSample.InitEditMode(mapControl, EditMode.Modify);
    }
}
