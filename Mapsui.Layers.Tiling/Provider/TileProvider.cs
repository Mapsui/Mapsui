// Copyright 2010 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// Mapsui is free software; you can redistribute it and/or modify
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BruTile;
using BruTile.Cache;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;

namespace Mapsui.Providers
{
    public class TileProvider : IProvider<IFeature>
    {
        private readonly ITileSource _source;
        private readonly MemoryCache<byte[]> _bitmaps = new(100, 200);
        private readonly List<TileIndex> _queue = new();

        public MRect? GetExtent()
        {
            return _source.Schema.Extent.ToMRect();
        }

        public string? CRS { get; set; }

        public TileProvider(ITileSource tileSource)
        {
            _source = tileSource;
        }

        public IEnumerable<IFeature> FetchTiles(FetchInfo fetchInfo)
        {
            var box = fetchInfo.Extent;
            var extent = new Extent(box.Min.X, box.Min.Y, box.Max.X, box.Max.Y);
            var levelId = BruTile.Utilities.GetNearestLevel(_source.Schema.Resolutions, fetchInfo.Resolution);
            var infos = _source.Schema.GetTileInfos(extent, levelId).ToList();

            ICollection<WaitHandle> waitHandles = new List<WaitHandle>();

            foreach (var info in infos)
            {
                if (_bitmaps.Find(info.Index) != null) continue;
                if (_queue.Contains(info.Index)) continue;
                var waitHandle = new AutoResetEvent(false);
                waitHandles.Add(waitHandle);
                _queue.Add(info.Index);
                Task.Run(() => GetTileOnThread(new object[] { _source, info, _bitmaps, waitHandle }));
            }

            WaitHandle.WaitAll(waitHandles.ToArray());

            var features = new List<IFeature>();
            foreach (var info in infos)
            {
                var bitmap = _bitmaps.Find(info.Index);
                if (bitmap == null) continue;
                var raster = new MRaster(new MemoryStream(bitmap), new MRect(info.Extent.MinX, info.Extent.MinY, info.Extent.MaxX, info.Extent.MaxY));
                features.Add(new RasterFeature(raster));
            }
            return features;
        }

        private void GetTileOnThread(object parameter) // This could accept normal parameters now we use PCL Profile111
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
                Logger.Log(LogLevel.Error, ex.Message, ex);
                // todo: report back through callback
            }
            finally
            {
                _queue.Remove(tileInfo.Index);
                autoResetEvent.Set();
            }
        }

        public IEnumerable<IFeature> GetFeatures(FetchInfo fetchInfo)
        {
            return FetchTiles(fetchInfo);
        }
    }
}
