using System;
using System.IO;
using System.Reflection;
using Mapsui.Samples.Common.Desktop.Cache;
using Mapsui.Tiling.Cache;

namespace Mapsui.Rendering.Skia.Tests
{
    internal static class File
    {
        private static readonly string RegressionImagesFolder = Path.Combine(AssemblyDirectory!, "Resources", "Images", "Regression");
        private static readonly string CacheFolder = Path.Combine(AssemblyDirectory!, "Resources", "Cache");
        private static readonly string OriginalImagesFolder = Path.Combine(AssemblyDirectory!, "Resources", "Images", "Original");
        private static readonly string GeneratedImagesFolder = Path.Combine(AssemblyDirectory!, "Resources", "Images", "Generated");

        static File()
        {
            Console.WriteLine($"Assembly Directory: {AssemblyDirectory}");
        }

        public static void WriteToGeneratedFolder(string fileName, MemoryStream? stream)
        {
            var filePath = Path.Combine(GeneratedImagesFolder, fileName);
            var folder = Path.GetDirectoryName(filePath);
            if (folder == null) throw new Exception("Images folder was not found");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            stream?.WriteTo(fileStream);
        }

        public static string? AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                if (codeBase == null)
                    return null;

                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static Stream ReadFromOriginalFolder(string fileName)
        {
            var filePath = Path.Combine(OriginalImagesFolder, fileName);
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        public static Stream? ReadFromRegressionFolder(string fileName)
        {
            var filePath = Path.Combine(RegressionImagesFolder, fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return null;
            }
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        public static SqlitePersistentCache ReadFromCacheFolder(string fileName)
        {
            var filePath = Path.Combine(CacheFolder, fileName);
            return new SqlitePersistentCache(filePath, folder: CacheFolder);
        }
    }
}
