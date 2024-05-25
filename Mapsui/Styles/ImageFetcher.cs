using Mapsui.Extensions;
using Mapsui.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Mapsui.Styles;
public static class ImageFetcher
{
    public static async Task<byte[]> FetchBytesFromImageSourceAsync(string imageSource)
    {
        var imageSourceUrl = new Uri(imageSource);

        return imageSourceUrl.Scheme switch
        {
            "embedded" => LoadEmbeddedResourceFromPath(imageSourceUrl),
            "file" => LoadFromFileSystem(imageSourceUrl),
            "http" or "https" => await LoadFromUrlAsync(imageSourceUrl),
            _ => throw new ArgumentException($"Scheme is not supported '{imageSourceUrl.Scheme}' of '{imageSource}'"),
        };
    }

    private static byte[] LoadEmbeddedResourceFromPath(Uri imageSource)
    {
        try
        {
            var assemblies = GetMatchingAssemblies(imageSource);

            foreach (var assembly in assemblies)
            {
                string[] resourceNames = assembly.GetManifestResourceNames();
                var matchingResourceName = resourceNames.FirstOrDefault(r => r.Equals(imageSource.Host, StringComparison.InvariantCultureIgnoreCase));
                if (matchingResourceName != null)
                {
                    using var stream = assembly.GetManifestResourceStream(matchingResourceName)
                        ?? throw new Exception($"The resource name was found but GetManifestResourceStream returned null: '{imageSource}'");
                    return stream.ToBytes();
                }
            }
            var allResourceNames = assemblies.SelectMany(a => a.GetManifestResourceNames()).ToList();
            string listOfEmbeddedResources = string.Concat(allResourceNames.Select(n => '\n' + n)); // All resources should be on a new line.
            throw new Exception($"Could not find the embedded resource in the current assemblies. ImageSource: '{imageSource}'. Other embedded resources in matching assemblies: {listOfEmbeddedResources}");
        }
        catch (Exception ex)
        {
            var message = $"Could not load embedded resource '{imageSource}' : '{ex.Message}'";
            Logger.Log(LogLevel.Error, message, ex);
            throw new Exception(message, ex);
        }
    }

    static private List<Assembly> GetMatchingAssemblies(Uri imageSource)
    {
        var result = new List<Assembly>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = assembly.GetName().Name ?? throw new Exception($"Assembly name is null: '{assembly}'");
            if (imageSource.Host.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
            {
                result.Add(assembly);
            }
        }
        if (!result.Any())
            throw new Exception($"No matching assemblies found for url: '{imageSource}");

        return result;
    }

    private async static Task<byte[]> LoadFromUrlAsync(Uri imageSource)
    {
        try
        {
            using HttpClientHandler handler = new HttpClientHandler { AllowAutoRedirect = true };
            using var httpClient = new HttpClient(handler);
            using HttpResponseMessage response = await httpClient.GetAsync(imageSource, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is unsuccessful
            await using var tempStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return tempStream.ToBytes();
        }
        catch (Exception ex)
        {
            var message = $"Could not load resource from url '{imageSource}' : '{ex.Message}'";
            Logger.Log(LogLevel.Error, message, ex);
            throw new Exception(message, ex);
        }
    }

    private static byte[] LoadFromFileSystem(Uri imageSource)
    {
        try
        {
            return File.ReadAllBytes(imageSource.LocalPath);
        }
        catch (Exception ex)
        {
            var message = $"Could not load resource from file '{imageSource}' : '{ex.Message}'";
            Logger.Log(LogLevel.Error, message, ex);
            throw new Exception(message, ex);
        }
    }
}
