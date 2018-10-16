using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Samples.Common
{
    public static class AllSamples
    {
        public static IEnumerable<IDemoSample> GetSamples()
        {
            var type = typeof(IDemoSample);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith("Mapsui"));

            return assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface)
                .Select(Activator.CreateInstance).Select(t => t as IDemoSample)
                .OrderBy(s => s?.Name)
                .ToList();
        }
    }
}
