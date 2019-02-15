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

using Mapsui.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries.WellKnownBinary;
using Mapsui.Geometries.WellKnownText;

namespace Mapsui.Providers
{
    /// <summary>
    /// Datasource for storing a limited set of geometries.
    /// </summary>
    /// <remarks>
    /// <para>The MemoryProvider doesn’t utilize performance optimizations of spatial indexing,
    /// and thus is primarily meant for rendering a limited set of Geometries.</para>
    /// <para>A common use of the MemoryProvider is for highlighting a set of selected features.</para>
    /// <example>
    /// The following example gets data within a BoundingBox of another datasource and adds it to the map.
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
    /// <code lang="C#">
    /// List&#60;Mapsui.Geometries.Geometry&#62; geometries = new List&#60;Mapsui.Geometries.Geometry&#62;();
    /// //Add two points
    /// geometries.Add(new Mapsui.Geometries.Point(23.345,64.325));
    /// geometries.Add(new Mapsui.Geometries.Point(23.879,64.194));
    /// Mapsui.Layers.VectorLayer layerVehicles = new Mapsui.Layers.VectorLayer("Vechicles");
    /// layerVehicles.DataSource = new Mapsui.Data.Providers.MemoryProvider(geometries);
    /// layerVehicles.Style.Symbol = Bitmap.FromFile(@"C:\data\car.gif");
    /// myMap.Layers.Add(layerVehicles);
    /// </code>
    /// </example>
    /// </remarks>
    public class MemoryProvider : IProvider
    {

        /// <summary>
        /// Gets or sets the geometries this datasource contains
        /// </summary>
        public IReadOnlyList<IFeature> Features { get; private set; }

        public double SymbolSize { get; set; } = 64;

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public string CRS { get; set; }

        BoundingBox _boundingBox;

        public MemoryProvider()
        {
            CRS = "";
            Features = new List<IFeature>();
            _boundingBox = GetExtents(Features);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="geometries">Set of geometries that this datasource should contain</param>
        public MemoryProvider(IEnumerable<IGeometry> geometries)
        {
            CRS = "";
            Features = geometries.Select(g => new Feature { Geometry = g }).ToList();
            _boundingBox = GetExtents(Features);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="feature">Feature to be in this datasource</param>
        public MemoryProvider(IFeature feature)
        {
            CRS = "";
            Features = new List<IFeature> { feature };
            _boundingBox = GetExtents(Features);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="wellKnownTextGeometry"><see cref="Geometry"/> as Well-known Text to be included in this datasource</param>
        public MemoryProvider(string wellKnownTextGeometry)
            : this(GeometryFromWKT.Parse(wellKnownTextGeometry))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="features">Features to be included in this datasource</param>
        public MemoryProvider(IEnumerable<IFeature> features)
        {
            CRS = "";
            Features = features.ToList();
            _boundingBox = GetExtents(Features);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="geometry">Geometry to be in this datasource</param>
        public MemoryProvider(Geometry geometry)
        {
            CRS = "";

            Features = new List<IFeature>
            {
                new Feature
                {
                    Geometry = geometry
                }
            };
            _boundingBox = GetExtents(Features);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="wellKnownBinaryGeometry"><see cref="Geometry"/> as Well-known Binary to be included in this datasource</param>
        public MemoryProvider(byte[] wellKnownBinaryGeometry) : this(GeometryFromWKB.Parse(wellKnownBinaryGeometry))
        {
        }

        public virtual IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            if (box == null) throw new ArgumentNullException(nameof(box));

            var features = Features.ToList();

            // Use a larger extent so that symbols partially outside of the extent are included
            var grownBox = box.Grow(resolution * SymbolSize * 0.5);

            return features.Where(f => f.Geometry != null && f.Geometry.BoundingBox.Intersects(grownBox)).ToList();
        }

        public IFeature Find(object value, string primaryKey)
        {
            return Features.FirstOrDefault(f => f[primaryKey] != null && value != null &&
                f[primaryKey].Equals(value));
        }

        /// <summary>
        /// Boundingbox of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public BoundingBox GetExtents()
        {
            return _boundingBox;
        }

        private static BoundingBox GetExtents(IReadOnlyList<IFeature> features)
        {
            BoundingBox box = null;
            foreach (var feature in features)
            {
                if (feature.Geometry.IsEmpty()) continue;
                box = box == null
                    ? feature.Geometry.BoundingBox
                    : box.Join(feature.Geometry.BoundingBox);
            }
            return box;
        }

        public void Clear()
        {
            Features = new List<IFeature>();
        }

        public void ReplaceFeatures(IEnumerable<IFeature> features)
        {
            Features = features.ToList();
            _boundingBox = GetExtents(Features);
        }

    }
}