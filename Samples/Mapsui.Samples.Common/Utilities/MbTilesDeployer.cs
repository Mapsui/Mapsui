using System;
using System.IO;
using System.Reflection;
using Mapsui.Samples.Common.Maps;

namespace Mapsui.Samples.Common.Utilities
{
    public static class MbTilesDeployer
    {
        public static string MbTilesLocation { get; set; } =  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mapsui.Samples");

        public static void CopyEmbeddedResourceToFile(string fileName)
        {
            CopyEmbeddedResourceToFile("Mapsui.Samples.Common.EmbeddedResources.", MbTilesLocation, fileName);
        }

        private static void CopyEmbeddedResourceToFile(string embeddedResourcesPath, string folder,
            string mbTilesFile)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var assembly = typeof(PointsSample).GetTypeInfo().Assembly;
            using (var image = assembly.GetManifestResourceStream(embeddedResourcesPath + mbTilesFile))
            {
                if (image == null) throw new ArgumentException("EmbeddedResource not found");
                using (var dest = File.Create(Path.Combine(folder, mbTilesFile)))
                {
                    image.CopyTo(dest);
                }
            }
        }
    }
}
