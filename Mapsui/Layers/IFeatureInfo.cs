using System;
using System.Collections.Generic;
using Mapsui.Providers;

namespace Mapsui.Layers
{
    public interface IFeatureInfo
    {
        void GetFeatureInfo(IViewport viewport, double x, double y, Action<IDictionary<string, IEnumerable<IFeature>>> callback);
    }
}
