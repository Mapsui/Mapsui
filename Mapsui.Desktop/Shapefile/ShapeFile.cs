// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Caching;
using Mapsui.Desktop.Shapefile.Indexing;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Desktop.Shapefile
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
        /// <summary>
        /// Filter Delegate Method
        /// </summary>
        /// <remarks>
        /// The FilterMethod delegate is used for applying a method that filters data from the dataset.
        /// The method should return 'true' if the feature should be included and false if not.
        /// </remarks>
        /// <returns>true if this feature should be included, false if it should be filtered</returns>
        public delegate bool FilterMethod(IFeature dr);
        
        private BoundingBox _envelope;
        private int _featureCount;
        private readonly bool _fileBasedIndex;
        private string _filename;
        private bool _isOpen;
        private ShapeType _shapeType;
        private string _crs = "";
        private BinaryReader _brShapeFile;
        private BinaryReader _brShapeIndex;
        private readonly DbaseReader _dbaseFile;
        private FileStream _fsShapeFile;
        private FileStream _fsShapeIndex;
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Tree used for fast query of data
        /// </summary>
        private QuadTree _tree;

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
        public ShapeFile(string filename, bool fileBasedIndex = false)
        {
            _filename = filename;
            _fileBasedIndex = (fileBasedIndex) && File.Exists(Path.ChangeExtension(filename, ".shx"));

            //Initialize DBF
            //string dbffile = _Filename.Substring(0, _Filename.LastIndexOf(".")) + ".dbf";
            string dbffile = Path.ChangeExtension(filename, ".dbf");
            if (File.Exists(dbffile))
                _dbaseFile = new DbaseReader(dbffile);
            //Parse shape header
            ParseHeader();
            //Read projection file
            ParseProjection();
        }

        /// <summary>
        /// Gets the <see cref="Shapefile.ShapeType">shape geometry type</see> in this shapefile.
        /// </summary>
        /// <remarks>
        /// The property isn't set until the first time the datasource has been opened,
        /// and will throw an exception if this property has been called since initialization. 
        /// <para>All the non-Null shapes in a shapefile are required to be of the same shape
        /// type.</para>
        /// </remarks>
        public ShapeType ShapeType
        {
            get { return _shapeType; }
        }


        /// <summary>
        /// Gets or sets the filename of the shapefile
        /// </summary>
        /// <remarks>If the filename changes, indexes will be rebuilt</remarks>
        public string Filename
        {
            get { return _filename; }
            set
            {
                if (value != _filename)
                {
                    lock (_syncRoot) {
                        _filename = value;
                    }
                    if (_isOpen)
                        throw new ApplicationException("Cannot change filename while datasource is open");

                    ParseHeader();
                    ParseProjection();
                    _tree = null;
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
            get { return _dbaseFile.Encoding; }
            set { _dbaseFile.Encoding = value; }
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
        public FilterMethod FilterDelegate { get; set; }

        
        private bool _disposed;

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
            if (!_disposed)
            {
                if (disposing)
                {
                    Close();
                    _envelope = null;
                    _tree = null;
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizes the object
        /// </summary>
        ~ShapeFile()
        {
            Dispose();
        }

        
        
        /// <summary>
        /// Opens the datasource
        /// </summary>
        private void Open()
        {
            // TODO:
            // Get a Connector.  The connector returned is guaranteed to be connected and ready to go.
            // Pooling.Connector connector = Pooling.ConnectorPool.ConnectorPoolManager.RequestConnector(this,true);

            if (!_isOpen)
            {
                _fsShapeIndex = new FileStream(_filename.Remove(_filename.Length - 4, 4) + ".shx", FileMode.Open, FileAccess.Read);
                _brShapeIndex = new BinaryReader(_fsShapeIndex, Encoding.Unicode);
                _fsShapeFile = new FileStream(_filename, FileMode.Open, FileAccess.Read);
                _brShapeFile = new BinaryReader(_fsShapeFile);
                InitializeShape(_filename, _fileBasedIndex);
                if (_dbaseFile != null)
                    _dbaseFile.Open();
                _isOpen = true;
            }
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        private void Close()
        {
            if (!_disposed)
            {
                if (_isOpen)
                {
                    _brShapeFile.Close();
                    _fsShapeFile.Close();
                    _brShapeIndex.Close();
                    _fsShapeIndex.Close();
                    _dbaseFile?.Close();
                    _isOpen = false;
                }
            }
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
            var geometries = new Collection<IGeometry>();

            for (int i = 0; i < objectlist.Count; i++)
            {
                IGeometry g = GetGeometry(objectlist[i]);
                if (g != null) geometries.Add(g);
            }
            return geometries;
        }

        /// <summary>
        /// Returns geometry Object IDs whose bounding box intersects 'bbox'
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<uint> GetObjectIDsInView(BoundingBox bbox)
        {
            if (!_isOpen)
                throw (new ApplicationException("An attempt was made to read from a closed datasource"));
            //Use the spatial index to get a list of features whose boundingbox intersects bbox
            return _tree.Search(bbox);
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
            return _featureCount;
        }

        /// <summary>
        /// Returns the extents of the datasource
        /// </summary>
        /// <returns></returns>
        public BoundingBox GetExtents()
        {
            lock (_syncRoot)
            {
                Open();

                try
                {
                    if (_tree == null)
                        return _envelope;
                    return _tree.Box;
                }
                finally
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Gets or sets the spatial reference ID (CRS)
        /// </summary>
        public string CRS
        {
            get { return _crs; }
            set { _crs = value; }
        }

        
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
            _fsShapeIndex = new FileStream(Path.ChangeExtension(_filename, ".shx"), FileMode.Open,
                                          FileAccess.Read);
            _brShapeIndex = new BinaryReader(_fsShapeIndex, Encoding.Unicode);

            _brShapeIndex.BaseStream.Seek(0, 0);
            //Check file header
            if (_brShapeIndex.ReadInt32() != 170328064)
                //File Code is actually 9994, but in Little Endian Byte Order this is '170328064'
                throw (new ApplicationException("Invalid Shapefile Index (.shx)"));

            _brShapeIndex.BaseStream.Seek(24, 0); //seek to File Length
            int indexFileSize = SwapByteOrder(_brShapeIndex.ReadInt32());
            //Read filelength as big-endian. The length is based on 16bit words
            _featureCount = (2 * indexFileSize - 100) / 8;
            //Calculate FeatureCount. Each feature takes up 8 bytes. The header is 100 bytes

            _brShapeIndex.BaseStream.Seek(32, 0); //seek to ShapeType
            _shapeType = (ShapeType)_brShapeIndex.ReadInt32();

            //Read the spatial bounding box of the contents
            _brShapeIndex.BaseStream.Seek(36, 0); //seek to box
            _envelope = new BoundingBox(_brShapeIndex.ReadDouble(), _brShapeIndex.ReadDouble(), _brShapeIndex.ReadDouble(),
                                        _brShapeIndex.ReadDouble());

            _brShapeIndex.Close();
            _fsShapeIndex.Close();
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
                    // todo: Automatically parse coordinate system: 
                    // var wkt = File.ReadAllText(projfile);
                    // CoordinateSystemWktReader.Parse(wkt);

                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Coordinate system file '" + projfile +
                                       "' found, but could not be parsed. WKT parser returned:" + ex.Message);
                    throw;
                }
            }
        }

        /// <summary>
        /// Reads the record offsets from the .shx index file and returns the information in an array
        /// </summary>
        private int[] ReadIndex()
        {
            var offsetOfRecord = new int[_featureCount];
            _brShapeIndex.BaseStream.Seek(100, 0); //skip the header

            for (int x = 0; x < _featureCount; ++x)
            {
                offsetOfRecord[x] = 2 * SwapByteOrder(_brShapeIndex.ReadInt32()); //Read shape data position // ibuffer);
                _brShapeIndex.BaseStream.Seek(_brShapeIndex.BaseStream.Position + 4, 0); //Skip content length
            }
            return offsetOfRecord;
        }

        /// <summary>
        /// Gets the file position of the n'th shape
        /// </summary>
        /// <param name="n">Shape ID</param>
        /// <returns></returns>
        private int GetShapeIndex(uint n)
        {
            _brShapeIndex.BaseStream.Seek(100 + n * 8, 0); //seek to the position of the index
            return 2 * SwapByteOrder(_brShapeIndex.ReadInt32()); //Read shape data position
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
            }

            QuadTree tree = CreateSpatialIndex();
            tree.SaveIndex(filename + ".sidx");
            return tree;
        }

        /// <summary>
        /// Generates a spatial index for a specified shape file.
        /// </summary>
        private QuadTree CreateSpatialIndex()
        {
            var objList = new List<QuadTree.BoxObjects>();
            //Convert all the geometries to boundingboxes 
            uint i = 0;
            foreach (var box in GetAllFeatureBoundingBoxes())
            {
                if (!double.IsNaN(box.Left) && !double.IsNaN(box.Right) && !double.IsNaN(box.Bottom) &&
                    !double.IsNaN(box.Top))
                {
                    var g = new QuadTree.BoxObjects { Box = box, Id = i };
                    objList.Add(g);
                    i++;
                }
            }

            Heuristic heur;
            heur.Maxdepth = (int)Math.Ceiling(Math.Log(GetFeatureCount(), 2));
            heur.Minerror = 10;
            heur.Tartricnt = 5;
            heur.Mintricnt = 2;
            return new QuadTree(objList, 0, heur);
        }

        private void LoadSpatialIndex(bool loadFromFile)
        {
            LoadSpatialIndex(false, loadFromFile);
        }

        private void LoadSpatialIndex(bool forceRebuild, bool loadFromFile)
        {
            //Only load the tree if we haven't already loaded it, or if we want to force a rebuild
            if (_tree == null || forceRebuild)
            {
                // Is this a web application? If so lets store the index in the cache so we don't
                // need to rebuild it for each request
                if (HttpContext.Current != null)
                {
                    //Check if the tree exists in the cache
                    if (HttpContext.Current.Cache[_filename] != null)
                        _tree = (QuadTree)HttpContext.Current.Cache[_filename];
                    else
                    {
                        _tree = !loadFromFile ? CreateSpatialIndex() : CreateSpatialIndexFromFile(_filename);
                        //Store the tree in the web cache
                        //TODO: Remove this when connection pooling is implemented
                        HttpContext.Current.Cache.Insert(_filename, _tree, null, Cache.NoAbsoluteExpiration,
                                                         TimeSpan.FromDays(1));
                    }
                }
                else if (!loadFromFile)
                    _tree = CreateSpatialIndex();
                else
                    _tree = CreateSpatialIndexFromFile(_filename);
            }
        }

        /// <summary>
        /// Forces a rebuild of the spatial index. If the instance of the ShapeFile provider
        /// uses a file-based index the file is rewritten to disk.
        /// </summary>
        public void RebuildSpatialIndex()
        {
            if (_fileBasedIndex)
            {
                if (File.Exists(_filename + ".sidx"))
                    File.Delete(_filename + ".sidx");
                _tree = CreateSpatialIndexFromFile(_filename);
            }
            else
                _tree = CreateSpatialIndex();
            if (HttpContext.Current != null)
                //TODO: Remove this when connection pooling is implemented:
                HttpContext.Current.Cache.Insert(_filename, _tree, null, Cache.NoAbsoluteExpiration, TimeSpan.FromDays(1));
        }

        /// <summary>
        /// Reads all boundingboxes of features in the shapefile. This is used for spatial indexing.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<BoundingBox> GetAllFeatureBoundingBoxes()
        {
            int[] offsetOfRecord = ReadIndex(); //Read the whole .idx file

            //List<BoundingBox> boxes = new List<BoundingBox>();

            if (_shapeType == ShapeType.Point)
            {
                for (int a = 0; a < _featureCount; ++a)
                {
                    _fsShapeFile.Seek(offsetOfRecord[a] + 8, 0); //skip record number and content length
                    if ((ShapeType)_brShapeFile.ReadInt32() != ShapeType.Null)
                    {
                        double x = _brShapeFile.ReadDouble();
                        double y = _brShapeFile.ReadDouble();
                        //boxes.Add(new BoundingBox(x, y, x, y));
                        yield return new BoundingBox(x, y, x, y);
                    }
                }
            }
            else
            {
                for (int a = 0; a < _featureCount; ++a)
                {
                    _fsShapeFile.Seek(offsetOfRecord[a] + 8, 0); //skip record number and content length
                    if ((ShapeType)_brShapeFile.ReadInt32() != ShapeType.Null)
                        yield return new BoundingBox(_brShapeFile.ReadDouble(), _brShapeFile.ReadDouble(),
                                                     _brShapeFile.ReadDouble(), _brShapeFile.ReadDouble());
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
        // ReSharper disable once CyclomaticComplexity // Fix when changes need to be made here
        private Geometry ReadGeometry(uint oid)
        {
            _brShapeFile.BaseStream.Seek(GetShapeIndex(oid) + 8, 0); //Skip record number and content length
            var type = (ShapeType)_brShapeFile.ReadInt32(); //Shape type
            if (type == ShapeType.Null)
                return null;
            if (_shapeType == ShapeType.Point || _shapeType == ShapeType.PointM || _shapeType == ShapeType.PointZ)
            {
                return new Point(_brShapeFile.ReadDouble(), _brShapeFile.ReadDouble());
            }
            if (_shapeType == ShapeType.Multipoint || _shapeType == ShapeType.MultiPointM ||
                _shapeType == ShapeType.MultiPointZ)
            {
                _brShapeFile.BaseStream.Seek(32 + _brShapeFile.BaseStream.Position, 0); //skip min/max box
                var feature = new MultiPoint();
                int nPoints = _brShapeFile.ReadInt32(); // get the number of points
                if (nPoints == 0)
                    return null;
                for (int i = 0; i < nPoints; i++)
                    feature.Points.Add(new Point(_brShapeFile.ReadDouble(), _brShapeFile.ReadDouble()));

                return feature;
            }
            if (_shapeType == ShapeType.PolyLine || _shapeType == ShapeType.Polygon ||
                _shapeType == ShapeType.PolyLineM || _shapeType == ShapeType.PolygonM ||
                _shapeType == ShapeType.PolyLineZ || _shapeType == ShapeType.PolygonZ)
            {
                _brShapeFile.BaseStream.Seek(32 + _brShapeFile.BaseStream.Position, 0); //skip min/max box

                int nParts = _brShapeFile.ReadInt32(); // get number of parts (segments)
                if (nParts == 0)
                    return null;
                int nPoints = _brShapeFile.ReadInt32(); // get number of points

                var segments = new int[nParts + 1];
                //Read in the segment indexes
                for (int b = 0; b < nParts; b++)
                    segments[b] = _brShapeFile.ReadInt32();
                //add end point
                segments[nParts] = nPoints;

                if ((int)_shapeType % 10 == 3)
                {
                    var mline = new MultiLineString();
                    for (int lineId = 0; lineId < nParts; lineId++)
                    {
                        var line = new LineString();
                        for (int i = segments[lineId]; i < segments[lineId + 1]; i++)
                            line.Vertices.Add(new Point(_brShapeFile.ReadDouble(), _brShapeFile.ReadDouble()));
                        mline.LineStrings.Add(line);
                    }
                    if (mline.LineStrings.Count == 1)
                        return mline[0];
                    return mline;
                }
                else //(_ShapeType == ShapeType.Polygon etc...)
                {
                    //First read all the rings
                    var rings = new List<LinearRing>();
                    for (int ringId = 0; ringId < nParts; ringId++)
                    {
                        var ring = new LinearRing();
                        for (int i = segments[ringId]; i < segments[ringId + 1]; i++)
                            ring.Vertices.Add(new Point(_brShapeFile.ReadDouble(), _brShapeFile.ReadDouble()));
                        rings.Add(ring);
                    }
                    var isCounterClockWise = new bool[rings.Count];
                    int polygonCount = 0;
                    for (int i = 0; i < rings.Count; i++)
                    {
                        isCounterClockWise[i] = rings[i].IsCCW();
                        if (!isCounterClockWise[i])
                            polygonCount++;
                    }
                    if (polygonCount == 1) //We only have one polygon
                    {
                        var poly = new Polygon { ExteriorRing = rings[0] };
                        if (rings.Count > 1)
                            for (int i = 1; i < rings.Count; i++)
                                poly.InteriorRings.Add(rings[i]);
                        return poly;
                    }
                    else
                    {
                        var mpoly = new MultiPolygon();
                        var poly = new Polygon { ExteriorRing = rings[0] };
                        for (var i = 1; i < rings.Count; i++)
                        {
                            if (!isCounterClockWise[i])
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

            throw (new ApplicationException("Shapefile type " + _shapeType.ToString() + " not supported"));
        }

        /// <summary>
        /// Gets a datarow from the datasource at the specified index belonging to the specified datatable
        /// </summary>
        /// <param name="rowId"></param>
        /// <param name="feature">Datatable to feature should belong to.</param>
        /// <returns></returns>
        public IFeature GetFeature(uint rowId, IFeatures feature = null)
        {
            lock (_syncRoot)
            {
                Open();

                try
                {
                    return GetFeaturePrivate(rowId, feature);
                }
                finally
                {
                    Close();
                }
            }

        }

        private IFeature GetFeaturePrivate(uint rowId, IFeatures dt)
        {
            if (_dbaseFile != null)
            {
                var dr = _dbaseFile.GetFeature(rowId, dt ?? new Features());
                dr.Geometry = ReadGeometry(rowId);
                if (FilterDelegate == null || FilterDelegate(dr))
                    return dr;
                return null;
            }
            throw (new ApplicationException("An attempt was made to read DBase data from a shapefile without a valid .DBF file"));
        }

        
        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            lock (_syncRoot)
            {
                Open();
                try
                {
                    //Use the spatial index to get a list of features whose boundingbox intersects bbox
                    var objectlist = GetObjectIDsInView(box);
                    var features = new Features();

                    foreach (var index in objectlist)
                    {
                        var feature = _dbaseFile.GetFeature(index, features);
                        feature.Geometry = ReadGeometry(index);
                        if (feature.Geometry == null) continue;
                        if (!feature.Geometry.BoundingBox.Intersects(box)) continue;
                        if (FilterDelegate != null && !FilterDelegate(feature)) continue;
                        features.Add(feature);
                    }
                    return features;
                }
                finally
                {
                    Close();
                }
            }
        }

            }
}