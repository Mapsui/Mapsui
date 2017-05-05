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
using Mapsui.Geometries;

namespace Mapsui.Fetcher
{
    public interface IAsyncDataFetcher
    {
        void AbortFetch();

        /// <summary>
        ///     Indicates that there has been a change in the view of the map
        /// </summary>
        /// <param name="majorChange">
        ///     If true an implementation should always refresh it's data. If false (minorChange) the
        ///     implemenatation could ignore it.
        /// </param>
        /// <param name="extent">The new extent of the visisble map</param>
        /// <param name="resolution">The new resolution of the visible map</param>
        void ViewChanged(bool majorChange, BoundingBox extent, double resolution);

        event DataChangedEventHandler DataChanged;
        void ClearCache();
    }

    public delegate void DataChangedEventHandler(object sender, DataChangedEventArgs e);

    public class DataChangedEventArgs : EventArgs
    {
        public DataChangedEventArgs() : this(null, false, null)
        {
        }

        public DataChangedEventArgs(Exception error, bool cancelled, TileInfo tileInfo)
            : this(error, cancelled, tileInfo, string.Empty)
        {
        }

        public DataChangedEventArgs(Exception error, bool cancelled, TileInfo tileInfo, string layerName)
        {
            Error = error;
            Cancelled = cancelled;
            TileInfo = tileInfo;
            LayerName = layerName;
        }

        public Exception Error { get; }
        public bool Cancelled { get; }
        public TileInfo TileInfo { get; } // todo: remove
        public string LayerName { get; }
    }
}