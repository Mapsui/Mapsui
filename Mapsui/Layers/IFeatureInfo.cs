using System;
using System.Collections.Generic;
using SharpMap.Providers;

namespace SharpMap.Layers
{
    public interface IFeatureInfo
    {
        void GetFeatureInfo(IView view, double x, double y, Action<IDictionary<string, IEnumerable<IFeature>>> callback);
    }
}
