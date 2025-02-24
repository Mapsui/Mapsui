﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Mapsui.Cache;
using Mapsui.Logging;

namespace Mapsui.Extensions;

public static class CacheExtensions
{
    public static async Task<byte[]> GetCachedBytesAsync(this IUrlPersistentCache? persistentCache, string url, Func<string, Task<byte[]>>? getBytesAsync = null)
    {
        var bytes = persistentCache?.Find(url);
        if (bytes == null)
        {
            Logger.Log(LogLevel.Debug, $@"Load Url {url}");

            if (getBytesAsync != null)
            {
                bytes = await getBytesAsync(url).ConfigureAwait(false);
            }
            else
            {
                using var handler = new HttpClientHandler();
                using var httpClient = new HttpClient(handler);
                // https://github.com/xamarin/xamarin-android/issues/5264 use ConfigureAwait(false) for Network access
                await using var response = await httpClient.GetStreamAsync(url).ConfigureAwait(false);
                bytes = response.ToBytes();
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
