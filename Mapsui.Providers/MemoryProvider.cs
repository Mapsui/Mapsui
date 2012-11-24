// Copyright 2006 - Morten Nielsen (www.iter.dk)
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
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharpMap.Converters.WellKnownBinary;
using SharpMap.Converters.WellKnownText;
using SharpMap.Geometries;

namespace SharpMap.Data.Providers
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
    /// List&#60;SharpMap.Geometries.Geometry&#62; geometries = new List&#60;SharpMap.Geometries.Geometry&#62;();
    /// //Add two points
    /// geometries.Add(new SharpMap.Geometries.Point(23.345,64.325));
    /// geometries.Add(new SharpMap.Geometries.Point(23.879,64.194));
    /// SharpMap.Layers.VectorLayer layerVehicles = new SharpMap.Layers.VectorLayer("Vechicles");
    /// layerVehicles.DataSource = new SharpMap.Data.Providers.MemoryProvider(geometries);
    /// layerVehicles.Style.Symbol = Bitmap.FromFile(@"C:\data\car.gif");
    /// myMap.Layers.Add(layerVehicles);
    /// </code>
    /// </example>
    /// </remarks>
    public class MemoryProvider : IProvider, IDisposable
    {
        #region Fields

        private Features _Features;
        private int _SRID = -1;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the geometries this datasource contains
        /// </summary>
        public IFeatures features
        {
            get { return _Features; }
        }

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        public bool IsOpen
        {
            get { return true; }
        }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public int SRID
        {
            get { return _SRID; }
            set { _SRID = value; }
        }

        #endregion
        
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="geometries">Set of geometries that this datasource should contain</param>
        public MemoryProvider(IEnumerable<IGeometry> geometries)
        {
            _Features = new Features();
            foreach (IGeometry geometry in geometries)
            {
                IFeature feature = _Features.New();
                feature.Geometry = geometry;
                _Features.Add(feature);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="feature">Feature to be in this datasource</param>
        public MemoryProvider(IFeature feature)
        {
            _Features = new Features();
            _Features.Add(feature);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="features">Features to be included in this datasource</param>
        public MemoryProvider(Features features)
        {
            _Features = features;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="geometry">Geometry to be in this datasource</param>
        public MemoryProvider(Geometry geometry)
        {
            _Features = new Features();
            IFeature feature = _Features.New();
            feature.Geometry = geometry;
            _Features.Add(feature);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="wellKnownBinaryGeometry"><see cref="SharpMap.Geometries.Geometry"/> as Well-known Binary to be included in this datasource</param>
        public MemoryProvider(byte[] wellKnownBinaryGeometry) : this(GeometryFromWKB.Parse(wellKnownBinaryGeometry))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryProvider"/>
        /// </summary>
        /// <param name="wellKnownTextGeometry"><see cref="SharpMap.Geometries.Geometry"/> as Well-known Text to be included in this datasource</param>
        public MemoryProvider(string wellKnownTextGeometry) : this(GeometryFromWKT.Parse(wellKnownTextGeometry))
        {
        }


        #endregion

        #region IProvider Members

        /// <summary>
        /// Returns features within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<IGeometry> GetGeometriesInView(BoundingBox bbox)
        {
            Collection<IGeometry> list = new Collection<IGeometry>();
            foreach (IFeature feature in _Features)
                if (!feature.Geometry.IsEmpty())
                    if (feature.Geometry.GetBoundingBox().Intersects(bbox))
                        list.Add(feature.Geometry);
            return list;
        }

        /// <summary>
        /// Returns all objects whose boundingbox intersects 'bbox'.
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<uint> GetObjectIDsInView(BoundingBox bbox)
        {
            Collection<uint> list = new Collection<uint>();
            for (int i = 0; i < _Features.Count; i++)
                if (_Features[i].Geometry.GetBoundingBox().Intersects(bbox))
                    list.Add((uint) i);
            return list;
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public IGeometry GetGeometry(uint oid)
        {
            return _Features[(int)oid].Geometry;
        }

        /// <summary>
        /// Throws an NotSupportedException. Attribute data is not supported by this datasource
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            throw new NotSupportedException("Attribute data is not supported by the MemoryProvider.");
        }

        /// <summary>
        /// Throws an NotSupportedException. Attribute data is not supported by this datasource
        /// </summary>
        /// <param name="box"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(BoundingBox box, FeatureDataSet ds)
        {
            throw new NotSupportedException("Attribute data is not supported by the MemoryProvider.");
        }

        public IFeatures GetFeaturesInView(IView view)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public int GetFeatureCount()
        {
            return _Features.Count;
        }

        /// <summary>
        /// Throws an NotSupportedException. Attribute data is not supported by this datasource
        /// </summary>
        /// <param name="RowID"></param>
        /// <returns></returns>
        public FeatureDataRow GetFeature(uint RowID)
        {
            throw new NotSupportedException("Attribute data is not supported by the MemoryProvider.");
        }

        /// <summary>
        /// Boundingbox of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public BoundingBox GetExtents()
        {
            if (_Features.Count == 0)
                return null;
            BoundingBox box = null; // _Geometries[0].GetBoundingBox();
            for (int i = 0; i < _Features.Count; i++)
                if (!_Features[i].Geometry.IsEmpty())
                    box = box == null ? _Features[i].Geometry.GetBoundingBox() : box.Join(_Features[i].Geometry.GetBoundingBox());

            return box;
        }

        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        /// <remarks>
        /// The ConnectionID is meant for Connection Pooling which doesn't apply to this datasource. Instead
        /// <c>String.Empty</c> is returned.
        /// </remarks>
        public string ConnectionID
        {
            get { return String.Empty; }
        }

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public void Open()
        {
            //Do nothing;
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            //Do nothing;
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            _Features = null;
        }

        #endregion
    }
}