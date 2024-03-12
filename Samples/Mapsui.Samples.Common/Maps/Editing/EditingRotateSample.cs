﻿using Mapsui.Nts.Editing;
using Mapsui.UI;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Samples.Common.Maps.Editing;

public class EditingScaleSample : IMapControlSample
{
    public string Name => "Editing Scale";
    public string Category => "Editing";
    public void Setup(IMapControl mapControl)
    {
        var editManager = EditingSample.InitEditMode(mapControl, EditMode.Scale);
        mapControl.Map.Navigator.ZoomToBox(editManager.GetGrownExtent());
    }
}
