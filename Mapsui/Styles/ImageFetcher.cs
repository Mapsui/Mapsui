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
    public const string EmbeddedScheme = "embedded";
    public const string FileScheme = "file";
    public const string HttpScheme = "http";
    public const string HttpsScheme = "https";
    public const string SvgContentScheme = "svg-content";
    public const string Base64ContentScheme = "base64-content";

    private static ConcurrentDictionary<string, Assembly>? _embeddedResources;

    public static async Task<byte[]> FetchBytesFromImageSourceAsync(string imageSource)
    {
        // Uri has a limitation of ~2000 bytes for URLs, so extract the scheme by hand
        var scheme = imageSource.Substring(0, imageSource.IndexOf(':'));

        return scheme switch
        {
            EmbeddedScheme => LoadEmbeddedResourceFromPath(new Uri(imageSource)),
            FileScheme => LoadFromFileSystem(new Uri(imageSource)),
            HttpScheme or HttpsScheme => await LoadFromUrlAsync(new Uri(imageSource)),
            SvgContentScheme => LoadFromSvg(imageSource[(SvgContentScheme.Length + 3)..]),
            Base64ContentScheme => LoadFromBase64(imageSource[(Base64ContentScheme.Length + 3)..]),
            _ => throw new ArgumentException($"Scheme '{scheme}' of '{imageSource}' is not supported"),
        };
    }

    private static byte[] LoadEmbeddedResourceFromPath(Uri imageSource)
    {
        try
        {
            // Known limitation: the index is built once, from the assemblies loaded at the first embedded fetch.
            // Resources in assemblies loaded later are not found. Concurrent first fetches may also build the
            // index more than once, which is harmless but wasteful.
            if (_embeddedResources is null)
                _embeddedResources = LoadEmbeddedResourcePaths();

            if (_embeddedResources.TryGetValue(imageSource.Host, out var assembly))
            {
                // Known limitation: the dictionary lookup already matched the name, but we scan the assembly again
                // to get the original casing for GetManifestResourceStream. Storing the original name as part of the
                // dictionary value would avoid this.
                string[] resourceNames = assembly.GetManifestResourceNames();
                var matchingResourceName = resourceNames.FirstOrDefault(r => r.Equals(imageSource.Host, StringComparison.OrdinalIgnoreCase));
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

    internal static ConcurrentDictionary<string, Assembly> LoadEmbeddedResourcePaths()
    {
        // Uri.Host is always lowercased, so the lookup must ignore case. Use an ordinal comparer rather than
        // lowercasing the keys: a culture-sensitive ToLower() turns 'I' into a dotless 'ı' under Turkish
        // culture, so the keys would never match. See https://github.com/Mapsui/Mapsui/issues/3383
        var result = new ConcurrentDictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic);

        foreach (var assembly in assemblies)
        {
            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (var resourceName in resourceNames)
            {
                if (!resourceName.EndsWith(".resources", StringComparison.Ordinal))
                    result.AddOrUpdate(resourceName, (r) => assembly, (r, a) => assembly);
            }
        }
        return result;
    }

    private static Task<byte[]> LoadFromUrlAsync(Uri imageSource)
    {
        return LoadFromUrlAsync(imageSource, new HttpClientHandler { AllowAutoRedirect = true });
    }

    internal async static Task<byte[]> LoadFromUrlAsync(Uri imageSource, HttpMessageHandler handler)
    {
        try
        {
            using var httpClient = new HttpClient(handler);
            using HttpResponseMessage response = await httpClient.GetAsync(imageSource).ConfigureAwait(false);
            response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is unsuccessful
            // Read asynchronously. In Blazor WebAssembly the response stream is backed by the browser's Fetch API
            // and throws on synchronous reads. See https://github.com/Mapsui/Mapsui/issues/3381
            return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
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
