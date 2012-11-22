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
using BruTile;
using SharpMap.Geometries;

namespace SharpMap.Fetcher
{
    public interface IAsyncDataFetcher
    {
        void AbortFetch();
        void ViewChanged(bool changeEnd, BoundingBox extent, double resolution);
        event DataChangedEventHandler DataChanged;
        void ClearCache();
    }

    public delegate void DataChangedEventHandler(object sender, DataChangedEventArgs e);

    public class DataChangedEventArgs
    {
        public DataChangedEventArgs(Exception error, bool cancelled, TileInfo tileInfo)
            : this(error, cancelled, tileInfo, string.Empty)
        { }
        
        public DataChangedEventArgs(Exception error, bool cancelled, TileInfo tileInfo, string layerName)
        {
            Error = error;
            Cancelled = cancelled;
            TileInfo = tileInfo;
            LayerName = layerName;
        }

        public Exception Error { get; private set; }
        public bool Cancelled { get; private set; }
        public TileInfo TileInfo { get; private set; }
        public string LayerName { get; set; }
    }
}
