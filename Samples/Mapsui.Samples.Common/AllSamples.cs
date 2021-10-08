using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mapsui.Logging;

namespace Mapsui.Samples.Common
{
    public static class AllSamples
    {
        public static IEnumerable<ISample> GetSamples()
        {
            var type = typeof(ISample);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith("Mapsui"));

            try
            {
                return assemblies
                    .SelectMany(s => s.GetTypes())
                    .Where(p => type.IsAssignableFrom(p) && !p.IsInterface)
                    .Select(Activator.CreateInstance).Select(t => t as ISample)
                    .OrderBy(s => s?.Name)
                    .ThenBy(s => s?.Category)
                    .ToList();
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();
                foreach (var exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    if (exSub is FileNotFoundException exFileNotFound)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                Logger.Log(LogLevel.Error, sb.ToString(), ex);
            }

            return null;
        }
    }
}
