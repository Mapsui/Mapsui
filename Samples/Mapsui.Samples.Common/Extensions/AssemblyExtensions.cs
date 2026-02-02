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

        var destPath = Path.Combine(folder, resourceFile);

        using var image = assembly.GetManifestResourceStream(embeddedResourcesPath + resourceFile);
        if (image == null) throw new ArgumentException("EmbeddedResource not found");

        if (File.Exists(destPath))
        {
            var fileInfo = new FileInfo(destPath);
            if (fileInfo.Length == image.Length)
            {
                return;
            }
        }

        using var dest = File.Create(destPath);
        image.CopyTo(dest);
    }
}
