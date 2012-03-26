using System.Collections.Generic;

namespace SharpMap.Providers.Wms
{
    public class FeatureInfo
    {
        public string LayerName { get; set; }
        public Dictionary<string, string> Attributes { get; set; }

        public FeatureInfo()
        {
            Attributes = new Dictionary<string, string>();
        }
    }
}
