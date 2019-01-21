using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Samples.Common
{
    public static class AllSamples
    {
        public static IEnumerable<ISample> GetSamples()
        {
            var type = typeof(ISample);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith("Mapsui"));

            return assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface)
                .Select(Activator.CreateInstance).Select(t => t as ISample)
                .OrderBy(s => s?.Name)
                .ThenBy(s => s?.Category)
                .ToList();
        }
    }
}
