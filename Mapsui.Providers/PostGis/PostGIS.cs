/*
 * Created by SharpDevelop.
 * User: TiberiuMihai
 * Date: 3/22/2011
 * Time: 9:07 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using SharpMap.Converters.WellKnownBinary;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Geometries;

namespace SharpMap.Providers
{
	/// <summary>
	/// Description of PostGIS.
	/// </summary>
	public class PostGIS : IProvider
	{
		
		#region Fields
		
		private string _ConnectionString;
        private string _defintionQuery;
        private string _GeometryColumn;
        private bool _IsOpen;
        private string _ObjectIdColumn;
        private string _Schema = "public";
        private int _srid = -2;
        private string _Table;
        private IFeatures _Features;
        
        #endregion
        
        #region Constructors
		
		/// <summary>
        /// Initializes a new connection to PostGIS
        /// </summary>
        /// <param name="ConnectionStr">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="geometryColumnName">Name of geometry column</param>
        /// /// <param name="OID_ColumnName">Name of column with unique identifier</param>
        public PostGIS(string ConnectionStr, string tablename, string geometryColumnName, string OID_ColumnName)
        {
            ConnectionString = ConnectionStr;
            Table = tablename;
            GeometryColumn = geometryColumnName;
            ObjectIdColumn = OID_ColumnName;
        }

        /// <summary>
        /// Initializes a new connection to PostGIS
        /// </summary>
        /// <param name="ConnectionStr">Connectionstring</param>
        /// <param name="tablename">Name of data table</param>
        /// <param name="OID_ColumnName">Name of column with unique identifier</param>
        public PostGIS(string ConnectionStr, string tablename, string OID_ColumnName)
            : this(ConnectionStr, tablename, "", OID_ColumnName)
        {
            GeometryColumn = GetGeometryColumn();
        }
        
        #endregion
        
        #region Properties
        
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
            set
            {
                _Table = value;
                qualifyTable();
            }
        }

        /// <summary>
        /// Schema Name
        /// </summary>
        public string Schema
        {
            get { return _Schema; }
            set { _Schema = value; }
        }

        /// <summary>
        /// Qualified Table Name
        /// </summary>
        public string QualifiedTable
        {
            get { return string.Format("\"{0}\".\"{1}\"", _Schema, _Table); }
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
        /// Name of column that contains the Object ID
        /// </summary>
        public string ObjectIdColumn
        {
            get { return _ObjectIdColumn; }
            set { _ObjectIdColumn = value; }
        }

        /// <summary>
        /// Definition query used for limiting dataset
        /// </summary>
        public string DefinitionQuery
        {
            get { return _defintionQuery; }
            set { _defintionQuery = value; }
        }
        
        /// <summary>
        /// Gets or sets the geometries this datasource contains
        /// </summary>
        public IFeatures Features
        {
            get { return _Features; }
            set { _Features = value; }
        }
        
        #endregion
        
        
        #region IProvider Members
        
        /// <summary>
        /// Returns all features with the view box
        /// </summary>
        /// <param name="bbox">view box</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(BoundingBox bbox, FeatureDataSet ds)
        {
            //List<Geometry> features = new List<Geometry>();
            using (NpgsqlConnection conn = new NpgsqlConnection(_ConnectionString))
            {
                string strBbox = "box2d('BOX3D(" +
                                 bbox.Min.X.ToString() + " " +
                                 bbox.Min.Y.ToString() + "," +
                                 bbox.Max.X.ToString() + " " +
                                 bbox.Max.Y.ToString() + ")'::box3d)";
                if (SRID > 0)
                    strBbox = "setSRID(" + strBbox + "," + SRID.ToString() + ")";

                string strSQL = "SELECT *, AsBinary(\"" + GeometryColumn + "\") AS sharpmap_tempgeometry ";
                strSQL += "FROM " + QualifiedTable + " WHERE ";

                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += DefinitionQuery + " AND ";

                strSQL += "\"" + GeometryColumn + "\" && " + strBbox;
                
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(strSQL, conn))
                {
                    conn.Open();
                    DataSet ds2 = new DataSet();
                    adapter.Fill(ds2);
                    conn.Close();
                    if (ds2.Tables.Count > 0)
                    {
                        FeatureDataTable fdt = new FeatureDataTable(ds2.Tables[0]);
                        foreach (DataColumn col in ds2.Tables[0].Columns)
                            if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                fdt.Columns.Add(col.ColumnName, col.DataType, col.Expression);

                        foreach (DataRow dr in ds2.Tables[0].Rows)
                        {
                            FeatureDataRow fdr = fdt.NewRow();
                            foreach (DataColumn col in ds2.Tables[0].Columns)
                                if (col.ColumnName != GeometryColumn && col.ColumnName != "sharpmap_tempgeometry")
                                    fdr[col.ColumnName] = dr[col];
                            fdr.Geometry = GeometryFromWKB.Parse((byte[]) dr["sharpmap_tempgeometry"]);
                            fdt.AddRow(fdr);
                        }
                        ds.Tables.Add(fdt);
                    }
                }
            }
        }
        
        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
        	IFeatures features = new Features();
        	FeatureDataSet ds = new FeatureDataSet();
        	
        	ExecuteIntersectionQuery(box, ds);
        	
        	foreach (FeatureDataTable table in ds.Tables)
            {
                foreach (FeatureDataRow row in table)
                {
                    IFeature feature = features.New();
                    feature.Geometry = row.Geometry;
                    foreach (DataColumn column in table.Columns)
                        feature[column.ColumnName] = row[column.ColumnName];

                    features.Add(feature);
                }
            }
        	
        	return features;
        }

        /// <summary>
        /// Boundingbox of dataset
        /// </summary>
        /// <returns>boundingbox</returns>
        public BoundingBox GetExtents()
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(_ConnectionString))
            {
                string strSQL = "SELECT EXTENT(\"" + GeometryColumn + "\") FROM " + QualifiedTable;
                if (!String.IsNullOrEmpty(_defintionQuery))
                    strSQL += " WHERE " + DefinitionQuery;
                using (NpgsqlCommand command = new NpgsqlCommand(strSQL, conn))
                {
                    conn.Open();
                    object result = command.ExecuteScalar();
                    conn.Close();
                    if (result == DBNull.Value)
                        return null;
                    string strBox = (string) result;
                    if (strBox.StartsWith("BOX("))
                    {
                        string[] vals = strBox.Substring(4, strBox.IndexOf(")") - 4).Split(new char[2] {',', ' '});
                        return new BoundingBox(
                            double.Parse(vals[0]),
                            double.Parse(vals[1]),
                            double.Parse(vals[2]),
                            double.Parse(vals[3]));
                    }
                    else
                        return null;
                }
            }
        }
        
        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        /// <remarks>
        /// <para>The ConnectionID should be unique to the datasource (for instance the filename or the
        /// connectionstring), and is meant to be used for connection pooling.</para>
        /// <para>If connection pooling doesn't apply to this datasource, the ConnectionID should return String.Empty</para>
        /// </remarks>
        public string ConnectionId
        {
            get { return _ConnectionString; }
        }
        
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
            //Don't really do anything. npgsql's ConnectionPooling takes over here
            _IsOpen = true;
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            //Don't really do anything. npgsql's ConnectionPooling takes over here
            _IsOpen = false;
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
                    string strSQL = "select srid from geometry_columns WHERE f_table_schema='" + _Schema +
                                    "' AND f_table_name='" + _Table + "'";

                    using (NpgsqlConnection conn = new NpgsqlConnection(_ConnectionString))
                    {
                        using (NpgsqlCommand command = new NpgsqlCommand(strSQL, conn))
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
            set { throw (new ApplicationException("Spatial Reference ID cannot by set on a PostGIS table")); }
        }
        
        #endregion
        
        #region Disposers and finalizers

        private bool disposed;

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
        ~PostGIS()
        {
            Dispose();
        }

        #endregion
        
        private void qualifyTable()
        {
            int dotPos = _Table.IndexOf(".");
            if (dotPos == -1)
            {
                _Schema = "public";
            }
            else
            {
                _Schema = _Table.Substring(0, dotPos);
                _Schema = _Schema.Replace('"', ' ').Trim();
            }
            _Table = _Table.Substring(dotPos + 1);
            _Table = _Table.Replace('"', ' ').Trim();
        }
        
        /// <summary>
        /// Queries the PostGIS database to get the name of the Geometry Column. This is used if the columnname isn't specified in the constructor
        /// </summary>
        /// <remarks></remarks>
        /// <returns>Name of column containing geometry</returns>
        private string GetGeometryColumn()
        {
            string strSQL = "SELECT f_geometry_column from geometry_columns WHERE f_table_schema='" + _Schema +
                            "' and f_table_name='" + _Table + "'";
            using (NpgsqlConnection conn = new NpgsqlConnection(_ConnectionString))
            using (NpgsqlCommand command = new NpgsqlCommand(strSQL, conn))
            {
                conn.Open();
                object columnname = command.ExecuteScalar();
                conn.Close();
                if (columnname == DBNull.Value)
                    throw new ApplicationException("Table '" + Table + "' does not contain a geometry column");
                return (string) columnname;
            }
        }
	}
}
