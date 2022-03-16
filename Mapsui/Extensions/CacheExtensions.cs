using System.IO;
using System.Net.Http;
using Mapsui.Cache;

namespace Mapsui.Extensions
{
    public static class CacheExtensions
    {
        public static Stream UrlCachedStream(this IUrlPersistentCache? persistentCache, string url)
        {
            var bytes = persistentCache?.Find(url);
            if (bytes == null)
            {
                using var httpClient = new HttpClient();
                using var response = httpClient.GetStreamAsync(url).Result;
                bytes = response.ToBytes();
                persistentCache?.Add(url, bytes);
            }

            return new MemoryStream(bytes);
        }
    }
}
