using System;
using BruTile;
using BruTile.Cache;
using SharpMap;
using SharpMap.Geometries;
using SilverlightRendering;

namespace SharpMapProvider
{
    public class SharpMapTileProvider : ITileProvider
    {
        #region fields

        Map map;
        object syncRoot = new object();
        ITileCache<byte[]> fileCache;

        #endregion

        #region Public Methods

        public SharpMapTileProvider(Map map)
            : this(map, new NullCache())
        {
        }

        public SharpMapTileProvider(Map map, ITileCache<byte[]> fileCache)
        {
            this.map = map;

            if (fileCache == null) throw new ArgumentException("File can not be null");

            this.fileCache = fileCache;
        }

        #endregion

        #region Private Methods

        public byte[] GetTile(TileInfo tileInfo)
        {
            byte[] bytes = null;
            bytes = fileCache.Find(tileInfo.Index);
            if (bytes == null)
            {
                lock (syncRoot)
                {
                    var renderer = new MapRenderer();
                    IView view = new View { Width = 256, Height = 256, Resolution = (tileInfo.Extent.Width / 256), Center = new Point(tileInfo.Extent.CenterX, tileInfo.Extent.CenterY) };
                    renderer.Render(view, map);
                    var stream = renderer.ToBitmapStream(256, 256);
                    stream.Position = 0;
                    bytes = Utilities.ReadFully(stream);
                    if (bytes != null)
                        fileCache.Add(tileInfo.Index, bytes);
                }
            }
            return bytes;
        }

        #endregion

        #region Private classes

        private class NullCache : ITileCache<byte[]>
        {
            public NullCache()
            {
            }

            public void Add(TileIndex index, byte[] image)
            {
                //do nothing
            }

            public void Remove(TileIndex index)
            {
                throw new NotImplementedException(); //and should not
            }

            public byte[] Find(TileIndex index)
            {
                return null;
            }
        }

        #endregion
    }
}
