using Mapsui.Extensions;
using Mapsui.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mapsui.Styles;
public static class ImageFetcher
{
    private static ConcurrentDictionary<string, Assembly>? _embeddedResources;

    public static async Task<byte[]> FetchBytesFromImageSourceAsync(string imageSource)
    {
        // Uri has a limitation of ~2000 bytes for URLs, so extract the scheme by hand
        var scheme = imageSource.Substring(0, imageSource.IndexOf(':'));

        return scheme switch
        {
            "embedded" => LoadEmbeddedResourceFromPath(new Uri(imageSource)),
            "file" => LoadFromFileSystem(new Uri(imageSource)),
            "http" or "https" => await LoadFromUrlAsync(new Uri(imageSource)),
            "svg-content" => LoadFromSvg(imageSource["svg-content://".Length..]),
            "base64-content" => LoadFromBase64(imageSource["base64-content://".Length..]),
            _ => throw new ArgumentException($"Scheme '{scheme}' of '{imageSource}' is not supported"),
        };
    }

    private static byte[] LoadEmbeddedResourceFromPath(Uri imageSource)
    {
        try
        {
            if (_embeddedResources is null)
                _embeddedResources = LoadEmbeddedResourcePaths();

            if (_embeddedResources.TryGetValue(imageSource.Host, out var assembly))
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
            var allResourceNames = _embeddedResources.Keys.ToList();
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

    private static ConcurrentDictionary<string, Assembly> LoadEmbeddedResourcePaths()
    {
        var result = new ConcurrentDictionary<string, Assembly>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (var resourceName in resourceNames)
            {
                if (!resourceName.EndsWith(".resources"))
                    result.AddOrUpdate(resourceName.ToLower(), (r) => assembly, (r, a) => assembly);
            }
        }
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

    private static byte[] LoadFromSvg(string imageSource)
    {
        try
        {
            return Encoding.UTF8.GetBytes(imageSource);
        }
        catch (Exception ex)
        {
            var message = $"Could not load svg from string '{imageSource}' : '{ex.Message}'";
            Logger.Log(LogLevel.Error, message, ex);
            throw new Exception(message, ex);
        }
    }

    private static byte[] LoadFromBase64(string imageSource)
    {
        try
        {
            return Convert.FromBase64String(imageSource);
        }
        catch (Exception ex)
        {
            var message = $"Could not load resource from base64 encoded string '{imageSource}' : '{ex.Message}'";
            Logger.Log(LogLevel.Error, message, ex);
            throw new Exception(message, ex);
        }
    }
}
