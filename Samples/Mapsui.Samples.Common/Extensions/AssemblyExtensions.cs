using System;
using System.IO;
using System.Reflection;

namespace Mapsui.Samples.Common.Extensions;

public static class AssemblyExtensions
{
    public static void CopyEmbeddedResourceToFile(
        this Assembly assembly,
        string embeddedResourcesPath,
        string folder,
        string resourceFile)
    {
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        using var image = assembly.GetManifestResourceStream(embeddedResourcesPath + resourceFile);
        if (image == null) throw new ArgumentException("EmbeddedResource not found");
        var destPath = Path.Combine(folder, resourceFile);
        using var dest = File.Create(destPath);
        image.CopyTo(dest);
    }
}
