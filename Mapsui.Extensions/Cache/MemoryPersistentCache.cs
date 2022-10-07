using System.Collections.Concurrent;
using System.Collections.Generic;
using BruTile;
using BruTile.Cache;
using Mapsui.Cache;

namespace Mapsui.Extensions.Cache
{
    public class MemoryPersistentCache: IPersistentCache<byte[]>, IUrlPersistentCache
    {
        private ConcurrentDictionary<TileIndex, byte[]> tileCache = new();
        private ConcurrentDictionary<string, byte[]> urlCache = new();

        public void Add(TileIndex index, byte[] tile)
        {
            tileCache[index] = tile;
        }

        public void Remove(TileIndex index)
        {
            tileCache.TryRemove(index, out _);
        }

        public byte[] Find(TileIndex index)
        {
            if (tileCache.TryGetValue(index, out var result))
                return result;

            return null;
        }

        public void Add(string url, byte[] tile)
        {
            urlCache[url] = tile;
        }

        public void Remove(string url)
        {
            urlCache.TryRemove(url, out _);
        }

        public byte[]? Find(string url)
        {
            if (urlCache.TryGetValue(url, out var result))            
                return result;            

            return null;
        }
    }
}