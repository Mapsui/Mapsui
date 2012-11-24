// Copyright 2006 - Ricardo Stuven (rstuven@gmail.com)
// Copyright 2006 - Morten Nielsen (www.iter.dk)
//
// MsSqlSpatial provider by Ricardo Stuven.
// Based on PostGIS provider by Morten Nielsen.
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using SharpMap.Converters.WellKnownBinary;
using SharpMap.Geometries;
using System.Globalization;
using SharpMap.Providers;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Microsoft SQL Server 2005 / MsSqlSpatial dataprovider
    /// </summary>
    /// <example>
    /// Adding a datasource to a layer:
    /// <code lang="C#">
    /// SharpMap.Layers.VectorLayer myLayer = new SharpMap.Layers.VectorLayer("My layer");
    /// string ConnStr = @"Data Source=localhost\sqlexpress;Initial Catalog=myGisDb;Integrated Security=SSPI;";
    /// myLayer.DataSource = new SharpMap.Data.Providers.MsSqlSpatial(ConnStr, "myTable", "myId");
    /// </code>
    /// </example>
    [Serializable]
    public class MsSqlSpatial : IProvider, IDisposable
    {
        private string _ConnectionString;
        private string _DefinitionQuery = String.Empty;
        private string _FeatureColumns = "*";
        private string _GeometryColumn;
        private string _GeometryExpression = "{0}";
        private bool _IsOpen;
        private string _ObjectIdColumn;
        private string _OrderQuery = String.Empty;
        private int _srid = -2;
        private string _Table;
        private int _TargetSRID = -1;

        /// <summary>
        /// Initializes a new connection to MsSqlSpatial
        /// </summary>
        /// <param name="connectionString">Connectionstring</param>
        /// <param name="tableName">Name of data table</param>
        /// <param name="geometryColumnName">Name of geometry column</param>
        /// /// <param name="identifierColumnName">Name of column with unique identifier</param>
        public MsSqlSpatial(string connectionString, string tableName, string geometryColumnName,
                            string identifierColumnName)
        {
            ConnectionString = connectionString;
            Table = tableName;
            GeometryColumn = geometryColumnName;
            ObjectIdColumn = identifierColumnName;
        }

        /// <summary>
        /// Initializes a new connection to MsSqlSpatial
        /// </summary>
        /// <param name="ConnectionStr">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="OID_ColumnName">Name of column with unique identifier</param>
        public MsSqlSpatial(string connectionString, string tableName, string identifierColumnName)
            : this(connectionString, tableName, "", identifierColumnName)
        {
            GeometryColumn = GetGeometryColumn();
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
        /// Data table name
        /// </summary>
        public string Table
        {
            get { return _Table; }
            set { _Table = value; }
        }

        /// <summary>
        /// Name of geometry column
        /// </summary>
        public string GeometryColumn
        {
            get { return _GeometryColumn; }
            set { _GeometryColumn = value; }
        }

        /// <summary>
        /// Expression template for geometry column evaluation.
        /// </summary>
        /// <example>
        /// You could, for instance, simplify your geometries before they're displayed.
        /// Simplification helps to speed the rendering of big geometries.
        /// Here's a sample code to simplify geometries using 100 meters of threshold.
        /// <code>
        /// datasource.GeometryExpression = "ST.Simplify({0}, 100)";
        /// </code>
        /// Also you could draw a 20 meters buffer around those little points:
        /// <code>
        /// datasource.GeometryExpression = "ST.Buffer({0}, 20)";
        /// </code>
        /// </example>
        public string GeometryExpression
        {
            get { return _GeometryExpression; }
            set { _GeometryExpression = value; }
        }

        /// <summary>
        /// List of columns or T-SQL expressions separated by comma.
        /// Using "*" (the value by default), all columns are selected.
        /// </summary>
        public string FeatureColumns
        {
            get { return _FeatureColumns; }
            set { _FeatureColumns = value; }
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
        /// Definition query used for limiting dataset (WHERE clause)
        /// </summary>
        public string DefinitionQuery
        {
            get { return _DefinitionQuery; }
            set { _DefinitionQuery = value; }
        }

        /// <summary>
        /// Columns or T-SQL expressions for sorting (ORDER BY clause)
        /// </summary>
        public string OrderQuery
        {
            get { return _OrderQuery; }
            set { _OrderQuery = value; }
        }


        /// <summary>
        /// The target spatial reference ID (SRID). 
        /// It allows on-the-fly transformations in the server-side.
        /// </summary>
        public int TargetSRID
        {
            get { return _TargetSRID; }
            set { _TargetSRID = value; }
        }

        private string TargetGeometryColumn
        {
            get
            {
                if (SRID > 0 && TargetSRID > 0 && SRID != TargetSRID)
                    return "ST.Transform(" + GeometryColumn + "," + TargetSRID + ")";
                else
                    return GeometryColumn;
            }
        }

        #region IProvider Members

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        public bool IsOpen
        {
            get { return _IsOpen; }
        }

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public void Open()
        {
            //Don't really do anything. SqlClient's ConnectionPooling takes over here
            _IsOpen = true;
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            //Don't really do anything. SqlClient's ConnectionPooling takes over here
            _IsOpen = false;
        }

        /// <summary>
        /// Returns geometries within the specified bounding box
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<Geometry> GetGeometriesInView(BoundingBox bbox)
        {
            Collection<Geometry> features = new Collection<Geometry>();
            using (SqlConnection conn = new SqlConnection(_ConnectionString))
            {
                string strSQL = "SELECT ST.AsBinary(" + BuildGeometryExpression() + ") ";
                strSQL += "FROM ST.FilterQuery" + BuildSpatialQuerySuffix() + "(" + BuildEnvelope(bbox) + ")";

                if (!String.IsNullOrEmpty(DefinitionQuery))
                    strSQL += " WHERE " + DefinitionQuery;

                if (!String.IsNullOrEmpty(OrderQuery))
                    strSQL += " ORDER BY " + OrderQuery;

                using (SqlCommand command = new SqlCommand(strSQL, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                Geometry geom = GeometryFromWKB.Parse((byte[]) dr[0]);
                                if (geom != null)
                                    features.Add(geom);
                            }
                        }
                    }
                    conn.Close();
                }
            }
            return features;
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public Geometry GetGeometryByID(uint oid)
        {
            Geometry geom = null;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string strSQL = "SELECT ST.AsBinary(" + BuildGeometryExpression() + ") AS Geom FROM " + Table +
                                " WHERE " + ObjectIdColumn + "='" + oid.ToString() + "'";
                conn.Open();
                using (SqlCommand command = new SqlCommand(strSQL, conn))
                {
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                                geom = GeometryFromWKB.Parse((byte[]) dr[0]);
                        }
                    }
                }
                conn.Close();
            }
            return geom;
        }

        /// <summary>
        /// Returns geometry Object IDs whose bounding box intersects 'bbox'
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<uint> GetObjectIDsInView(BoundingBox bbox)
        {
            Collection<uint> objectlist = new Collection<uint>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string strSQL = "SELECT * FROM ST.FilterQuery('" + Table + "', '" + GeometryColumn + "', " +
                                BuildEnvelope(bbox) + ")";

                if (!String.IsNullOrEmpty(DefinitionQuery))
                    strSQL += " WHERE " + DefinitionQuery;

                if (!String.IsNullOrEmpty(OrderQuery))
                    strSQL += " ORDER BY " + OrderQuery;

                using (SqlCommand command = new SqlCommand(strSQL, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr[0] != DBNull.Value)
                            {
                                uint ID = (uint) (int) dr[0];
                                objectlist.Add(ID);
                            }
                        }
                    }
                    conn.Close();
                }
            }
            return objectlist;
        }

        /// <summary>
        /// Returns the features that intersects with 'geom'
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            List<Geometry> features = new List<Geometry>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string strGeom;
                if (TargetSRID > 0 && SRID > 0 && SRID != TargetSRID)
                    strGeom = "ST.Transform(ST.GeomFromText('" + geom.AsText() + "'," + TargetSRID.ToString() + ")," +
                              SRID.ToString() + ")";
                else
                    strGeom = "ST.GeomFromText('" + geom.AsText() + "', " + SRID.ToString() + ")";

                string strSQL = "SELECT " + FeatureColumns + ", ST.AsBinary(" + BuildGeometryExpression() +
                                ") As sharpmap_tempgeometry ";
                strSQL += "FROM ST.RelateQuery" + BuildSpatialQuerySuffix() + "(" + strGeom + ", 'intersects')";

                if (!String.IsNullOrEmpty(DefinitionQuery))
                    strSQL += " WHERE " + DefinitionQuery;

                if (!String.IsNullOrEmpty(OrderQuery))
                    strSQL += " ORDER BY " + OrderQuery;

                using (SqlDataAdapter adapter = new SqlDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn &&
                                !col.ColumnName.StartsWith(GeometryColumn + "_Envelope_") &&
                                col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn &&
                                    !col.ColumnName.StartsWith(GeometryColumn + "_Envelope_") &&
                                    col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            if (dr["sharpmap_tempgeometry"] != DBNull.Value)
                                fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"]);
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
        /// <returns>number of features</returns>
        public int GetFeatureCount()
        {
            int count = 0;
            using (SqlConnection conn = new SqlConnection(_ConnectionString))
            {
                string strSQL = "SELECT COUNT(*) FROM " + Table;
                if (!String.IsNullOrEmpty(DefinitionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (SqlCommand command = new SqlCommand(strSQL, conn))
                {
                    conn.Open();
                    count = (int) command.ExecuteScalar();
                    conn.Close();
                }
            }
            return count;
        }

        /// <summary>
        /// Spacial Reference ID
        /// </summary>
        public int SRID
        {
            get
            {
                if (_srid == -2)
                {
                    int dotPos = Table.IndexOf(".");
                    string strSQL = "";
                    if (dotPos == -1)
                        strSQL = "select SRID from ST.GEOMETRY_COLUMNS WHERE F_TABLE_NAME='" + Table + "'";
                    else
                    {
                        string schema = Table.Substring(0, dotPos);
                        string table = Table.Substring(dotPos + 1);
                        strSQL = "select SRID from ST.GEOMETRY_COLUMNS WHERE F_TABLE_SCHEMA='" + schema +
                                 "' AND F_TABLE_NAME='" + table + "'";
                    }

                    using (SqlConnection conn = new SqlConnection(_ConnectionString))
                    {
                        using (SqlCommand command = new SqlCommand(strSQL, conn))
                        {
                            try
                            {
                                conn.Open();
                                _srid = (int) command.ExecuteScalar();
                                conn.Close();
                            }
                            catch
                            {
                                _srid = -1;
                            }
                        }
                    }
                }
                return _srid;
            }
            set
            {
                // SRID can be set in order to support views.
                _srid = value;
            }
        }


        /// <summary>
        /// Returns a datarow based on a RowID
        /// </summary>
        /// <param name="RowID"></param>
        /// <returns>datarow</returns>
        public FeatureDataRow GetFeature(uint RowID)
        {
            using (SqlConnection conn = new SqlConnection(_ConnectionString))
            {
                string strSQL = "select " + FeatureColumns + ", ST.AsBinary(" + BuildGeometryExpression() +
                                ") As sharpmap_tempgeometry from " + Table + " WHERE " + ObjectIdColumn + "='" +
                                RowID.ToString() + "'";
                using (SqlDataAdapter adapter = new SqlDataAdapter(strSQL, conn))
                {
                    FeatureDataSet ds = new FeatureDataSet();
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn &&
                                !col.ColumnName.StartsWith(GeometryColumn + "_Envelope_") &&
                                col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn &&
                                    !col.ColumnName.StartsWith(GeometryColumn + "_Envelope_") &&
                                    col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            if (dr["sharpmap_tempgeometry"] != DBNull.Value)
                                fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"]);
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
            using (SqlConnection conn = new SqlConnection(_ConnectionString))
            {
                string strSQL = string.Format("SELECT ST.AsBinary(ST.EnvelopeQueryWhere('{0}', '{1}', '{2}'))", Table,
                                              GeometryColumn, DefinitionQuery.Replace("'", "''"));
                using (SqlCommand command = new SqlCommand(strSQL, conn))
                {
                    conn.Open();
                    object result = command.ExecuteScalar();
                    conn.Close();
                    if (result == DBNull.Value)
                        return null;
                    BoundingBox bbox = GeometryFromWKB.Parse((byte[]) result).GetBoundingBox();
                    return bbox;
                }
            }
        }

        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        public string ConnectionId
        {
            get { return _ConnectionString; }
        }

        /// <summary>
        /// Returns all features with the view box
        /// </summary>
        /// <param name="bbox">view box</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(BoundingBox bbox, FeatureDataSet ds)
        {
            List<Geometry> features = new List<Geometry>();
            using (SqlConnection conn = new SqlConnection(_ConnectionString))
            {
                string strSQL = "SELECT " + FeatureColumns + ", ST.AsBinary(" + BuildGeometryExpression() +
                                ") AS sharpmap_tempgeometry ";
                strSQL += "FROM ST.FilterQuery" + BuildSpatialQuerySuffix() + "(" + BuildEnvelope(bbox) + ")";

                if (!String.IsNullOrEmpty(DefinitionQuery))
                    strSQL += " WHERE " + DefinitionQuery;

                if (!String.IsNullOrEmpty(OrderQuery))
                    strSQL += " ORDER BY " + OrderQuery;

                using (SqlDataAdapter adapter = new SqlDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    DataSet ds2 = new DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (DataColumn col in ds2.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn &&
                                !col.ColumnName.StartsWith(GeometryColumn + "_Envelope_") &&
                                col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (DataRow dr in ds2.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds2.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn &&
                                    !col.ColumnName.StartsWith(GeometryColumn + "_Envelope_") &&
                                    col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            if (dr["sharpmap_tempgeometry"] != DBNull.Value)
                                fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"]);
                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
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
                    //Close();
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~MsSqlSpatial()
        {
            Dispose();
        }

        #endregion

        /// <summary>
        /// Returns all objects within a distance of a geometry
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        [Obsolete("Use ExecuteIntersectionQuery instead")]
        public FeatureDataTable QueryFeatures(Geometry geom, double distance)
        {
            //List<Geometries.Geometry> features = new List<SharpMap.Geometries.Geometry>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string strGeom;
                if (TargetSRID > 0 && SRID > 0 && SRID != TargetSRID)
                    strGeom = "ST.Transform(ST.GeomFromText('" + geom.AsText() + "'," + TargetSRID.ToString() + ")," +
                              SRID.ToString() + ")";
                else
                    strGeom = "ST.GeomFromText('" + geom.AsText() + "', " + SRID.ToString() + ")";

                string strSQL = "SELECT " + FeatureColumns + ", ST.AsBinary(" + BuildGeometryExpression() +
                                ") As sharpmap_tempgeometry ";
                strSQL += "FROM ST.IsWithinDistanceQuery" + BuildSpatialQuerySuffix() + "(" + strGeom + ", " +
                          distance.ToString(CultureInfo.CurrentCulture) + ")";

                if (!String.IsNullOrEmpty(DefinitionQuery))
                    strSQL += " WHERE " + DefinitionQuery;

                if (!String.IsNullOrEmpty(OrderQuery))
                    strSQL += " ORDER BY " + OrderQuery;

                using (SqlDataAdapter adapter = new SqlDataAdapter(strSQL, conn))
                {
                    DataSet ds = new DataSet();
                    conn.Open();
                    adapter.Fill(ds);
                    conn.Close();
                    if (ds.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds.Tables[0]);
                        foreach (DataColumn col in ds.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn &&
                                !col.ColumnName.StartsWith(GeometryColumn + "_Envelope_") &&
                                col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn &&
                                    !col.ColumnName.StartsWith(GeometryColumn + "_Envelope_") &&
                                    col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            if (dr["sharpmap_tempgeometry"] != DBNull.Value)
                                fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"]);
                            fdt.AddRow(fdr);
                        }
                        return fdt;
                    }
                    else return null;
                }
            }
        }

        /// <summary>
        /// Queries the MsSqlSpatial database to get the name of the Geometry Column. This is used if the columnname isn't specified in the constructor
        /// </summary>
        /// <remarks></remarks>
        /// <returns>Name of column containing geometry</returns>
        private string GetGeometryColumn()
        {
            string strSQL = "select F_GEOMETRY_COLUMN from ST.GEOMETRY_COLUMNS WHERE F_TABLE_NAME='" + Table + "'";
            using (SqlConnection conn = new SqlConnection(_ConnectionString))
            using (SqlCommand command = new SqlCommand(strSQL, conn))
            {
                conn.Open();
                object columnname = command.ExecuteScalar();
                conn.Close();
                if (columnname == DBNull.Value)
                    throw new ApplicationException("Table '" + Table + "' does not contain a geometry column");
                return (string) columnname;
            }
        }

        /// <summary>
        /// Returns all features with the view box
        /// </summary>
        /// <param name="bbox">view box</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        [Obsolete("Use ExecuteIntersectionQuery")]
        public void GetFeaturesInView(BoundingBox bbox, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(bbox, ds);
        }

        private string BuildSpatialQuerySuffix()
        {
            string schema;
            string table = Table;
            int dotPosition = table.IndexOf('.');
            if (dotPosition == -1)
            {
                schema = "dbo";
            }
            else
            {
                schema = table.Substring(0, dotPosition);
                table = table.Substring(dotPosition + 1);
            }
            return "#" + schema + "#" + table + "#" + GeometryColumn;
        }

        private string BuildGeometryExpression()
        {
            return string.Format(GeometryExpression, TargetGeometryColumn);
        }

        private string BuildEnvelope(BoundingBox bbox)
        {
            if (TargetSRID > 0 && SRID > 0 && SRID != TargetSRID)
                return string.Format(CultureInfo.CurrentCulture,
                                     "ST.Transform(ST.MakeEnvelope({0},{1},{2},{3},{4}),{5})",
                                     bbox.Min.X,
                                     bbox.Min.Y,
                                     bbox.Max.X,
                                     bbox.Max.Y,
                                     TargetSRID,
                                     SRID);
            else
                return string.Format(CultureInfo.CurrentCulture,
                                     "ST.MakeEnvelope({0},{1},{2},{3},{4})",
                                     bbox.Min.X,
                                     bbox.Min.Y,
                                     bbox.Max.X,
                                     bbox.Max.Y,
                                     SRID);
        }

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