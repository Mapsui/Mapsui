using System;
using System.Linq;
using System.Collections.Generic;
using Mapsui.Samples.Common;

namespace Mapsui.Tests.Common
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
                .Where(i => i?.Category == "Tests").ToList();
        }
    }
}