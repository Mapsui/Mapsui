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
using BruTile;

namespace Mapsui.Fetcher
{
    [Obsolete("Use TileFetchDispatcher and FetchMachine")]
    class FetchOnThread
        //This class is needed because in CF one can only pass arguments to a thread using a class constructor.
        //Once support for CF is dropped (replaced by SL on WinMo?) this class should be removed.
    {
        readonly ITileProvider _tileProvider;
        readonly TileInfo _tileInfo;
        readonly FetchTileCompletedEventHandler _fetchTileCompleted;

        public FetchOnThread(ITileProvider tileProvider, TileInfo tileInfo, FetchTileCompletedEventHandler fetchTileCompleted)
        {
            _tileProvider = tileProvider;
            _tileInfo = tileInfo;
            _fetchTileCompleted = fetchTileCompleted;
        }

        public void FetchTile()
        {
            Exception error = null;
            byte[] image = null;

            try
            {
                if (_tileProvider != null) image = _tileProvider.GetTile(_tileInfo);
            }
            catch (Exception ex) // On this worker thread exceptions will not fall through to the caller so catch and pass in callback  
            {
                error = ex;
            }
            _fetchTileCompleted(this, new FetchTileCompletedEventArgs(error, false, _tileInfo, image));
        }
    }

    public delegate void FetchTileCompletedEventHandler(object sender, FetchTileCompletedEventArgs e);

    public class FetchTileCompletedEventArgs
    {
        public FetchTileCompletedEventArgs(Exception error, bool cancelled, TileInfo tileInfo, byte[] image)
        {
            Error = error;
            Cancelled = cancelled;
            TileInfo = tileInfo;
            Image = image;
        }

        public Exception Error;
        public readonly bool Cancelled;
        public readonly TileInfo TileInfo;
        public readonly byte[] Image;
    }
}