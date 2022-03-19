using System;
using System.IO;
using System.Reflection;
using Mapsui.Extensions.Cache;

namespace Mapsui.Rendering.Skia.Tests
{
    internal static class File
    {
        private static readonly string OriginalTestImagesFolder = Path.Combine(AssemblyDirectory, "Resources", "Images", "OriginalTest");
        private static readonly string GeneratedTestImagesFolder = Path.Combine(AssemblyDirectory, "Resources", "Images", "GeneratedTest");
        private static readonly string OriginalRegressionImagesFolder = Path.Combine(AssemblyDirectory, "Resources", "Images", "OriginalRegression");
        private static readonly string GeneratedRegressionImagesFolder = Path.Combine(AssemblyDirectory, "Resources", "Images", "GeneratedRegression");
        private static readonly string CacheFolder = Path.Combine(AssemblyDirectory, "Resources", "Cache");

        static File()
        {
            Console.WriteLine($"Assembly Directory: {AssemblyDirectory}");
        }

        public static void WriteToGeneratedTestImagesFolder(string fileName, MemoryStream? stream)
        {
            WriteToGeneratedImagesFolder(GeneratedTestImagesFolder, fileName, stream);
        }
        public static void WriteToGeneratedRegressionFolder(string fileName, MemoryStream? stream)
        {
            WriteToGeneratedImagesFolder(GeneratedRegressionImagesFolder, fileName, stream);
        }

        private static void WriteToGeneratedImagesFolder(string folderName, string fileName, MemoryStream? stream)
        {
            var filePath = Path.Combine(folderName, fileName);
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

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                if (codeBase == null)
                    throw new Exception($"Assembly.GetExecutingAssembly().CodeBase was null") ;

                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path)!;
            }
        }

        public static Stream ReadFromOriginalFolder(string fileName)
        {
            var filePath = Path.Combine(OriginalTestImagesFolder, fileName);
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        public static Stream? ReadFromRegressionFolder(string fileName)
        {
            var filePath = Path.Combine(OriginalRegressionImagesFolder, fileName);
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
