using System;
using System.Collections.Generic;
using Mapsui.Samples.Wpf.Editing.Samples;

namespace Mapsui.Samples.Wpf.Editing;

public static class AllSamples
{
    public static Dictionary<string, Func<Map>> CreateList()
    {
        return new Dictionary<string, Func<Map>>
        {
            ["editing"] = EditingSample.CreateMap
        };
    }
}
