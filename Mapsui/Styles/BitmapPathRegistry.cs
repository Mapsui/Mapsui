using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Logging;

namespace Mapsui.Styles;

/// <summary>
/// Class for managing all bitmaps, which are registered for Mapsui drawing
/// </summary>
public sealed class BitmapPathRegistry : IDisposable
{
    private static BitmapPathRegistry? _instance;
    private readonly ConcurrentDictionary<string, Stream> _register = [];

    private BitmapPathRegistry() { }

    /// <summary>
    /// Singleton of BitmapRegistry class
    /// </summary>
    public static BitmapPathRegistry Instance => _instance ??= new BitmapPathRegistry();

    /// <inheritdoc />
    public async Task RegisterAsync(Uri bitmapPath)
    {
        var key = bitmapPath.ToString();
        if (_register.ContainsKey(key))
        {
            Logger.Log(LogLevel.Warning, $"Bitmap already registered: '{key}'");
        }

        var stream = bitmapPath.Scheme switch
        {
            "embeddedresource" => LoadEmbeddedResourceFromPath(bitmapPath),
            "file" => LoadFromFileSystem(bitmapPath),
            "http" or "https" => await LoadFromUrlAsync(bitmapPath),
            _ => throw new ArgumentException($"Scheme is not supported '{bitmapPath.Scheme}' of '{bitmapPath}'"),
        };
        ValidateBitmapData(stream);
        _register[bitmapPath.ToString()] = stream;
    }

    /// <inheritdoc />
    public Stream Get(Uri key)
    {
        return _register[key.ToString()];
    }

    /// <inheritdoc />
    public Stream? Unregister(Uri key)
    {
        _register.TryRemove(key.ToString(), out var val);
        return val;
    }

    public void Dispose()
    {
        foreach (var it in _register)
        {
            if (it.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        _register.Clear();
    }

    private static Stream LoadEmbeddedResourceFromPath(Uri bitmapPath)
    {
        try
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = assembly.GetName().Name;
                if (name is null)
                    throw new Exception($"Assembly name is null: '{assembly}'");
                if (bitmapPath.Host.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] resourceNames = assembly.GetManifestResourceNames();
                    var realResourceName = resourceNames.FirstOrDefault(r => r.Equals(bitmapPath.Host, StringComparison.InvariantCultureIgnoreCase));
                    if (realResourceName != null)
                    {
                        using var stream = assembly.GetManifestResourceStream(realResourceName);
                        if (stream is null)
                            throw new Exception($"The resource name was found but GetManifestResourceStream returned null: '{bitmapPath}'");
                        return stream.CopyToMemoryStream(); // Copy stream to memory stream to avoid issues with disposing the stream
                    }
                }
            }

            throw new Exception($"Could not find the embedded resource in the CurrentDomain.GetAssemblies(): '{bitmapPath}'");
        }
        catch (Exception ex)
        {
            var message = $"Could not load embedded resource '{bitmapPath}' : '{ex.Message}'";
            Logger.Log(LogLevel.Error, message, ex);
            throw new Exception(message, ex);
        }
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
    private void ValidateBitmapData(Stream bitmapData)
    {
        if (bitmapData == null)
            throw new ArgumentException("The bitmap data that is registered is null. Was the image loaded correctly?");
    }
}
