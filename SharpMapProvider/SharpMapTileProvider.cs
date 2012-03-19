using System;
using BruTile;
using BruTile.Cache;
using SharpMap;
using SharpMap.Geometries;
using SilverlightRendering;
using Utilities = BruTile.Utilities;

namespace SharpMapProvider
{
    public class SharpMapTileProvider : ITileProvider
    {
        readonly Map map;
        readonly object syncRoot = new object();
        ITileCache<byte[]> tileCache;

        public SharpMapTileProvider(Map map)
        {
            this.map = map;
            this.map.DataChanged += MapDataChanged;
            tileCache = new MemoryCache<byte[]>(200, 300);

            if (tileCache == null) throw new ArgumentException("File can not be null");
        }

        void MapDataChanged(object sender, SharpMap.Fetcher.DataChangedEventArgs e)
        {
            tileCache = new MemoryCache<byte[]>(200, 300); // the crude way to refreshing
        }

        public byte[] GetTile(TileInfo tileInfo)
        {
            byte[] bytes = tileCache.Find(tileInfo.Index);
            if (bytes == null)
            {
                lock (syncRoot)
                {
                    var renderer = new MapRenderer();
                    IView view = new View { Width = 256, Height = 256, Resolution = (tileInfo.Extent.Width / 256), Center = new Point(tileInfo.Extent.CenterX, tileInfo.Extent.CenterY) };
                    renderer.Render(view, map.Layers);
                    var stream = renderer.ToBitmapStream(256, 256);
                    stream.Position = 0;
                    bytes = Utilities.ReadFully(stream);
                    if (bytes != null)
                        tileCache.Add(tileInfo.Index, bytes);
                }
            }
            return bytes;
        }
    }
}
