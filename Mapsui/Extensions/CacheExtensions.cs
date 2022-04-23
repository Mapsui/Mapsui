using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Mapsui.Cache;

namespace Mapsui.Extensions
{
    public static class CacheExtensions
    {
        public static async Task<Stream> UrlCachedStreamAsync(this IUrlPersistentCache? persistentCache, string url)
        {
            var bytes = persistentCache?.Find(url);
            if (bytes == null)
            {
                using var httpClient = new HttpClient();
                using var response = await httpClient.GetStreamAsync(url);
                bytes = response.ToBytes();
                persistentCache?.Add(url, bytes);
            }

            return new MemoryStream(bytes);
        }
    }
}
