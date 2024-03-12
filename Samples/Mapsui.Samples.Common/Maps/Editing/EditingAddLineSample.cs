﻿using Mapsui.Nts.Editing;
using Mapsui.UI;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Samples.Common.Maps.Editing;

public class EditingAddLineSample : IMapControlSample
{
    public string Name => "Editing Add Line";
    public string Category => "Editing";
    public void Setup(IMapControl mapControl)
    {
        var editManager = EditingSample.InitEditMode(mapControl, EditMode.AddLine);
        mapControl.Map.Navigator.ZoomToBox(editManager.GetGrownExtent());
    }
}
