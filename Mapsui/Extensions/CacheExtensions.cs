using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Mapsui.Cache;
using Mapsui.Logging;

namespace Mapsui.Extensions;

public static class CacheExtensions
{
    public static async Task<Stream> UrlCachedStreamAsync(this IUrlPersistentCache? persistentCache, string url, Func<string, Task<Stream>>? loadUrl = null)
    {
        var bytes = await UrlCachedArrayAsync(persistentCache, url, loadUrl);

        return new MemoryStream(bytes);
    }

    public static async Task<byte[]> UrlCachedArrayAsync(this IUrlPersistentCache? persistentCache, string url, Func<string, Task<Stream>>? loadUrl = null)
    {
        var bytes = persistentCache?.Find(url);
        if (bytes == null)
        {
            Logger.Log(LogLevel.Debug, $@"Load Url {url}");
            Stream? response = null;
            try
            {
#pragma warning disable IDISP001 // Dispose created                    
                if (loadUrl != null)
                {
                    response = await loadUrl(url);
                }
                else
                {
                    var handler = new HttpClientHandler();
                    using var httpClient = new HttpClient(handler);
                    // https://github.com/xamarin/xamarin-android/issues/5264 use ConfigureAwait(false) for Network access
                    response = await httpClient.GetStreamAsync(url).ConfigureAwait(false);
                }
#pragma warning restore IDISP001

                bytes = response.ToBytes();
            }
            finally
            {
                if (response != null)
                {
#if NETSTANDARD2_0
                    response.Dispose();
#else                        
                    await response.DisposeAsync();
#endif    
                }
            }

            Logger.Log(LogLevel.Debug, $@"Caching Url {url}");
            persistentCache?.Add(url, bytes);
        }
        else
        {
            Logger.Log(LogLevel.Debug, $@"Cached Load Url {url}");
        }

        return bytes;
    }
}
