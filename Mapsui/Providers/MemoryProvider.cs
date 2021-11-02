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
using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.Geometries.WellKnownBinary;
using Mapsui.Geometries.WellKnownText;
using Mapsui.Layers;

namespace Mapsui.Providers
{
    /// <summary>
    /// Data source for storing a limited set of geometries.
    /// </summary>
    /// <remarks>
    /// <para>The MemoryProvider does not utilize performance optimizations of spatial indexing,
    /// and thus is primarily meant for rendering a limited set of Geometries.</para>
    /// <para>A common use of the MemoryProvider is for highlighting a set of selected features.</para>
    /// <example>
    /// The following example gets data within a BoundingBox of another data source and adds it to the map.
    /// <code lang="C#">
    /// List&#60;Geometry&#62; geometries = myMap.Layers[0].DataSource.GetGeometriesInView(myBox);
    /// VectorLayer laySelected = new VectorLayer("Selected Features");
    /// laySelected.DataSource = new MemoryProvider(geometries);
    /// laySelected.Style.Outline = new Pen(Color.Magenta, 3f);
    /// laySelected.Style.EnableOutline = true;
    /// myMap.Layers.Add(laySelected);
    /// </code>
    /// </example>
    /// <example>
    /// Adding points of interest to the map. This is useful for vehicle tracking etc.
    /// </example>
    /// </remarks>
    public class MemoryProvider<T> : IProvider<T> where T : IFeature
    {
        /// <summary>
        /// Gets or sets the geometries this data source contains
        /// </summary>
        public IReadOnlyList<IGeometryFeature> Features { get; private set; }

        public double SymbolSize { get; set; } = 64;

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public string CRS { get; set; } = "";

        private readonly MRect _boundingBox;

        public MemoryProvider()
        {
            Features = new List<IGeometryFeature>();
            _boundingBox = GetExtent(Features);
        }

        /// <summary>
        /// Initializes a new instance of the MemoryProvider
        /// </summary>
        /// <param name="geometries">Set of geometries that this data source should contain</param>
        public MemoryProvider(IEnumerable<IGeometry> geometries)
        {
            Features = geometries.Select(g => new Feature { Geometry = g }).ToList();
            _boundingBox = GetExtent(Features);
        }

        /// <summary>
        /// Initializes a new instance of the MemoryProvider
        /// </summary>
        /// <param name="feature">Feature to be in this dataSource</param>
        public MemoryProvider(IGeometryFeature feature)
        {
            Features = new List<IGeometryFeature> { feature };
            _boundingBox = GetExtent(Features);
        }

        /// <summary>
        /// Initializes a new instance of the MemoryProvider
        /// </summary>
        /// <param name="wellKnownTextGeometry"><see cref="Geometry"/> as Well-known Text to be included in this data source</param>
        public MemoryProvider(string wellKnownTextGeometry)
            : this(GeometryFromWKT.Parse(wellKnownTextGeometry))
        {
        }

        /// <summary>
        /// Initializes a new instance of the MemoryProvider
        /// </summary>
        /// <param name="features">Features to be included in this dataSource</param>
        public MemoryProvider(IEnumerable<IGeometryFeature> features)
        {
            Features = features.ToList();
            _boundingBox = GetExtent(Features);
        }

        /// <summary>
        /// Initializes a new instance of the MemoryProvider
        /// </summary>
        /// <param name="geometry">Geometry to be in this dataSource</param>
        public MemoryProvider(Geometry geometry)
        {
            Features = new List<IGeometryFeature>
            {
                new Feature
                {
                    Geometry = geometry
                }
            };
            _boundingBox = GetExtent(Features);
        }

        /// <summary>
        /// Initializes a new instance of the MemoryProvider
        /// </summary>
        /// <param name="wellKnownBinaryGeometry"><see cref="Geometry"/> as Well-known Binary to be included in this data source</param>
        public MemoryProvider(byte[] wellKnownBinaryGeometry) : this(GeometryFromWKB.Parse(wellKnownBinaryGeometry))
        {
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

        private static MRect GetExtent(IReadOnlyList<IGeometryFeature> features)
        {
            MRect? box = null;
            foreach (var feature in features)
            {
                if (feature.Geometry.IsEmpty()) continue;
                box = box == null
                    ? feature.Geometry.BoundingBox.ToMRect()
                    : box.Join(feature.Geometry.BoundingBox.ToMRect());
            }
            return box;
        }

        public void Clear()
        {
            Features = new List<IGeometryFeature>();
        }
    }
}