using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.Extensions
{
    public static class AsyncProviderExtensions
    {
        public static async Task<List<T>> GetFeaturesAsync<T>(this IProviderBase? provider, FetchInfo fetchInfo)
            where T : IFeature
        {
            if (provider is IAsyncProvider<T> asyncProvider)
            {
                return await asyncProvider.GetFeaturesAsync(fetchInfo).ToListAsync();
            }

            return provider.GetFeatures<T>(fetchInfo).ToList();
        }

        public static List<T> GetFeatures<T>(this IProviderBase? provider, FetchInfo fetchInfo)
            where T : IFeature
        {
            if (provider is IProvider<T> syncProvider)
                return syncProvider.GetFeatures(fetchInfo).ToList() ?? new List<T>();

            throw new InvalidOperationException();
        }
    }
}
