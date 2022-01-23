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
using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.GeometryLayers;
using Mapsui.Layers;
using Mapsui.Providers.Shapefile.Indexing;

namespace Mapsui.Providers.Shapefile
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
        /// Mapsui interprets this as <see cref="Mapsui.Geometries.Point"/>
        /// </summary>
        Point = 1,
        /// <summary>
        /// PolyLine is an ordered set of vertices that consists of one or more parts. A part is a
        /// connected sequence of two or more points. Parts may or may not be connected to one
        ///	another. Parts may or may not intersect one another.
        /// Mapsui interprets this as either <see cref="Mapsui.Geometries.LineString"/> or <see cref="Mapsui.Geometries.MultiLineString"/>
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
        /// Mapsui interprets this as either <see cref="Mapsui.Geometries.Polygon"/> or <see cref="Mapsui.Geometries.MultiPolygon"/>
        /// </summary>
        Polygon = 5,
        /// <summary>
        /// A MultiPoint represents a set of points.
        /// Mapsui interprets this as <see cref="Mapsui.Geometries.MultiPoint"/>
        /// </summary>
        Multipoint = 8,
        /// <summary>
        /// A PointZ consists of a triplet of double-precision coordinates plus a measure.
        /// Mapsui interprets this as <see cref="Mapsui.Geometries.Point"/>
        /// </summary>
        PointZ = 11,
        /// <summary>
        /// A PolyLineZ consists of one or more parts. A part is a connected sequence of two or
        /// more points. Parts may or may not be connected to one another. Parts may or may not
        /// intersect one another.
        /// Mapsui interprets this as <see cref="Mapsui.Geometries.LineString"/> or <see cref="Mapsui.Geometries.MultiLineString"/>
        /// </summary>
        PolyLineZ = 13,
        /// <summary>
        /// A PolygonZ consists of a number of rings. A ring is a closed, non-self-intersecting loop.
        /// A PolygonZ may contain multiple outer rings. The rings of a PolygonZ are referred to as
        /// its parts.
        /// Mapsui interprets this as either <see cref="Mapsui.Geometries.Polygon"/> or <see cref="Mapsui.Geometries.MultiPolygon"/>
        /// </summary>
        PolygonZ = 15,
        /// <summary>
        /// A MultiPointZ represents a set of <see cref="PointZ"/>s.
        /// Mapsui interprets this as <see cref="Mapsui.Geometries.MultiPoint"/>
        /// </summary>
        MultiPointZ = 18,
        /// <summary>
        /// A PointM consists of a pair of double-precision coordinates in the order X, Y, plus a measure M.
        /// Mapsui interprets this as <see cref="Mapsui.Geometries.Point"/>
        /// </summary>
        PointM = 21,
        /// <summary>
        /// A shapefile PolyLineM consists of one or more parts. A part is a connected sequence of
        /// two or more points. Parts may or may not be connected to one another. Parts may or may
        /// not intersect one another.
        /// Mapsui interprets this as <see cref="Mapsui.Geometries.LineString"/> or <see cref="Mapsui.Geometries.MultiLineString"/>
        /// </summary>
        PolyLineM = 23,
        /// <summary>
        /// A PolygonM consists of a number of rings. A ring is a closed, non-self-intersecting loop.
        /// Mapsui interprets this as either <see cref="Mapsui.Geometries.Polygon"/> or <see cref="Mapsui.Geometries.MultiPolygon"/>
        /// </summary>
        PolygonM = 25,
        /// <summary>
        /// A MultiPointM represents a set of <see cref="PointM"/>s.
        /// Mapsui interprets this as <see cref="Mapsui.Geometries.MultiPoint"/>
        /// </summary>
        MultiPointM = 28,
        /// <summary>
        /// A MultiPatch consists of a number of surface patches. Each surface patch describes a
        /// surface. The surface patches of a MultiPatch are referred to as its parts, and the type of
        /// part controls how the order of vertices of an MultiPatch part is interpreted.
        /// Mapsui doesn't support this feature type.
        /// </summary>
        MultiPatch = 31
    };

    /// <summary>
    /// Shapefile data provider
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
    public class ShapeFile : IProvider<GeometryFeature>, IDisposable
    {
        /// <summary>
        /// Filter Delegate Method
        /// </summary>
        /// <remarks>
        /// The FilterMethod delegate is used for applying a method that filters data from the data set.
        /// The method should return 'true' if the feature should be included and false if not.
        /// </remarks>
        /// <returns>true if this feature should be included, false if it should be filtered</returns>
        public delegate bool FilterMethod(IFeature dr);

        private MRect? _envelope;
        private int _featureCount;
        private readonly bool _fileBasedIndex;
        private string _filename;
        private bool _isOpen;
        private ShapeType _shapeType;
        private BinaryReader _brShapeFile = default!;
        private BinaryReader _brShapeIndex = default!;
        private readonly DbaseReader? _dbaseFile;
        private FileStream _fsShapeFile = default!;
        private FileStream _fsShapeIndex = default!;
        private readonly object _syncRoot = new();

        /// <summary>
        /// Tree used for fast query of data
        /// </summary>
        private QuadTree? _tree;

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
            var dbfFile = Path.ChangeExtension(filename, ".dbf");
            if (File.Exists(dbfFile))
                _dbaseFile = new DbaseReader(dbfFile);
            //Parse shape header
            ParseHeader();
            //Read projection file
            ParseProjection();
        }

        /// <summary>
        /// Gets the <see cref="Shapefile.ShapeType">shape geometry type</see> in this shapefile.
        /// </summary>
        /// <remarks>
        /// The property isn't set until the first time the data source has been opened,
        /// and will throw an exception if this property has been called since initialization. 
        /// <para>All the non-Null shapes in a shapefile are required to be of the same shape
        /// type.</para>
        /// </remarks>
        public ShapeType ShapeType => _shapeType;

        /// <summary>
        /// Gets or sets the filename of the shapefile
        /// </summary>
        /// <remarks>If the filename changes, indexes will be rebuilt</remarks>
        public string Filename
        {
            get => _filename;
            set
            {
                if (value != _filename)
                {
                    lock (_syncRoot)
                    {
                        _filename = value;
                    }
                    if (_isOpen)
                        throw new ApplicationException("Cannot change filename while data source is open");

                    ParseHeader();
                    ParseProjection();
                    _tree?.Dispose();
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
        public Encoding? Encoding
        {
            get => _dbaseFile?.Encoding;
            set
            {
                if (_dbaseFile != null)
                {
                    _dbaseFile.Encoding = value;
                }
            }
        }

        /// <summary>
        /// Filter Delegate Method for limiting the data source
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
        public FilterMethod? FilterDelegate { get; set; }


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
                    _tree?.Dispose();
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
            Dispose(false);
        }



        /// <summary>
        /// Opens the data source
        /// </summary>
        private void Open()
        {
            // TODO:
            // Get a Connector.  The connector returned is guaranteed to be connected and ready to go.
            // Pooling.Connector connector = Pooling.ConnectorPool.ConnectorPoolManager.RequestConnector(this,true);

            if (!_isOpen)
            {
                _fsShapeIndex?.Dispose();
                _fsShapeIndex = new FileStream(_filename.Remove(_filename.Length - 4, 4) + ".shx", FileMode.Open, FileAccess.Read);
                _brShapeFile?.Dispose();
                _brShapeIndex = new BinaryReader(_fsShapeIndex, Encoding.Unicode);
                _fsShapeFile?.Dispose();
                _fsShapeFile = new FileStream(_filename, FileMode.Open, FileAccess.Read);
                _brShapeFile?.Dispose();
                _brShapeFile = new BinaryReader(_fsShapeFile);
                InitializeShape(_filename, _fileBasedIndex);
                _dbaseFile?.Open();
                _isOpen = true;
            }
        }

        /// <summary>
        /// Closes the data source
        /// </summary>
        private void Close()
        {
            if (!_disposed)
            {
                if (_isOpen)
                {
                    _brShapeIndex.Dispose();
                    _brShapeFile.Dispose();
                    _fsShapeFile.Dispose();
                    _brShapeFile.Dispose();
                    _fsShapeIndex.Dispose();
                    _dbaseFile?.Dispose();
                    _isOpen = false;
                }
            }
        }

        /// <summary>
        /// Returns geometries whose bounding box intersects 'bbox'
        /// </summary>
        /// <remarks>
        /// <para>Please note that this method doesn't guarantee that the geometries returned actually intersect 'bbox', but only
        /// that their BoundingBox intersects 'bbox'.</para>
        /// <para>This method is much faster than the QueryFeatures method, because intersection tests
        /// are performed on objects simplified by their BoundingBox, and using the Spatial Index.</para>
        /// </remarks>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<IGeometry> GetGeometriesInView(MRect bbox)
        {
            lock (_syncRoot)
            {
                Open();

                try
                {
                    //Use the spatial index to get a list of features whose BoundingBox intersects bbox
                    var objectList = GetObjectIDsInViewPrivate(bbox);
                    if (objectList.Count == 0) //no features found. Return an empty set
                        return new Collection<IGeometry>();

                    var geometries = new Collection<IGeometry>();

                    for (var i = 0; i < objectList.Count; i++)
                    {
                        var g = GetGeometryPrivate(objectList[i]);
                        if (g != null) geometries.Add(g);
                    }
                    return geometries;
                }
                finally
                {
                    Close();
                }
            }

        }


        /// <summary>
        /// Returns geometry Object IDs whose bounding box intersects 'bbox'
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public Collection<uint> GetObjectIDsInView(MRect bbox)
        {
            lock (_syncRoot)
            {
                Open();

                try
                {
                    return GetObjectIDsInViewPrivate(bbox);
                }
                finally
                {
                    Close();
                }
            }

        }

        private Collection<uint> GetObjectIDsInViewPrivate(MRect? bbox)
        {
            if (bbox == null)
                return new Collection<uint>();
            if (!_isOpen)
                throw new ApplicationException("An attempt was made to read from a closed data source");
            //Use the spatial index to get a list of features whose BoundingBox intersects bbox
            return _tree!.Search(bbox);
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public IGeometry? GetGeometry(uint oid)
        {
            lock (_syncRoot)
            {
                Open();

                try
                {
                    return GetGeometryPrivate(oid);
                }
                finally
                {
                    Close();
                }
            }
        }

        private IGeometry? GetGeometryPrivate(uint oid)
        {
            if (FilterDelegate != null) //Apply filtering
            {
                var fdr = GetFeature(oid);
                return fdr?.Geometry;
            }

            return ReadGeometry(oid);
        }

        /// <summary>
        /// Returns the total number of features in the data source (without any filter applied)
        /// </summary>
        /// <returns></returns>
        public int GetFeatureCount()
        {
            return _featureCount;
        }

        /// <summary>
        /// Returns the extent of the data source
        /// </summary>
        /// <returns></returns>
        public MRect? GetExtent()
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
        public string? CRS { get; set; } = "";

        private void InitializeShape(string filename, bool fileBasedIndex)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException($"Could not find file \"{filename}\"");
            if (!filename.ToLower().EndsWith(".shp"))
                throw (new Exception("Invalid shapefile filename: " + filename));

            LoadSpatialIndex(fileBasedIndex); //Load spatial index			
        }

        /// <summary>
        /// Reads and parses the header of the .shx index file
        /// </summary>
        private void ParseHeader()
        {
            _fsShapeIndex?.Dispose();
            _fsShapeIndex = new FileStream(Path.ChangeExtension(_filename, ".shx"), FileMode.Open,
                                          FileAccess.Read);
            _brShapeIndex?.Dispose();
            _brShapeIndex = new BinaryReader(_fsShapeIndex, Encoding.Unicode);

            _brShapeIndex.BaseStream.Seek(0, 0);
            //Check file header
            if (_brShapeIndex.ReadInt32() != 170328064)
                //File Code is actually 9994, but in Little Endian Byte Order this is '170328064'
                throw (new ApplicationException("Invalid Shapefile Index (.shx)"));

            _brShapeIndex.BaseStream.Seek(24, 0); //seek to File Length
            var indexFileSize = SwapByteOrder(_brShapeIndex.ReadInt32());
            //Read file length as big-endian. The length is based on 16bit words
            _featureCount = (2 * indexFileSize - 100) / 8;
            //Calculate FeatureCount. Each feature takes up 8 bytes. The header is 100 bytes

            _brShapeIndex.BaseStream.Seek(32, 0); //seek to ShapeType
            _shapeType = (ShapeType)_brShapeIndex.ReadInt32();

            //Read the spatial bounding box of the contents
            _brShapeIndex.BaseStream.Seek(36, 0); //seek to box
            _envelope = new MRect(_brShapeIndex.ReadDouble(), _brShapeIndex.ReadDouble(), _brShapeIndex.ReadDouble(),
                                        _brShapeIndex.ReadDouble());

            _brShapeIndex.Close();
            _fsShapeIndex.Close();
        }

        /// <summary>
        /// Reads and parses the projection if a projection file exists
        /// </summary>
        private void ParseProjection()
        {
            var projFile = Path.GetDirectoryName(Filename) + "\\" + Path.GetFileNameWithoutExtension(Filename) +
                              ".prj";
            if (File.Exists(projFile))
            {
                try
                {
                    // todo: Automatically parse coordinate system: 
                    // var wkt = File.ReadAllText(projFile);
                    // CoordinateSystemWktReader.Parse(wkt);

                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Coordinate system file '" + projFile +
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

            for (var x = 0; x < _featureCount; ++x)
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
            var buffer = BitConverter.GetBytes(i);
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

            var tree = CreateSpatialIndex();
            tree.SaveIndex(filename + ".sidx");
            return tree;
        }

        /// <summary>
        /// Generates a spatial index for a specified shape file.
        /// </summary>
        private QuadTree CreateSpatialIndex()
        {
            var objList = new List<QuadTree.BoxObjects>();
            // Convert all the geometries to BoundingBoxes 
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

            Heuristic heuristic;
            heuristic.Maxdepth = (int)Math.Ceiling(Math.Log(GetFeatureCount(), 2));
            heuristic.Minerror = 10;
            heuristic.Tartricnt = 5;
            heuristic.Mintricnt = 2;
            return new QuadTree(objList, 0, heuristic);
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
                _tree?.Dispose();
                _tree = !loadFromFile ? CreateSpatialIndex() : CreateSpatialIndexFromFile(_filename);
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
                {
                    _tree?.Dispose();
                    _tree = CreateSpatialIndexFromFile(_filename);
                }
            }
            else
            {
                _tree?.Dispose();
                _tree = CreateSpatialIndex();
            }
        }

        /// <summary>
        /// Reads all BoundingBoxes of features in the shapefile. This is used for spatial indexing.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<MRect> GetAllFeatureBoundingBoxes()
        {
            var offsetOfRecord = ReadIndex(); //Read the whole .idx file

            if (_shapeType == ShapeType.Point)
            {
                for (var a = 0; a < _featureCount; ++a)
                {
                    _fsShapeFile.Seek(offsetOfRecord[a] + 8, 0); //skip record number and content length
                    if ((ShapeType)_brShapeFile.ReadInt32() != ShapeType.Null)
                    {
                        var x = _brShapeFile.ReadDouble();
                        var y = _brShapeFile.ReadDouble();
                        yield return new MRect(x, y, x, y);
                    }
                }
            }
            else
            {
                for (var a = 0; a < _featureCount; ++a)
                {
                    _fsShapeFile.Seek(offsetOfRecord[a] + 8, 0); //skip record number and content length
                    if ((ShapeType)_brShapeFile.ReadInt32() != ShapeType.Null)
                        yield return new MRect(_brShapeFile.ReadDouble(), _brShapeFile.ReadDouble(),
                                                     _brShapeFile.ReadDouble(), _brShapeFile.ReadDouble());
                }
            }
        }

        /// <summary>
        /// Reads and parses the geometry with ID 'oid' from the ShapeFile
        /// </summary>
        /// <remarks><see cref="FilterDelegate">Filtering</see> is not applied to this method</remarks>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        // ReSharper disable once CyclomaticComplexity // Fix when changes need to be made here
        private Geometry? ReadGeometry(uint oid)
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
                var nPoints = _brShapeFile.ReadInt32(); // get the number of points
                if (nPoints == 0)
                    return null;
                for (var i = 0; i < nPoints; i++)
                    feature.Points.Add(new Point(_brShapeFile.ReadDouble(), _brShapeFile.ReadDouble()));

                return feature;
            }
            if (_shapeType == ShapeType.PolyLine || _shapeType == ShapeType.Polygon ||
                _shapeType == ShapeType.PolyLineM || _shapeType == ShapeType.PolygonM ||
                _shapeType == ShapeType.PolyLineZ || _shapeType == ShapeType.PolygonZ)
            {
                _brShapeFile.BaseStream.Seek(32 + _brShapeFile.BaseStream.Position, 0); //skip min/max box

                var nParts = _brShapeFile.ReadInt32(); // get number of parts (segments)
                if (nParts == 0)
                    return null;
                var nPoints = _brShapeFile.ReadInt32(); // get number of points

                var segments = new int[nParts + 1];
                //Read in the segment indexes
                for (var b = 0; b < nParts; b++)
                    segments[b] = _brShapeFile.ReadInt32();
                //add end point
                segments[nParts] = nPoints;

                if ((int)_shapeType % 10 == 3)
                {
                    var multiLineString = new MultiLineString();
                    for (var lineId = 0; lineId < nParts; lineId++)
                    {
                        var line = new LineString();
                        for (var i = segments[lineId]; i < segments[lineId + 1]; i++)
                            line.Vertices.Add(new Point(_brShapeFile.ReadDouble(), _brShapeFile.ReadDouble()));
                        multiLineString.LineStrings.Add(line);
                    }
                    if (multiLineString.LineStrings.Count == 1)
                        return multiLineString[0];
                    return multiLineString;
                }
                else
                {
                    // First read all the rings
                    var rings = new List<LinearRing>();
                    for (var ringId = 0; ringId < nParts; ringId++)
                    {
                        var ring = new LinearRing();
                        for (var i = segments[ringId]; i < segments[ringId + 1]; i++)
                            ring.Vertices.Add(new Point(_brShapeFile.ReadDouble(), _brShapeFile.ReadDouble()));
                        rings.Add(ring);
                    }
                    var isCounterClockWise = new bool[rings.Count];
                    var polygonCount = 0;
                    for (var i = 0; i < rings.Count; i++)
                    {
                        isCounterClockWise[i] = rings[i].IsCCW();
                        if (!isCounterClockWise[i])
                            polygonCount++;
                    }
                    if (polygonCount == 1) // We only have one polygon
                    {
                        var poly = new Polygon { ExteriorRing = rings[0] };
                        if (rings.Count > 1)
                            for (var i = 1; i < rings.Count; i++)
                                poly.InteriorRings.Add(rings[i]);
                        return poly;
                    }
                    else
                    {
                        var multiPolygon = new MultiPolygon();
                        var poly = new Polygon { ExteriorRing = rings[0] };
                        for (var i = 1; i < rings.Count; i++)
                        {
                            if (!isCounterClockWise[i])
                            {
                                multiPolygon.Polygons.Add(poly);
                                poly = new Polygon(rings[i]);
                            }
                            else
                                poly.InteriorRings.Add(rings[i]);
                        }
                        multiPolygon.Polygons.Add(poly);
                        return multiPolygon;
                    }
                }
            }

            throw new ApplicationException($"Shapefile type {_shapeType} not supported");
        }

        /// <summary>
        /// Gets a data row from the data source at the specified index belonging to the specified datatable
        /// </summary>
        /// <param name="rowId"></param>
        /// <param name="features">Data table to feature should belong to.</param>
        /// <returns></returns>
        public GeometryFeature? GetFeature(uint rowId, List<GeometryFeature>? features = null)
        {
            lock (_syncRoot)
            {
                Open();

                try
                {
                    return GetFeaturePrivate(rowId, features);
                }
                finally
                {
                    Close();
                }
            }

        }

        private GeometryFeature? GetFeaturePrivate(uint rowId, IEnumerable<GeometryFeature>? dt)
        {
            if (_dbaseFile != null)
            {
                var dr = _dbaseFile.GetFeature(rowId, dt ?? new List<GeometryFeature>());
                if (dr != null)
                {
                    dr.Geometry = ReadGeometry(rowId);
                    if (FilterDelegate == null || FilterDelegate(dr))
                        return dr;
                }
                return null;
            }
            throw (new ApplicationException("An attempt was made to read DBase data from a shapefile without a valid .DBF file"));
        }


        public IEnumerable<GeometryFeature> GetFeatures(FetchInfo fetchInfo)
        {
            lock (_syncRoot)
            {
                Open();
                try
                {
                    //Use the spatial index to get a list of features whose BoundingBox intersects bbox
                    var objectList = GetObjectIDsInViewPrivate(fetchInfo.Extent);
                    var features = new List<GeometryFeature>();

                    foreach (var index in objectList)
                    {
                        var feature = _dbaseFile?.GetFeature(index, features);
                        if (feature != null)
                        {
                            feature.Geometry = ReadGeometry(index);
                            if (feature.Geometry?.BoundingBox == null) continue;
                            if (!feature.Geometry.BoundingBox.Intersects(fetchInfo.Extent.ToBoundingBox())) continue;
                            if (FilterDelegate != null && !FilterDelegate(feature)) continue;
                            features.Add(feature);
                        }
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