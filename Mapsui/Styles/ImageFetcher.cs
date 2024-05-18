using System.IO;
using System.Threading.Tasks;
using System;
using Mapsui.Logging;
using System.Net.Http;
using Mapsui.Extensions;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Mapsui.Styles;
public static class ImageFetcher
{
    public static async Task<Stream> FetchStreamFromImagePathAsync(Uri imagePath)
    {
        var stream = imagePath.Scheme switch
        {
            "embeddedresource" => LoadEmbeddedResourceFromPath(imagePath),
            "file" => LoadFromFileSystem(imagePath),
            "http" or "https" => await LoadFromUrlAsync(imagePath),
            _ => throw new ArgumentException($"Scheme is not supported '{imagePath.Scheme}' of '{imagePath}'"),
        };
        ValidateBitmapData(stream);
        return stream;
    }

    private static Stream LoadEmbeddedResourceFromPath(Uri imagePath)
    {
        try
        {
            var assemblies = GetMatchingAssemblies(imagePath);

            foreach (var assembly in assemblies)
            {
                string[] resourceNames = assembly.GetManifestResourceNames();
                var matchingResourceName = resourceNames.FirstOrDefault(r => r.Equals(imagePath.Host, StringComparison.InvariantCultureIgnoreCase));
                if (matchingResourceName != null)
                {
                    using var stream = assembly.GetManifestResourceStream(matchingResourceName)
                        ?? throw new Exception($"The resource name was found but GetManifestResourceStream returned null: '{imagePath}'");
                    return stream.CopyToMemoryStream(); // Copy stream to memory stream to avoid issues with disposing the stream
                }
            }
            var allResourceNames = assemblies.SelectMany(a => a.GetManifestResourceNames()).ToList();
            string listOfEmbeddedResources = string.Concat(allResourceNames.Select(n => '\n' + n)); // All resources should be on a new line.
            throw new Exception($"Could not find the embedded resource in the current assemblies. BitmapPath: '{imagePath}'. Other embedded resources in matching assemblies: {listOfEmbeddedResources}");
        }
        catch (Exception ex)
        {
            var message = $"Could not load embedded resource '{imagePath}' : '{ex.Message}'";
            Logger.Log(LogLevel.Error, message, ex);
            throw new Exception(message, ex);
        }
    }

    static private IEnumerable<Assembly> GetMatchingAssemblies(Uri imagePath)
    {
        var result = new List<Assembly>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = assembly.GetName().Name;
            if (name is null)
                throw new Exception($"Assembly name is null: '{assembly}'");
            if (imagePath.Host.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
            {
                result.Add(assembly);
            }
        }
        if (!result.Any())
            throw new Exception($"No matching assemblies found for url: '{imagePath}");

        return result;
    }

    private async static Task<Stream> LoadFromUrlAsync(Uri bitmapPath)
    {
        try
        {
            using HttpClientHandler handler = new HttpClientHandler { AllowAutoRedirect = true };
            using var httpClient = new HttpClient(handler);
            using HttpResponseMessage response = await httpClient.GetAsync(bitmapPath, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is unsuccessful
            await using var tempStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return tempStream.CopyToMemoryStream(); // Copy stream to memory stream to avoid issues with disposing the stream
        }
        catch (Exception ex)
        {
            var message = $"Could not load resource from url '{bitmapPath}' : '{ex.Message}'";
            Logger.Log(LogLevel.Error, message, ex);
            throw new Exception(message, ex);
        }
    }

    private static Stream LoadFromFileSystem(Uri bitmapPath)
    {
        try
        {
            return File.OpenRead(bitmapPath.LocalPath);
        }
        catch (Exception ex)
        {
            var message = $"Could not load resource from file '{bitmapPath}' : '{ex.Message}'";
            Logger.Log(LogLevel.Error, message, ex);
            throw new Exception(message, ex);
        }
    }

    /// <summary>
    /// Validate the correctness of the bitmap. Throws if not valid.
    /// </summary>
    /// <param name="bitmapData">Bitmap data to check</param>
    private static void ValidateBitmapData(Stream bitmapData)
    {
        if (bitmapData == null)
            throw new ArgumentException("The bitmap data that is registered is null. Was the image loaded correctly?");
    }
}
