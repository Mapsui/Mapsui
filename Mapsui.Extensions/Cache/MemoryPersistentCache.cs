using System.Collections.Generic;
using BruTile;
using BruTile.Cache;
using Mapsui.Cache;

namespace Mapsui.Extensions.Cache
{
    public class MemoryPersistentCache: IPersistentCache<byte[]>, IUrlPersistentCache
    {
        private Dictionary<TileIndex, byte[]> tileCache = new();
        private Dictionary<string, byte[]> urlCache = new();

        public void Add(TileIndex index, byte[] tile)
        {
            tileCache[index] = tile;
        }

        public void Remove(TileIndex index)
        {
            tileCache.Remove(index);
        }

        public byte[] Find(TileIndex index)
        {
            return tileCache[index];
        }

        public void Add(string url, byte[] tile)
        {
            urlCache[url] = tile;
        }

        public void Remove(string url)
        {
            urlCache.Remove(url);
        }

        public byte[]? Find(string url)
        {
            return urlCache[url];
        }
    }
}