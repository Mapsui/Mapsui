// Copyright 2010 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Threading;
using BruTile;
using BruTile.Cache;
using SharpMap.Geometries;
using System.IO;

namespace SharpMap.Providers
{
    public class TileProvider : IProvider
    {
        #region Fields

        readonly ITileSource source;
        readonly MemoryCache<byte[]> bitmaps = new MemoryCache<byte[]>(100, 200);
        readonly List<TileIndex> queue = new List<TileIndex>();

        #endregion

        #region Properties

        public BoundingBox GetExtents()
        {
            return source.Schema.Extent.ToBoundingBox();
        }

        public int SRID { get; set; }

        #endregion

        public TileProvider(ITileSource tileSource)
        {
            source = tileSource;
        }

        public IFeatures FetchTiles(BoundingBox boundingBox, double resolution)
        {
            var extent = new Extent(boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Max.X, boundingBox.Max.Y);
            int level = BruTile.Utilities.GetNearestLevel(source.Schema.Resolutions, resolution);
            IList<TileInfo> tiles = source.Schema.GetTilesInView(extent, level);

            ICollection<WaitHandle> waitHandles = new List<WaitHandle>();
                        
            foreach (TileInfo info in tiles)    
            {
                if (bitmaps.Find(info.Index) != null) continue;
                if (queue.Contains(info.Index)) continue;
                var waitHandle = new AutoResetEvent(false);
                waitHandles.Add(waitHandle);
                queue.Add(info.Index);
                var thread = new Thread(GetTileOnThread);
                thread.Start(new object[] { source.Provider, info, bitmaps, waitHandle });
            }

            foreach (WaitHandle handle in waitHandles)
                handle.WaitOne();

            IFeatures features = new Features();
            foreach (TileInfo info in tiles)
            {
                byte[] bitmap = bitmaps.Find(info.Index);
                if (bitmap == null) continue;
                IRaster raster = new Raster(new MemoryStream(bitmap), new BoundingBox(info.Extent.MinX, info.Extent.MinY, info.Extent.MaxX, info.Extent.MaxY));
                IFeature feature = features.New();
                feature.Geometry = raster;
                features.Add(feature);
            }
            return features;
        }
        
        private void GetTileOnThread(object parameter)
        {
            var parameters = (object[])parameter;
            if (parameters.Length != 4) throw new ArgumentException("Four parameters expected");
            var tileProvider = (ITileProvider)parameters[0];
            var tileInfo = (TileInfo)parameters[1];
            var bitmap = (MemoryCache<byte[]>)parameters[2];
            var autoResetEvent = (AutoResetEvent)parameters[3];

            try
            {
                bitmap.Add(tileInfo.Index, tileProvider.GetTile(tileInfo));
            }
            catch (Exception ex)
            {
                //todo: log and use other ways to report to user.
            }
            finally
            {
                queue.Remove(tileInfo.Index);
                autoResetEvent.Set();
            }
        }

        #region IRasterProvider Members

        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            return FetchTiles(box, resolution);
        }

        #endregion

        #region IProvider Members

        public string ConnectionId
        {
            get { return String.Empty; }
        }

        public bool IsOpen
        {
            get { return true; }
        }
                
        public void Open()
        {
            //TODO: redesign so that methods like these are not necessary if not implemented
        }

        public void Close()
        {
            //TODO: redesign so that methods like these are not necessary if not implemented
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //nothing to dispose
        }

        #endregion
    }
}
