using System;
using System.IO;
using System.Reflection;
using Mapsui.Samples.Common.Maps;

namespace Mapsui.Samples.Common.Helpers
{
    public static class MbTilesHelper
    {

        /// <summary>
        /// Copies a number of embedded resources to the local file system.
        /// </summary>
        public static void DeployMbTilesFile(string folder)
        {
            // So what is this all about?
            // I don't know how to access the file as part of the apk (let me know if there is a simple way)
            // So I store them as embedded resources and copy them to disk on startup.
            // (Is there a way to access sqlite files directly as memory stream?).

            var embeddedResourcesPath = "Mapsui.Samples.Common.EmbeddedResources.";
            var mbTileFiles = new[] { "world.mbtiles", "el-molar.mbtiles", "torrejon-de-ardoz.mbtiles" };

            foreach (var mbTileFile in mbTileFiles)
            {
                CopyEmbeddedResourceToStorage(embeddedResourcesPath, folder, mbTileFile);
            }
        }

        private static void CopyEmbeddedResourceToStorage(string embeddedResourcesPath, string folder,
            string mbTilesFile)
        {
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
