using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Samples.Common;

public static class AllSamples
{
    private static readonly List<ISampleBase> _registeredSamples = new();

    public static void Register(ISampleBase sample)
    {
        _registeredSamples.Add(sample);
    }

    public static IEnumerable<ISampleBase> GetSamples()
    {
        return _registeredSamples.OrderBy(f => f.Name);
    }
}
