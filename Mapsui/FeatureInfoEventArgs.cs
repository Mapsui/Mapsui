using System;
using System.Collections.Generic;

namespace Mapsui;

public class FeatureInfoEventArgs : EventArgs
{
    public IDictionary<string, IEnumerable<IFeature>>? FeatureInfo { get; set; }
}
