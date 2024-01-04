using System;
using System.Collections.Generic;

namespace Mapsui;

[Obsolete("Use Info and ILayerFeatureInfo", true)]
public class FeatureInfoEventArgs : EventArgs
{
    public IDictionary<string, IEnumerable<IFeature>>? FeatureInfo { get; set; }
}
