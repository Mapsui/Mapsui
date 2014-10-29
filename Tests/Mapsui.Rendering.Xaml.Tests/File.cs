using System;
using System.IO;

namespace Mapsui.Rendering.Xaml.Tests
{
    public static class File
    {
        private readonly static string OriginalImagesFolder = Path.Combine("Resources", "Images", "Original");
        private readonly static string GeneratedImagesFolder = Path.Combine("Resources", "Images", "Generated");

        public static void Write(string fileName, MemoryStream stream)
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

        public static MemoryStream Read(string fileName)
        {
            var filePath = Path.Combine(OriginalImagesFolder, fileName);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return Mapsui.Tests.Common.Utilities.ToMemoryStream(fileStream);
        }
    }
}
