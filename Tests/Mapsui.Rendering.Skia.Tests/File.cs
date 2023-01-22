using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Mapsui.Extensions.Cache;

namespace Mapsui.Rendering.Skia.Tests;

internal static class File
{
    private static readonly string ImagesFolder = Path.Combine(AssemblyDirectory, "Resources", "Images");
    private static readonly string ImagesOriginalTestFolder = Path.Combine(AssemblyDirectory, "Resources", "Images", "OriginalTest");
    private static readonly string ImagesGeneratedTestFolder = Path.Combine(AssemblyDirectory, "Resources", "Images", "GeneratedTest");
    private static readonly string ImagesOriginalRegressionFolder = Path.Combine(AssemblyDirectory, "Resources", "Images", "OriginalRegression");
    private static readonly string ImagesGeneratedRegressionFolder = Path.Combine(AssemblyDirectory, "Resources", "Images", "GeneratedRegression");
    private static readonly string CacheFolder = Path.Combine(AssemblyDirectory, "Resources", "Cache");

    static File()
    {
        Console.WriteLine($"Assembly Directory: {AssemblyDirectory}");
    }

    public static void WriteToGeneratedTestImagesFolder(string fileName, MemoryStream? stream)
    {
        WriteToGeneratedImagesFolder(ImagesGeneratedTestFolder, fileName, stream);
    }
    public static void WriteToGeneratedRegressionFolder(string fileName, MemoryStream? stream)
    {
        WriteToGeneratedImagesFolder(ImagesGeneratedRegressionFolder, fileName, stream);
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
            var path = Assembly.GetExecutingAssembly().Location;
            if (path == null)
                throw new Exception($"Assembly.GetExecutingAssembly().Location was null");

            return Path.GetDirectoryName(path)!;
        }
    }

    public static Stream ReadFromImagesFolder(string fileName)
    {
        var filePath = Path.Combine(ImagesFolder, fileName);
        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    public static Stream ReadFromOriginalFolder(string fileName)
    {
        var filePath = Path.Combine(ImagesOriginalTestFolder, fileName);
        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    public static Stream? ReadFromOriginalRegressionFolder(string fileName)
    {
        var filePath = Path.Combine(ImagesOriginalRegressionFolder, fileName);
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
