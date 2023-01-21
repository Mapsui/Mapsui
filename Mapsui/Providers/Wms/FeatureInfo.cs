using System.Collections.Generic;

namespace Mapsui.Providers.Wms;

public class FeatureInfo
{
    public string? LayerName { get; set; }
    public List<Dictionary<string, string>>? FeatureInfos { get; set; }
}
