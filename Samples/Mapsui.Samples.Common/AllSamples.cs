using Mapsui.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mapsui.Samples.Common;

public static class AllSamples
{
    public static IEnumerable<ISampleBase> GetSamples()
    {
        var type = typeof(ISampleBase);
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName?.StartsWith("Mapsui") ?? false) ?? Array.Empty<Assembly>();

        try
        {
            return (assemblies
                    .SelectMany(s => s.GetTypes())
                    .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
                    .Select(Activator.CreateInstance)).Where(f => f is not null).OfType<ISampleBase>()
                    .OrderBy(s => s?.Name)
                    .ThenBy(s => s?.Category)
                    .ToList();
        }
        catch (ReflectionTypeLoadException ex)
        {
            var sb = new StringBuilder();
            foreach (var exSub in ex.LoaderExceptions)
            {
                if (exSub == null)
                    continue;

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

        return new List<ISampleBase>();
    }
}
