using System;
using System.IO;

namespace Mapsui.Rendering.Xaml.Tests
{
    public static class File
    {
        private static readonly string OriginalImagesFolder = Path.Combine("Resources", "Images", "Original");
        private static readonly string GeneratedImagesFolder = Path.Combine("Resources", "Images", "Generated");

        public static void WriteToGeneratedFolder(string fileName, MemoryStream stream)
        {
            var filePath = Path.Combine(GeneratedImagesFolder, fileName);
            var folder = Path.GetDirectoryName(filePath);
            if (folder == null) throw new Exception("Images folder was not found");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                stream.WriteTo(fileStream);
            }
        }

        public static Stream ReadFromOriginalFolder(string fileName)
        {
            var filePath = Path.Combine(OriginalImagesFolder, fileName);
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
    }
}
