using System.Collections.Generic;

namespace SharpMap.Providers.Wms
{
    public class FeatureInfo
    {
        public string LayerName { get; set; }
        public List<Dictionary<string, string>> FeatureInfos { get; set; }
    }
}
