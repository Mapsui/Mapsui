// Copyright 2006 - Morten Nielsen (www.iter.dk)
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
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;

namespace Mapsui.Providers
{
    /// <summary>
    /// Data source for storing a limited set of geometries.
    /// </summary>
    public class MemoryProvider<T> : IProvider<T> where T : IFeature
    {
        /// <summary>
        /// Gets or sets the geometries this data source contains
        /// </summary>
        public IReadOnlyList<IFeature> Features { get; private set; }

        public double SymbolSize { get; set; } = 64;

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public string CRS { get; set; } = "";

        private readonly MRect? _boundingBox;

        public MemoryProvider()
        {
            Features = new List<IFeature>();
            _boundingBox = GetExtent(Features);
        }

        /// <summary>
        /// Initializes a new instance of the MemoryProvider
        /// </summary>
        /// <param name="features">Features to be included in this dataSource</param>
        public MemoryProvider(IEnumerable<IFeature> features)
        {
            Features = features.ToList();
            _boundingBox = GetExtent(Features);
        }

        public virtual IEnumerable<T> GetFeatures(FetchInfo fetchInfo)
        {
            if (fetchInfo == null) throw new ArgumentNullException(nameof(fetchInfo));
            if (fetchInfo.Extent == null) throw new ArgumentNullException(nameof(fetchInfo.Extent));

            var features = Features.ToList();

            fetchInfo = new FetchInfo(fetchInfo);
            // Use a larger extent so that symbols partially outside of the extent are included
            var grownBox = fetchInfo.Extent.Grow(fetchInfo.Resolution * SymbolSize * 0.5);
            var grownFeatures = features.Where(f => f != null && f.BoundingBox.Intersects(grownBox));
            return (IEnumerable<T>)grownFeatures.ToList(); // Why do I need to cast if T is constrained to IFeature?
        }

        public IFeature Find(object? value, string primaryKey)
        {
            return Features.FirstOrDefault(f => f[primaryKey] != null && value != null &&
                f[primaryKey].Equals(value));
        }

        /// <summary>
        /// BoundingBox of data set
        /// </summary>
        /// <returns>BoundingBox</returns>
        public MRect GetExtent()
        {
            return _boundingBox;
        }

        private static MRect? GetExtent(IReadOnlyList<IFeature> features)
        {
            MRect? box = null;
            foreach (var feature in features)
            {
                if (feature.BoundingBox == null) continue;
                box = box == null
                    ? feature.BoundingBox
                    : box.Join(feature.BoundingBox);
            }
            return box;
        }

        public void Clear()
        {
            Features = new List<IGeometryFeature>();
        }
    }
}