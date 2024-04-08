using System;
using System.IO;
using Mapsui.Extensions.Cache;

namespace Mapsui.Rendering.Skia.Tests;

internal static class File
{
    private static readonly string _imagesFolder = Path.Combine(AssemblyDirectory, "Resources", "Images");
    private static readonly string _imagesOriginalTestFolder = Path.Combine(AssemblyDirectory, "Resources", "Images", "OriginalTest");
    private static readonly string _imagesGeneratedTestFolder = Path.Combine(AssemblyDirectory, "Resources", "Images", "GeneratedTest");
    private static readonly string _imagesOriginalRegressionFolder = Path.Combine(AssemblyDirectory, "Resources", "Images", "OriginalRegression");
    private static readonly string _imagesGeneratedRegressionFolder = Path.Combine(AssemblyDirectory, "Resources", "Images", "GeneratedRegression");
    private static readonly string _cacheFolder = Path.Combine(AssemblyDirectory, "Resources", "Cache");

    static File()
    {
        Console.WriteLine($"Assembly Directory: {AssemblyDirectory}");
    }

    public static void WriteToGeneratedTestImagesFolder(string fileName, MemoryStream? stream)
    {
        WriteToGeneratedImagesFolder(_imagesGeneratedTestFolder, fileName, stream);
    }
    public static void WriteToGeneratedRegressionFolder(string fileName, MemoryStream? stream)
    {
        WriteToGeneratedImagesFolder(_imagesGeneratedRegressionFolder, fileName, stream);
    }

    private static void WriteToGeneratedImagesFolder(string folderName, string fileName, MemoryStream? stream)
    {
        var filePath = Path.Combine(folderName, fileName);
        var folder = Path.GetDirectoryName(filePath) ?? throw new Exception("Images folder was not found");
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
            var path = System.AppContext.BaseDirectory
                ?? throw new Exception($"Assembly.GetExecutingAssembly().Location was null");
            return Path.GetDirectoryName(path)!;
        }
    }

    public static Stream ReadFromImagesFolder(string fileName)
    {
        var filePath = Path.Combine(_imagesFolder, fileName);
        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    public static Stream ReadFromOriginalFolder(string fileName)
    {
        var filePath = Path.Combine(_imagesOriginalTestFolder, fileName);
        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    public static Stream? ReadFromOriginalRegressionFolder(string fileName)
    {
        var filePath = Path.Combine(_imagesOriginalRegressionFolder, fileName);
        if (!System.IO.File.Exists(filePath))
        {
            return null;
        }
        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    public static SqlitePersistentCache ReadFromCacheFolder(string fileName)
    {
        var filePath = Path.Combine(_cacheFolder, fileName);
        return new SqlitePersistentCache(filePath, folder: _cacheFolder);
    }
}
