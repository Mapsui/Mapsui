// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Utilities.SpatialIndexing;
using System.Web.Caching;

namespace Mapsui.Data.Providers
{
    /// <summary>
    /// Shapefile geometry type.
    /// </summary>
    public enum ShapeType
    {
        /// <summary>
        /// Null shape with no geometric data
        /// </summary>
        Null = 0,
        /// <summary>
        /// A point consists of a pair of double-precision coordinates.
        /// Mapsui interpretes this as <see cref="Mapsui.Geometries.Point"/>
        /// </summary>
        Point = 1,
        /// <summary>
        /// PolyLine is an ordered set of vertices that consists of one or more parts. A part is a
        /// connected sequence of two or more points. Parts may or may not be connected to one
        ///	another. Parts may or may not intersect one another.
        /// Mapsui interpretes this as either <see cref="Mapsui.Geometries.LineString"/> or <see cref="Mapsui.Geometries.MultiLineString"/>
        /// </summary>
        PolyLine = 3,
        /// <summary>
        /// A polygon consists of one or more rings. A ring is a connected sequence of four or more
        /// points that form a closed, non-self-intersecting loop. A polygon may contain multiple
        /// outer rings. The order of vertices or orientation for a ring indicates which side of the ring
        /// is the interior of the polygon. The neighborhood to the right of an observer walking along
        /// the ring in vertex order is the neighborhood inside the polygon. Vertices of rings defining
        /// holes in polygons are in a counterclockwise direction. Vertices for a single, ringed
        /// polygon are, therefore, always in clockwise order. The rings of a polygon are referred to
        /// as its parts.
        /// Mapsui interpretes this as either <see cref="Mapsui.Geometries.Polygon"/> or <see cref="Mapsui.Geometries.MultiPolygon"/>
        /// </summary>
        Polygon = 5,
        /// <summary>
        /// A MultiPoint represents a set of points.
        /// Mapsui interpretes this as <see cref="Mapsui.Geometries.MultiPoint"/>
        /// </summary>
        Multipoint = 8,
        /// <summary>
        /// A PointZ consists of a triplet of double-precision coordinates plus a measure.
        /// Mapsui interpretes this as <see cref="Mapsui.Geometries.Point"/>
        /// </summary>
        PointZ = 11,
        /// <summary>
        /// A PolyLineZ consists of one or more parts. A part is a connected sequence of two or
        /// more points. Parts may or may not be connected to one another. Parts may or may not
        /// intersect one another.
        /// Mapsui interpretes this as <see cref="Mapsui.Geometries.LineString"/> or <see cref="Mapsui.Geometries.MultiLineString"/>
        /// </summary>
        PolyLineZ = 13,
        /// <summary>
        /// A PolygonZ consists of a number of rings. A ring is a closed, non-self-intersecting loop.
        /// A PolygonZ may contain multiple outer rings. The rings of a PolygonZ are referred to as
        /// its parts.
        /// Mapsui interpretes this as either <see cref="Mapsui.Geometries.Polygon"/> or <see cref="Mapsui.Geometries.MultiPolygon"/>
        /// </summary>
        PolygonZ = 15,
        /// <summary>
        /// A MultiPointZ represents a set of <see cref="PointZ"/>s.
        /// Mapsui interpretes this as <see cref="Mapsui.Geometries.MultiPoint"/>
        /// </summary>
        MultiPointZ = 18,
        /// <summary>
        /// A PointM consists of a pair of double-precision coordinates in the order X, Y, plus a measure M.
        /// Mapsui interpretes this as <see cref="Mapsui.Geometries.Point"/>
        /// </summary>
        PointM = 21,
        /// <summary>
        /// A shapefile PolyLineM consists of one or more parts. A part is a connected sequence of
        /// two or more points. Parts may or may not be connected to one another. Parts may or may
        /// not intersect one another.
        /// Mapsui interpretes this as <see cref="Mapsui.Geometries.LineString"/> or <see cref="Mapsui.Geometries.MultiLineString"/>
        /// </summary>
        PolyLineM = 23,
        /// <summary>
        /// A PolygonM consists of a number of rings. A ring is a closed, non-self-intersecting loop.
        /// Mapsui interpretes this as either <see cref="Mapsui.Geometries.Polygon"/> or <see cref="Mapsui.Geometries.MultiPolygon"/>
        /// </summary>
        PolygonM = 25,
        /// <summary>
        /// A MultiPointM represents a set of <see cref="PointM"/>s.
        /// Mapsui interpretes this as <see cref="Mapsui.Geometries.MultiPoint"/>
        /// </summary>
        MultiPointM = 28,
        /// <summary>
        /// A MultiPatch consists of a number of surface patches. Each surface patch describes a
        /// surface. The surface patches of a MultiPatch are referred to as its parts, and the type of
        /// part controls how the order of vertices of an MultiPatch part is interpreted.
        /// Mapsui doesn't support this feature type.
        /// </summary>
        MultiPatch = 31
    } ;

    /// <summary>
    /// Shapefile dataprovider
    /// </summary>
    /// <remarks>
    /// <para>The ShapeFile provider is used for accessing ESRI ShapeFiles. The ShapeFile should at least contain the
    /// [filename].shp, [filename].idx, and if feature-data is to be used, also [filename].dbf file.</para>
    /// <para>The first time the ShapeFile is accessed, Mapsui will automatically create a spatial index
    /// of the shp-file, and save it as [filename].shp.sidx. If you change or update the contents of the .shp file,
    /// delete the .sidx file to force Mapsui to rebuilt it. In web applications, the index will automatically
    /// be cached to memory for faster access, so to reload the index, you will need to restart the web application
    /// as well.</para>
    /// <para>
    /// M and Z values in a shapefile is ignored by Mapsui.
    /// </para>
    /// </remarks>
    /// <example>
    /// Adding a datasource to a layer:
    /// <code lang="C#">
    /// Mapsui.Layers.VectorLayer myLayer = new Mapsui.Layers.VectorLayer("My layer");
    /// myLayer.DataSource = new Mapsui.Data.Providers.ShapeFile(@"C:\data\MyShapeData.shp");
    /// </code>
    /// </example>
    public class ShapeFile : IProvider, IDisposable
    {
        #region Delegates

        /// <summary>
        /// Filter Delegate Method
        /// </summary>
        /// <remarks>
        /// The FilterMethod delegate is used for applying a method that filters data from the dataset.
        /// The method should return 'true' if the feature should be included and false if not.
        /// <para>See the <see cref="FilterDelegate"/> property for more info</para>
        /// </remarks>
        /// <seealso cref="FilterDelegate"/>
        /// <param name="dr"><see cref="Mapsui.Data.FeatureDataRow"/> to test on</param>
        /// <returns>true if this feature should be included, false if it should be filtered</returns>
        public delegate bool FilterMethod(IFeature dr);

        #endregion
        
        private BoundingBox _Envelope;
        private int _FeatureCount;
        private bool _FileBasedIndex;
        private string _Filename;
        private FilterMethod _FilterDelegate;
        private bool _IsOpen;
        private ShapeType _ShapeType;
        private int _SRID = -1;
        private BinaryReader brShapeFile;
        private BinaryReader brShapeIndex;
        private DbaseReader dbaseFile;
        private FileStream fsShapeFile;
        private FileStream fsShapeIndex;

        /// <summary>
        /// Tree used for fast query of data
        /// </summary>
        private QuadTree tree;

        /// <summary>
        /// Initializes a ShapeFile DataProvider without a file-based spatial index.
        /// </summary>
        /// <param name="filename">Path to shape file</param>
        public ShapeFile(string filename) : this(filename, false)
        {
        }

        /// <summary>
        /// Initializes a ShapeFile DataProvider.
        /// </summary>
        /// <remarks>
        /// <para>If FileBasedIndex is true, the spatial index will be read from a local copy. If it doesn't exist,
        /// it will be generated and saved to [filename] + '.sidx'.</para>
        /// <para>Using a file-based index is especially recommended for ASP.NET applications which will speed up
        /// start-up time when the cache has been emptied.
        /// </para>
        /// </remarks>
        /// <param name="filename">Path to shape file</param>
        /// <param name="fileBasedIndex">Use file-based spatial index</param>
        public ShapeFile(string filename, bool fileBasedIndex)
        {
            _Filename = filename;
            _FileBasedIndex = (fileBasedIndex) && File.Exists(Path.ChangeExtension(filename, ".shx"));

            //Initialize DBF
            //string dbffile = _Filename.Substring(0, _Filename.LastIndexOf(".")) + ".dbf";
            string dbffile = Path.ChangeExtension(filename, ".dbf");
            if (File.Exists(dbffile))
                dbaseFile = new DbaseReader(dbffile);
            //Parse shape header
            ParseHeader();
            //Read projection file
            ParseProjection();
        }

        /// <summary>
        /// Gets the <see cref="Mapsui.Data.Providers.ShapeType">shape geometry type</see> in this shapefile.
        /// </summary>
        /// <remarks>
        /// The property isn't set until the first time the datasource has been opened,
        /// and will throw an exception if this property has been called since initialization. 
        /// <para>All the non-Null shapes in a shapefile are required to be of the same shape
        /// type.</para>
        /// </remarks>
        public ShapeType ShapeType
        {
            get { return _ShapeType; }
        }


        /// <summary>
        /// Gets or sets the filename of the shapefile
        /// </summary>
        /// <remarks>If the filename changes, indexes will be rebuilt</remarks>
        public string Filename
        {
            get { return _Filename; }
            set
            {
                if (value != _Filename)
                {
                    _Filename = value;
                    if (IsOpen)
                        throw new ApplicationException("Cannot change filename while datasource is open");

                    ParseHeader();
                    ParseProjection();
                    tree = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the encoding used for parsing strings from the DBase DBF file.
        /// </summary>
        /// <remarks>
        /// The DBase default encoding is <see cref="System.Text.Encoding.UTF7"/>.
        /// </remarks>
        public Encoding Encoding
        {
            get { return dbaseFile.Encoding; }
            set { dbaseFile.Encoding = value; }
        }

        /// <summary>
        /// Filter Delegate Method for limiting the datasource
        /// </summary>
        /// <remarks>
        /// <example>
        /// Using an anonymous method for filtering all features where the NAME column starts with S:
        /// <code lang="C#">
        /// myShapeDataSource.FilterDelegate = new Mapsui.Data.Providers.ShapeFile.FilterMethod(delegate(Mapsui.Data.FeatureDataRow row) { return (!row["NAME"].ToString().StartsWith("S")); });
        /// </code>
        /// </example>
        /// <example>
        /// Declaring a delegate method for filtering (multi)polygon-features whose area is larger than 5.
        /// <code>
        /// myShapeDataSource.FilterDelegate = CountryFilter;
        /// [...]
        /// public static bool CountryFilter(Mapsui.Data.FeatureDataRow row)
        /// {
        ///		if(row.Geometry.GetType()==typeof(Mapsui.Geometries.Polygon))
        ///			return ((row.Geometry as Mapsui.Geometries.Polygon).Area>5);
        ///		if (row.Geometry.GetType() == typeof(Mapsui.Geometries.MultiPolygon))
        ///			return ((row.Geometry as Mapsui.Geometries.MultiPolygon).Area > 5);
        ///		else return true;
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        /// <seealso cref="FilterMethod"/>
        public FilterMethod FilterDelegate
        {
            get { return _FilterDelegate; }
            set { _FilterDelegate = value; }
        }

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

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Close();
                    _Envelope = null;
                    tree = null;
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Finalizes the object
        /// </summary>
        ~ShapeFile()
        {
            Dispose();
        }

        #endregion

        #region IProvider Members

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public void Open()
        {
            // TODO:
            // Get a Connector.  The connector returned is guaranteed to be connected and ready to go.
            // Pooling.Connector connector = Pooling.ConnectorPool.ConnectorPoolManager.RequestConnector(this,true);

            if (!_IsOpen)
            {
                fsShapeIndex = new FileStream(_Filename.Remove(_Filename.Length - 4, 4) + ".shx", FileMode.Open,
                                              FileAccess.Read);
                brShapeIndex = new BinaryReader(fsShapeIndex, Encoding.Unicode);
                fsShapeFile = new FileStream(_Filename, FileMode.Open, FileAccess.Read);
                brShapeFile = new BinaryReader(fsShapeFile);
                InitializeShape(_Filename, _FileBasedIndex);
                if (dbaseFile != null)
                    dbaseFile.Open();
                _IsOpen = true;
            }
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            if (!disposed)
            {
                //TODO: (ConnectionPooling)
                /*	if (connector != null)
					{ Pooling.ConnectorPool.ConnectorPoolManager.Release...()
				}*/
                if (_IsOpen)
                {
                    brShapeFile.Close();
                    fsShapeFile.Close();
                    brShapeIndex.Close();
                    fsShapeIndex.Close();
                    if (dbaseFile != null)
                        dbaseFile.Close();
                    _IsOpen = false;
                }
            }
        }

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>		
        public bool IsOpen
        {
            get { return _IsOpen; }
        }

        /// <summary>
        /// Returns geometries whose bounding box intersects 'bbox'
        /// </summary>
        /// <remarks>
        /// <para>Please note that this method doesn't guarantee that the geometries returned actually intersect 'bbox', but only
        /// that their boundingbox intersects 'bbox'.</para>
        /// <para>This method is much faster than the QueryFeatures method, because intersection tests
        /// are performed on objects simplifed by their boundingbox, and using the Spatial Index.</para>
        /// </remarks>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<IGeometry> GetGeometriesInView(BoundingBox bbox)
        {
            //Use the spatial index to get a list of features whose boundingbox intersects bbox
            Collection<uint> objectlist = GetObjectIDsInView(bbox);
            if (objectlist.Count == 0) //no features found. Return an empty set
                return new Collection<IGeometry>();

            //Collection<Mapsui.Geometries.Geometry> geometries = new Collection<Mapsui.Geometries.Geometry>(objectlist.Count);
            Collection<IGeometry> geometries = new Collection<IGeometry>();

            for (int i = 0; i < objectlist.Count; i++)
            {
                IGeometry g = GetGeometry(objectlist[i]);
                if (g != null)
                    geometries.Add(g);
            }
            return geometries;
        }

        /// <summary>
        /// Returns all objects whose boundingbox intersects bbox.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Please note that this method doesn't guarantee that the geometries returned actually intersect 'bbox', but only
        /// that their boundingbox intersects 'bbox'.
        /// </para>
        /// </remarks>
        /// <param name="bbox"></param>
        /// <param name="ds"></param>
        /// <returns></returns>
        //public void ExecuteIntersectionQuery(BoundingBox bbox, FeatureDataSet ds)
        //{
        //    //Use the spatial index to get a list of features whose boundingbox intersects bbox
        //    Collection<uint> objectlist = GetObjectIDsInView(bbox);
        //    FeatureDataTable dt = dbaseFile.NewTable;

        //    for (int i = 0; i < objectlist.Count; i++)
        //    {
        //        FeatureDataRow fdr = GetFeature(objectlist[i], dt);
        //        if ( fdr != null ) dt.AddRow(fdr);

        //        /*
        //        //This is triple effort since 
        //        //- Bounding Boxes are checked by GetObjectIdsInView,
        //        //- FilterDelegate is evaluated in GetFeature
        //        FeatureDataRow fdr = dbaseFile.GetFeature(objectlist[i], dt);
        //        fdr.Geometry = ReadGeometry(objectlist[i]);
        //        if (fdr.Geometry != null)
        //            if (fdr.Geometry.GetBoundingBox().Intersects(bbox))
        //                if (FilterDelegate == null || FilterDelegate(fdr))
        //                    dt.AddRow(fdr);
        //         */
        //    }
        //    ds.Tables.Add(dt);
        //}

        /// <summary>
        /// Returns geometry Object IDs whose bounding box intersects 'bbox'
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<uint> GetObjectIDsInView(BoundingBox bbox)
        {
            if (!IsOpen)
                throw (new ApplicationException("An attempt was made to read from a closed datasource"));
            //Use the spatial index to get a list of features whose boundingbox intersects bbox
            return tree.Search(bbox);
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public IGeometry GetGeometry(uint oid)
        {
            if (FilterDelegate != null) //Apply filtering
            {
                IFeature fdr = GetFeature(oid);
                if (fdr != null)
                    return fdr.Geometry;
                return null;
            }
            
            return ReadGeometry(oid);
        }

        /// <summary>
        /// Returns the total number of features in the datasource (without any filter applied)
        /// </summary>
        /// <returns></returns>
        public int GetFeatureCount()
        {
            return _FeatureCount;
        }

        /// <summary>
        /// Returns the extents of the datasource
        /// </summary>
        /// <returns></returns>
        public BoundingBox GetExtents()
        {
            if (tree == null)
                return _Envelope;
            return tree.Box;
        }

        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        /// <remarks>
        /// The connection ID of a shapefile is its filename
        /// </remarks>
        public string ConnectionId
        {
            get { return _Filename; }
        }

        /// <summary>
        /// Gets or sets the spatial reference ID (CRS)
        /// </summary>
        public int SRID
        {
            get { return _SRID; }
            set { _SRID = value; }
        }

        #endregion

        private void InitializeShape(string filename, bool fileBasedIndex)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException(String.Format("Could not find file \"{0}\"", filename));
            if (!filename.ToLower().EndsWith(".shp"))
                throw (new Exception("Invalid shapefile filename: " + filename));

            LoadSpatialIndex(fileBasedIndex); //Load spatial index			
        }

        /// <summary>
        /// Reads and parses the header of the .shx index file
        /// </summary>
        private void ParseHeader()
        {
            fsShapeIndex = new FileStream(Path.ChangeExtension(_Filename, ".shx"), FileMode.Open,
                                          FileAccess.Read);
            brShapeIndex = new BinaryReader(fsShapeIndex, Encoding.Unicode);

            brShapeIndex.BaseStream.Seek(0, 0);
            //Check file header
            if (brShapeIndex.ReadInt32() != 170328064)
                //File Code is actually 9994, but in Little Endian Byte Order this is '170328064'
                throw (new ApplicationException("Invalid Shapefile Index (.shx)"));

            brShapeIndex.BaseStream.Seek(24, 0); //seek to File Length
            int IndexFileSize = SwapByteOrder(brShapeIndex.ReadInt32());
                //Read filelength as big-endian. The length is based on 16bit words
            _FeatureCount = (2*IndexFileSize - 100)/8;
                //Calculate FeatureCount. Each feature takes up 8 bytes. The header is 100 bytes

            brShapeIndex.BaseStream.Seek(32, 0); //seek to ShapeType
            _ShapeType = (ShapeType) brShapeIndex.ReadInt32();

            //Read the spatial bounding box of the contents
            brShapeIndex.BaseStream.Seek(36, 0); //seek to box
            _Envelope = new BoundingBox(brShapeIndex.ReadDouble(), brShapeIndex.ReadDouble(), brShapeIndex.ReadDouble(),
                                        brShapeIndex.ReadDouble());

            brShapeIndex.Close();
            fsShapeIndex.Close();
        }

        /// <summary>
        /// Reads and parses the projection if a projection file exists
        /// </summary>
        private void ParseProjection()
        {
            string projfile = Path.GetDirectoryName(Filename) + "\\" + Path.GetFileNameWithoutExtension(Filename) +
                              ".prj";
            if (File.Exists(projfile))
            {
                try
                {
                    string wkt = File.ReadAllText(projfile);
                    //TODO: Automatically parse coordinate system: CoordinateSystemWktReader.Parse(wkt);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Coordinate system file '" + projfile +
                                       "' found, but could not be parsed. WKT parser returned:" + ex.Message);
                    throw (ex);
                }
            }
        }

        /// <summary>
        /// Reads the record offsets from the .shx index file and returns the information in an array
        /// </summary>
        private int[] ReadIndex()
        {
            int[] OffsetOfRecord = new int[_FeatureCount];
            brShapeIndex.BaseStream.Seek(100, 0); //skip the header

            for (int x = 0; x < _FeatureCount; ++x)
            {
                OffsetOfRecord[x] = 2*SwapByteOrder(brShapeIndex.ReadInt32()); //Read shape data position // ibuffer);
                brShapeIndex.BaseStream.Seek(brShapeIndex.BaseStream.Position + 4, 0); //Skip content length
            }
            return OffsetOfRecord;
        }

        /// <summary>
        /// Gets the file position of the n'th shape
        /// </summary>
        /// <param name="n">Shape ID</param>
        /// <returns></returns>
        private int GetShapeIndex(uint n)
        {
            brShapeIndex.BaseStream.Seek(100 + n*8, 0); //seek to the position of the index
            return 2*SwapByteOrder(brShapeIndex.ReadInt32()); //Read shape data position
        }

        ///<summary>
        ///Swaps the byte order of an int32
        ///</summary>
        /// <param name="i">Integer to swap</param>
        /// <returns>Byte Order swapped int32</returns>
        private int SwapByteOrder(int i)
        {
            byte[] buffer = BitConverter.GetBytes(i);
            Array.Reverse(buffer, 0, buffer.Length);
            return BitConverter.ToInt32(buffer, 0);
        }

        /// <summary>
        /// Loads a spatial index from a file. If it doesn't exist, one is created and saved
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>QuadTree index</returns>
        private QuadTree CreateSpatialIndexFromFile(string filename)
        {
            if (File.Exists(filename + ".sidx"))
            {
                try
                {
                    return QuadTree.FromFile(filename + ".sidx");
                }
                catch (QuadTree.ObsoleteFileFormatException)
                {
                    File.Delete(filename + ".sidx");
                    return CreateSpatialIndexFromFile(filename);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                QuadTree tree = CreateSpatialIndex(_Filename);
                tree.SaveIndex(filename + ".sidx");
                return tree;
            }
        }

        /// <summary>
        /// Generates a spatial index for a specified shape file.
        /// </summary>
        /// <param name="filename"></param>
        private QuadTree CreateSpatialIndex(string filename)
        {
            List<QuadTree.BoxObjects> objList = new List<QuadTree.BoxObjects>();
            //Convert all the geometries to boundingboxes 
            uint i = 0;
            foreach (BoundingBox box in GetAllFeatureBoundingBoxes())
            {
                if (!double.IsNaN(box.Left) && !double.IsNaN(box.Right) && !double.IsNaN(box.Bottom) &&
                    !double.IsNaN(box.Top))
                {
                    QuadTree.BoxObjects g = new QuadTree.BoxObjects();
                    g.box = box;
                    g.ID = i;
                    objList.Add(g);
                    i++;
                }
            }

            Heuristic heur;
            heur.maxdepth = (int) Math.Ceiling(Math.Log(GetFeatureCount(), 2));
            heur.minerror = 10;
            heur.tartricnt = 5;
            heur.mintricnt = 2;
            return new QuadTree(objList, 0, heur);
        }

        private void LoadSpatialIndex()
        {
            LoadSpatialIndex(false, false);
        }

        private void LoadSpatialIndex(bool LoadFromFile)
        {
            LoadSpatialIndex(false, LoadFromFile);
        }

        private void LoadSpatialIndex(bool ForceRebuild, bool LoadFromFile)
        {
            //Only load the tree if we haven't already loaded it, or if we want to force a rebuild
            if (tree == null || ForceRebuild)
            {
                // Is this a web application? If so lets store the index in the cache so we don't
                // need to rebuild it for each request
                if (HttpContext.Current != null)
                {
                    //Check if the tree exists in the cache
                    if (HttpContext.Current.Cache[_Filename] != null)
                        tree = (QuadTree) HttpContext.Current.Cache[_Filename];
                    else
                    {
                        if (!LoadFromFile)
                            tree = CreateSpatialIndex(_Filename);
                        else
                            tree = CreateSpatialIndexFromFile(_Filename);
                        //Store the tree in the web cache
                        //TODO: Remove this when connection pooling is implemented
                        HttpContext.Current.Cache.Insert(_Filename, tree, null, Cache.NoAbsoluteExpiration,
                                                         TimeSpan.FromDays(1));
                    }
                }
                else if (!LoadFromFile)
                    tree = CreateSpatialIndex(_Filename);
                else
                    tree = CreateSpatialIndexFromFile(_Filename);
            }
        }

        /// <summary>
        /// Forces a rebuild of the spatial index. If the instance of the ShapeFile provider
        /// uses a file-based index the file is rewritten to disk.
        /// </summary>
        public void RebuildSpatialIndex()
        {
            if (_FileBasedIndex)
            {
                if (File.Exists(_Filename + ".sidx"))
                    File.Delete(_Filename + ".sidx");
                tree = CreateSpatialIndexFromFile(_Filename);
            }
            else
                tree = CreateSpatialIndex(_Filename);
            if (HttpContext.Current != null)
                //TODO: Remove this when connection pooling is implemented:
                HttpContext.Current.Cache.Insert(_Filename, tree, null, Cache.NoAbsoluteExpiration, TimeSpan.FromDays(1));
        }

        /// <summary>
        /// Reads all boundingboxes of features in the shapefile. This is used for spatial indexing.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<BoundingBox> GetAllFeatureBoundingBoxes()
        {
            int[] offsetOfRecord = ReadIndex(); //Read the whole .idx file

            //List<BoundingBox> boxes = new List<BoundingBox>();

            if (_ShapeType == ShapeType.Point)
            {
                for (int a = 0; a < _FeatureCount; ++a)
                {
                    fsShapeFile.Seek(offsetOfRecord[a] + 8, 0); //skip record number and content length
                    if ((ShapeType) brShapeFile.ReadInt32() != ShapeType.Null)
                    {
                        double x = brShapeFile.ReadDouble();
                        double y = brShapeFile.ReadDouble();
                        //boxes.Add(new BoundingBox(x, y, x, y));
                        yield return new BoundingBox(x, y, x, y);
                    }
                }
            }
            else
            {
                for (int a = 0; a < _FeatureCount; ++a)
                {
                    fsShapeFile.Seek(offsetOfRecord[a] + 8, 0); //skip record number and content length
                    if ((ShapeType)brShapeFile.ReadInt32() != ShapeType.Null)
                        yield return new BoundingBox(brShapeFile.ReadDouble(), brShapeFile.ReadDouble(),
                                                     brShapeFile.ReadDouble(), brShapeFile.ReadDouble());
                        //boxes.Add(new BoundingBox(brShapeFile.ReadDouble(), brShapeFile.ReadDouble(),
                        //                          brShapeFile.ReadDouble(), brShapeFile.ReadDouble()));
                }
            }
            //return boxes;
        }

        /// <summary>
        /// Reads and parses the geometry with ID 'oid' from the ShapeFile
        /// </summary>
        /// <remarks><see cref="FilterDelegate">Filtering</see> is not applied to this method</remarks>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        private Geometry ReadGeometry(uint oid)
        {
            brShapeFile.BaseStream.Seek(GetShapeIndex(oid) + 8, 0); //Skip record number and content length
            ShapeType type = (ShapeType) brShapeFile.ReadInt32(); //Shape type
            if (type == ShapeType.Null)
                return null;
            if (_ShapeType == ShapeType.Point || _ShapeType == ShapeType.PointM || _ShapeType == ShapeType.PointZ)
            {
                Point tempFeature = new Point();
                return new Point(brShapeFile.ReadDouble(), brShapeFile.ReadDouble());
            }
            else if (_ShapeType == ShapeType.Multipoint || _ShapeType == ShapeType.MultiPointM ||
                     _ShapeType == ShapeType.MultiPointZ)
            {
                brShapeFile.BaseStream.Seek(32 + brShapeFile.BaseStream.Position, 0); //skip min/max box
                MultiPoint feature = new MultiPoint();
                int nPoints = brShapeFile.ReadInt32(); // get the number of points
                if (nPoints == 0)
                    return null;
                for (int i = 0; i < nPoints; i++)
                    feature.Points.Add(new Point(brShapeFile.ReadDouble(), brShapeFile.ReadDouble()));

                return feature;
            }
            else if (_ShapeType == ShapeType.PolyLine || _ShapeType == ShapeType.Polygon ||
                     _ShapeType == ShapeType.PolyLineM || _ShapeType == ShapeType.PolygonM ||
                     _ShapeType == ShapeType.PolyLineZ || _ShapeType == ShapeType.PolygonZ)
            {
                brShapeFile.BaseStream.Seek(32 + brShapeFile.BaseStream.Position, 0); //skip min/max box

                int nParts = brShapeFile.ReadInt32(); // get number of parts (segments)
                if (nParts == 0)
                    return null;
                int nPoints = brShapeFile.ReadInt32(); // get number of points

                int[] segments = new int[nParts + 1];
                //Read in the segment indexes
                for (int b = 0; b < nParts; b++)
                    segments[b] = brShapeFile.ReadInt32();
                //add end point
                segments[nParts] = nPoints;

                if ((int) _ShapeType%10 == 3)
                {
                    MultiLineString mline = new MultiLineString();
                    for (int LineID = 0; LineID < nParts; LineID++)
                    {
                        LineString line = new LineString();
                        for (int i = segments[LineID]; i < segments[LineID + 1]; i++)
                            line.Vertices.Add(new Point(brShapeFile.ReadDouble(), brShapeFile.ReadDouble()));
                        mline.LineStrings.Add(line);
                    }
                    if (mline.LineStrings.Count == 1)
                        return mline[0];
                    return mline;
                }
                else //(_ShapeType == ShapeType.Polygon etc...)
                {
                    //First read all the rings
                    List<LinearRing> rings = new List<LinearRing>();
                    for (int RingID = 0; RingID < nParts; RingID++)
                    {
                        LinearRing ring = new LinearRing();
                        for (int i = segments[RingID]; i < segments[RingID + 1]; i++)
                            ring.Vertices.Add(new Point(brShapeFile.ReadDouble(), brShapeFile.ReadDouble()));
                        rings.Add(ring);
                    }
                    bool[] IsCounterClockWise = new bool[rings.Count];
                    int PolygonCount = 0;
                    for (int i = 0; i < rings.Count; i++)
                    {
                        IsCounterClockWise[i] = rings[i].IsCCW();
                        if (!IsCounterClockWise[i])
                            PolygonCount++;
                    }
                    if (PolygonCount == 1) //We only have one polygon
                    {
                        Polygon poly = new Polygon();
                        poly.ExteriorRing = rings[0];
                        if (rings.Count > 1)
                            for (int i = 1; i < rings.Count; i++)
                                poly.InteriorRings.Add(rings[i]);
                        return poly;
                    }
                    else
                    {
                        MultiPolygon mpoly = new MultiPolygon();
                        Polygon poly = new Polygon();
                        poly.ExteriorRing = rings[0];
                        for (int i = 1; i < rings.Count; i++)
                        {
                            if (!IsCounterClockWise[i])
                            {
                                mpoly.Polygons.Add(poly);
                                poly = new Polygon(rings[i]);
                            }
                            else
                                poly.InteriorRings.Add(rings[i]);
                        }
                        mpoly.Polygons.Add(poly);
                        return mpoly;
                    }
                }
            }
            else
                throw (new ApplicationException("Shapefile type " + _ShapeType.ToString() + " not supported"));
        }

        /// <summary>
        /// Gets a datarow from the datasource at the specified index
        /// </summary>
        /// <param name="RowID"></param>
        /// <returns></returns>
        public IFeature GetFeature(uint RowID)
        {
            return GetFeature(RowID, null);
        }

        /// <summary>
        /// Gets a datarow from the datasource at the specified index belonging to the specified datatable
        /// </summary>
        /// <param name="RowID"></param>
        /// <param name="dt">Datatable to feature should belong to.</param>
        /// <returns></returns>
        public IFeature GetFeature(uint RowID, IFeatures dt)
        {
            if (dbaseFile != null)
            {
                IFeature dr = (IFeature)dbaseFile.GetFeature(RowID, (dt == null) ? new Features() : dt);
                dr.Geometry = ReadGeometry(RowID);
                if (FilterDelegate == null || FilterDelegate(dr))
                    return dr;
                else
                    return null;
            }
            else
                throw (new ApplicationException("An attempt was made to read DBase data from a shapefile without a valid .DBF file"));
        }

        #region IProvider Members


        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            //Use the spatial index to get a list of features whose boundingbox intersects bbox
            Collection<uint> objectlist = GetObjectIDsInView(box);
            IFeatures features = new Features();

            foreach (uint index in objectlist)
            {
                IFeature feature = dbaseFile.GetFeature(index, features);
                feature.Geometry = ReadGeometry(index);
                if (feature.Geometry != null)
                    if (feature.Geometry.GetBoundingBox().Intersects(box))
                        if (FilterDelegate == null || FilterDelegate(feature))
                            features.Add(feature);
            }
            return features;
        }


        #endregion
    }
}