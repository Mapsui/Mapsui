// Copyright 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Converters.WellKnownBinary;
using Mapsui.Converters.WellKnownText;
using Mapsui.Geometries;

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
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Gets or sets the geometries this datasource contains
        /// </summary>
        public IFeatures Features { get; set; }

        public double SymbolSize { get; set; }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public int SRID { get; set; }
        
        public MemoryProvider()
        {
            SRID = -1;
            Features = new Features();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="geometries">Set of geometries that this datasource should contain</param>
        public MemoryProvider(IEnumerable<IGeometry> geometries)
        {
            SRID = -1;
            Features = new Features();
            foreach (IGeometry geometry in geometries)
            {
                IFeature feature = Features.New();
                feature.Geometry = geometry;
                Features.Add(feature);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="feature">Feature to be in this datasource</param>
        public MemoryProvider(IFeature feature)
        {
            SRID = -1;
            Features = new Features {feature};
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="wellKnownTextGeometry"><see cref="Mapsui.Geometries.Geometry"/> as Well-known Text to be included in this datasource</param>
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
            SRID = -1;
            Features = new Features();
            foreach (var feature in features) Features.Add(feature);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="features">Features to be included in this datasource</param>
        public MemoryProvider(IFeatures features)
        {
            SRID = -1;
            Features = features;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="geometry">Geometry to be in this datasource</param>
        public MemoryProvider(Geometry geometry)
        {
            SRID = -1;
            Features = new Features();
            IFeature feature = Features.New();
            feature.Geometry = geometry;
            Features.Add(feature);
            SymbolSize = 64;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="wellKnownBinaryGeometry"><see cref="Mapsui.Geometries.Geometry"/> as Well-known Binary to be included in this datasource</param>
        public MemoryProvider(byte[] wellKnownBinaryGeometry) : this(GeometryFromWKB.Parse(wellKnownBinaryGeometry))
        {
        }

        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            lock (_syncRoot)
            {
                var features = Features.ToList();

                foreach (var feature in features)
                {
                    if (feature.Geometry == null)
                        continue;

                    if (box.Intersects(feature.Geometry.GetBoundingBox().Grow(resolution * SymbolSize * 0.5)))
                    {
                        yield return feature;
                    }
                }
            }
        }

        public IFeature Find(object value)
        {
            lock (_syncRoot)
            {
                if (string.IsNullOrEmpty(Features.PrimaryKey)) throw new Exception("ID Field was not set");

                return Features.FirstOrDefault(f =>
                                               ((f[Features.PrimaryKey] != null) && (value != null)) &&
                                               f[Features.PrimaryKey].Equals(value));
            }
        }

        /// <summary>
        /// Boundingbox of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public BoundingBox GetExtents()
        {
            lock (_syncRoot)
            {
                BoundingBox box = null;
                foreach (IFeature feature in Features)
                {
                    if (feature.Geometry.IsEmpty()) continue;
                    box = box == null
                            ? feature.Geometry.GetBoundingBox()
                            : box.Join(feature.Geometry.GetBoundingBox());
                }
                return box;
            }
        }
        
        public void Clear()
        {
            lock (_syncRoot)
            {
                Features.Clear();    
            }
        }
    }
}