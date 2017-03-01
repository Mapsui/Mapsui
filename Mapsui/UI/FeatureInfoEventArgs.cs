using System;
using System.Collections.Generic;
using Mapsui.Providers;

namespace Mapsui.UI
{
    public class FeatureInfoEventArgs : EventArgs
    {
        public IDictionary<string, IEnumerable<IFeature>> FeatureInfo { get; set; }
    }
}