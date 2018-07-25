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
        /// <param name="createFile"></param>
        public static void DeployMbTilesFile(Func<string, Stream> createFile)
        {
            // So what is this all about?
            // I don't know how to access the file as part of the apk (let me know if there is a simple way)
            // So I store them as embbeded resources and copy them to disk on startup.
            // (Is there a way to access sqlite files directly as memory stream?).

            var embeddedResourcesPath = "Mapsui.Samples.Common.EmbeddedResources.";
            var mbTileFiles = new[] { "world.mbtiles", "el-molar.mbtiles", "torrejon-de-ardoz.mbtiles" };

            foreach (var mbTileFile in mbTileFiles)
            {
                CopyEmbeddedResourceToStorage(embeddedResourcesPath, mbTileFile, createFile);
            }
        }

        private static void CopyEmbeddedResourceToStorage(string embeddedResourcesPath, string mbTilesFile,
            Func<string, Stream> createFile)
        {
            var assembly = typeof(PointsSample).GetTypeInfo().Assembly;
            using (var image = assembly.GetManifestResourceStream(embeddedResourcesPath + mbTilesFile))
            {
                if (image == null) throw new ArgumentException("EmbeddedResource not found");
                using (var dest = createFile(mbTilesFile))
                {
                    image.CopyTo(dest);
                }
            }
        }
    }
}
