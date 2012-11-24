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

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using SharpMap.Geometries;
using System.Globalization;
using System.Collections.Generic;
using SharpMap.Providers;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// The OleDbPoint provider is used for rendering point data from an OleDb compatible datasource.
    /// </summary>
    /// <remarks>
    /// <para>The data source will need to have two double-type columns, xColumn and yColumn that contains the coordinates of the point,
    /// and an integer-type column containing a unique identifier for each row.</para>
    /// <para>To get good performance, make sure you have applied indexes on ID, xColumn and yColumns in your datasource table.</para>
    /// </remarks>
    public class OleDbPoint : IProvider, IDisposable
    {
        private string _ConnectionString;
        private string _defintionQuery;
        private bool _IsOpen;
        private string _ObjectIdColumn;
        private int _SRID = -1;
        private string _Table;
        private string _XColumn;
        private string _YColumn;

        /// <summary>
        /// Initializes a new instance of the OleDbPoint provider
        /// </summary>
        /// <param name="ConnectionStr"></param>
        /// <param name="tablename"></param>
        /// <param name="OID_ColumnName"></param>
        /// <param name="xColumn"></param>
        /// <param name="yColumn"></param>
        public OleDbPoint(string ConnectionStr, string tablename, string OID_ColumnName, string xColumn, string yColumn)
        {
            Table = tablename;
            XColumn = xColumn;
            YColumn = yColumn;
            ObjectIdColumn = OID_ColumnName;
            ConnectionString = ConnectionStr;
        }

        /// <summary>
        /// Data table name
        /// </summary>
        public string Table
        {
            get { return _Table; }
            set { _Table = value; }
        }


        /// <summary>
        /// Name of column that contains the Object ID
        /// </summary>
        public string ObjectIdColumn
        {
            get { return _ObjectIdColumn; }
            set { _ObjectIdColumn = value; }
        }

        /// <summary>
        /// Name of column that contains X coordinate
        /// </summary>
        public string XColumn
        {
            get { return _XColumn; }
            set { _XColumn = value; }
        }

        /// <summary>
        /// Name of column that contains Y coordinate
        /// </summary>
        public string YColumn
        {
            get { return _YColumn; }
            set { _YColumn = value; }
        }

        /// <summary>
        /// Connectionstring
        /// </summary>
        public string ConnectionString
        {
            get { return _ConnectionString; }
            set { _ConnectionString = value; }
        }

        /// <summary>
        /// Definition query used for limiting dataset
        /// </summary>
        public string DefinitionQuery
        {
            get { return _defintionQuery; }
            set { _defintionQuery = value; }
        }

        #region IProvider Members

        /// <summary>
        /// Returns geometries within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<Geometry> GetGeometriesInView(BoundingBox bbox)
        {
            Collection<Geometry> features = new Collection<Geometry>();
            using (OleDbConnection conn = new OleDbConnection(_ConnectionString))
            {
                string strSQL = "Select " + XColumn + ", " + YColumn + " FROM " + Table + " WHERE ";
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += _defintionQuery + " AND ";
                //Limit to the points within the boundingbox
                strSQL += XColumn + " BETWEEN " + bbox.Left.ToString(CultureInfo.InvariantCulture) + " AND " +
                          bbox.Right.ToString(CultureInfo.InvariantCulture) + " AND " +
                          YColumn + " BETWEEN " + bbox.Bottom.ToString(CultureInfo.InvariantCulture) + " AND " +
                          bbox.Top.ToString(CultureInfo.InvariantCulture);

                using (OleDbCommand command = new OleDbCommand(strSQL, conn))
                {
                    conn.Open();
                    using (OleDbDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value && dr[1] != DBNull.Value)
                                features.Add(new Point((double) dr[0], (double) dr[1]));
                        }
                    }
                    conn.Close();
                }
            }
            return features;
        }

        /// <summary>
        /// Returns geometry Object IDs whose bounding box intersects 'bbox'
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<uint> GetObjectIDsInView(BoundingBox bbox)
        {
            Collection<uint> objectlist = new Collection<uint>();
            using (OleDbConnection conn = new OleDbConnection(_ConnectionString))
            {
                string strSQL = "Select " + ObjectIdColumn + " FROM " + Table + " WHERE ";
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += _defintionQuery + " AND ";
                //Limit to the points within the boundingbox
                strSQL += XColumn + " BETWEEN " + bbox.Left.ToString(CultureInfo.InvariantCulture) + " AND " +
                          bbox.Right.ToString(CultureInfo.InvariantCulture) + " AND " + YColumn +
                          " BETWEEN " + bbox.Bottom.ToString(CultureInfo.InvariantCulture) + " AND " +
                          bbox.Top.ToString(CultureInfo.InvariantCulture);

                using (OleDbCommand command = new OleDbCommand(strSQL, conn))
                {
                    conn.Open();
                    using (OleDbDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                            if (dr[0] != DBNull.Value)
                                objectlist.Add((uint) (int) dr[0]);
                    }
                    conn.Close();
                }
            }
            return objectlist;
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public Geometry GetGeometryByID(uint oid)
        {
            Geometry geom = null;
            using (OleDbConnection conn = new OleDbConnection(_ConnectionString))
            {
                string strSQL = "Select " + XColumn + ", " + YColumn + " FROM " + Table + " WHERE " + ObjectIdColumn +
                                "=" + oid.ToString();
                using (OleDbCommand command = new OleDbCommand(strSQL, conn))
                {
                    conn.Open();
                    using (OleDbDataReader dr = command.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            //If the read row is OK, create a point geometry from the XColumn and YColumn and return it
                            if (dr[0] != DBNull.Value && dr[1] != DBNull.Value)
                                geom = new Point((double) dr[0], (double) dr[1]);
                        }
                    }
                    conn.Close();
                }
            }
            return geom;
        }

        /// <summary>
        /// Throws NotSupportedException. 
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            throw new NotSupportedException(
                "ExecuteIntersectionQuery(Geometry) is not supported by the OleDbPointProvider.");
            //When relation model has been implemented the following will complete the query
            /*
			ExecuteIntersectionQuery(geom.GetBoundingBox(), ds);
			if (ds.Tables.Count > 0)
			{
				for(int i=ds.Tables[0].Count-1;i>=0;i--)
				{
					if (!geom.Intersects(ds.Tables[0][i].Geometry))
						ds.Tables.RemoveAt(i);
				}
			}
			*/
        }

        /// <summary>
        /// Returns all features with the view box
        /// </summary>
        /// <param name="bbox">view box</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(BoundingBox bbox, FeatureDataSet ds)
        {
            //List<Geometries.Geometry> features = new List<SharpMap.Geometries.Geometry>();
            using (OleDbConnection conn = new OleDbConnection(_ConnectionString))
            {
                string strSQL = "Select * FROM " + Table + " WHERE ";
                if (!String.IsNullOrEmpty(_defintionQuery))
                    //If a definition query has been specified, add this as a filter on the query
                    strSQL += _defintionQuery + " AND ";
                //Limit to the points within the boundingbox
                strSQL += XColumn + " BETWEEN " + bbox.Left.ToString(CultureInfo.InvariantCulture) + " AND " +
                          bbox.Right.ToString(CultureInfo.InvariantCulture) + " AND " + YColumn +
                          " BETWEEN " + bbox.Bottom.ToString(CultureInfo.InvariantCulture) + " AND " +
                          bbox.Top.ToString(CultureInfo.InvariantCulture);

                using (OleDbDataAdapter adapter = new OleDbDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    DataSet ds2 = new DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (DataColumn col in ds2.Tables[0].Columns)
                            fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (DataRow dr in ds2.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds2.Tables[0].Columns)
                                fdr[col.ColumnName] = dr[col];
                            if (dr[XColumn] != DBNull.Value && dr[YColumn] != DBNull.Value)
                                fdr.Geometry = new Point((double) dr[XColumn], (double) dr[YColumn]);
                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>Total number of features</returns>
        public int GetFeatureCount()
        {
            int count = 0;
            using (OleDbConnection conn = new OleDbConnection(_ConnectionString))
            {
                string strSQL = "Select Count(*) FROM " + Table;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    //If a definition query has been specified, add this as a filter on the query
                    strSQL += " WHERE " + _defintionQuery;

                using (OleDbCommand command = new OleDbCommand(strSQL, conn))
                {
                    conn.Open();
                    count = (int) command.ExecuteScalar();
                    conn.Close();
                }
            }
            return count;
        }

        /// <summary>
        /// Returns a datarow based on a RowID
        /// </summary>
        /// <param name="RowID"></param>
        /// <returns>datarow</returns>
        public FeatureDataRow GetFeature(uint RowID)
        {
            using (OleDbConnection conn = new OleDbConnection(_ConnectionString))
            {
                string strSQL = "select * from " + Table + " WHERE " + ObjectIdColumn + "=" + RowID.ToString();

                using (OleDbDataAdapter adapter = new OleDbDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (DataColumn col in ds.Tables[0].Columns)
                            fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds.Tables[0].Columns)
                                fdr[col.ColumnName] = dr[col];
                            if (dr[XColumn] != DBNull.Value && dr[YColumn] != DBNull.Value)
                                fdr.Geometry = new Point((double) dr[XColumn], (double) dr[YColumn]);
                            return fdr;
                        }
                        else
                            return null;
                    }
                    else
                        return null;
                }
            }
        }

        /// <summary>
        /// Boundingbox of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public BoundingBox GetExtents()
        {
            BoundingBox box = null;
            using (OleDbConnection conn = new OleDbConnection(_ConnectionString))
            {
                string strSQL = "Select Min(" + XColumn + ") as MinX, Min(" + YColumn + ") As MinY, " +
                                "Max(" + XColumn + ") As MaxX, Max(" + YColumn + ") As MaxY FROM " + Table;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    //If a definition query has been specified, add this as a filter on the query
                    strSQL += " WHERE " + _defintionQuery;

                using (OleDbCommand command = new OleDbCommand(strSQL, conn))
                {
                    conn.Open();
                    using (OleDbDataReader dr = command.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            //If the read row is OK, create a point geometry from the XColumn and YColumn and return it
                            if (dr[0] != DBNull.Value && dr[1] != DBNull.Value && dr[2] != DBNull.Value &&
                                dr[3] != DBNull.Value)
                                box = new BoundingBox((double) dr[0], (double) dr[1], (double) dr[2], (double) dr[3]);
                        }
                    }
                    conn.Close();
                }
            }
            return box;
        }

        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        public string ConnectionId
        {
            get { return _ConnectionString; }
        }

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public void Open()
        {
            //Don't really do anything. OleDb's ConnectionPooling takes over here
            _IsOpen = true;
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            //Don't really do anything. OleDb's ConnectionPooling takes over here
            _IsOpen = false;
        }

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        public bool IsOpen
        {
            get { return _IsOpen; }
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

        #region Disposers and finalizers

        private bool disposed = false;

        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~OleDbPoint()
        {
            Dispose();
        }

        #endregion

        #region IProvider Members

        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            FeatureDataSet dataSet = new FeatureDataSet();
            ExecuteIntersectionQuery(box, dataSet);
            return SharpMap.Providers.Utilities.DataSetToFeatures(dataSet);
        }

        #endregion
    }
}