using System.Collections.Generic;

namespace Mapsui.Desktop.Wms
{
    public class FeatureInfo
    {
        public string LayerName { get; set; }
        public List<Dictionary<string, string>> FeatureInfos { get; set; }
    }
}
