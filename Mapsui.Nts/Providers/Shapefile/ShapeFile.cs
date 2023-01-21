// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Nts.Extensions;
using Mapsui.Nts.Providers.Shapefile.Indexing;
using Mapsui.Projections;
using Mapsui.Providers;
using NetTopologySuite.Geometries;

namespace Mapsui.Nts.Providers.Shapefile;

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
    /// Mapsui interprets this as <see cref="Point"/>
    /// </summary>
    Point = 1,
    /// <summary>
    /// PolyLine is an ordered set of coordinates that consists of one or more parts. A part is a
    /// connected sequence of two or more points. Parts may or may not be connected to one
    ///	another. Parts may or may not intersect one another.
    /// Mapsui interprets this as either <see cref="LineString"/> or <see cref="MultiLineString"/>
    /// </summary>
    PolyLine = 3,
    /// <summary>
    /// A polygon consists of one or more rings. A ring is a connected sequence of four or more
    /// points that form a closed, non-self-intersecting loop. A polygon may contain multiple
    /// outer rings. The order of coordinates or orientation for a ring indicates which side of the ring
    /// is the interior of the polygon. The neighborhood to the right of an observer walking along
    /// the ring in vertex order is the neighborhood inside the polygon. Coordinates of rings defining
    /// holes in polygons are in a counterclockwise direction. Vertices for a single, ringed
    /// polygon are, therefore, always in clockwise order. The rings of a polygon are referred to
    /// as its parts.
    /// Mapsui interprets this as either <see cref="Polygon"/> or <see cref="MultiPolygon"/>
    /// </summary>
    Polygon = 5,
    /// <summary>
    /// A MultiPoint represents a set of points.
    /// Mapsui interprets this as <see cref="MultiPoint"/>
    /// </summary>
    Multipoint = 8,
    /// <summary>
    /// A PointZ consists of a triplet of double-precision coordinates plus a measure.
    /// Mapsui interprets this as <see cref="Point"/>
    /// </summary>
    PointZ = 11,
    /// <summary>
    /// A PolyLineZ consists of one or more parts. A part is a connected sequence of two or
    /// more points. Parts may or may not be connected to one another. Parts may or may not
    /// intersect one another.
    /// Mapsui interprets this as <see cref="LineString"/> or <see cref="MultiLineString"/>
    /// </summary>
    PolyLineZ = 13,
    /// <summary>
    /// A PolygonZ consists of a number of rings. A ring is a closed, non-self-intersecting loop.
    /// A PolygonZ may contain multiple outer rings. The rings of a PolygonZ are referred to as
    /// its parts.
    /// Mapsui interprets this as either <see cref="Polygon"/> or <see cref="MultiPolygon"/>
    /// </summary>
    PolygonZ = 15,
    /// <summary>
    /// A MultiPointZ represents a set of <see cref="PointZ"/>s.
    /// Mapsui interprets this as <see cref="MultiPoint"/>
    /// </summary>
    MultiPointZ = 18,
    /// <summary>
    /// A PointM consists of a pair of double-precision coordinates in the order X, Y, plus a measure M.
    /// Mapsui interprets this as <see cref="Point"/>
    /// </summary>
    PointM = 21,
    /// <summary>
    /// A shapefile PolyLineM consists of one or more parts. A part is a connected sequence of
    /// two or more points. Parts may or may not be connected to one another. Parts may or may
    /// not intersect one another.
    /// Mapsui interprets this as <see cref="LineString"/> or <see cref="MultiLineString"/>
    /// </summary>
    PolyLineM = 23,
    /// <summary>
    /// A PolygonM consists of a number of rings. A ring is a closed, non-self-intersecting loop.
    /// Mapsui interprets this as either <see cref="Polygon"/> or <see cref="MultiPolygon"/>
    /// </summary>
    PolygonM = 25,
    /// <summary>
    /// A MultiPointM represents a set of <see cref="PointM"/>s.
    /// Mapsui interprets this as <see cref="MultiPoint"/>
    /// </summary>
    MultiPointM = 28,
    /// <summary>
    /// A MultiPatch consists of a number of surface patches. Each surface patch describes a
    /// surface. The surface patches of a MultiPatch are referred to as its parts, and the type of
    /// part controls how the order of coordinates of an MultiPatch part is interpreted.
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
public class ShapeFile : IProvider, IDisposable
{

    static ShapeFile()
    {
        try
        {
            // Without this Fix this method throws an exception:
            // Encoding.GetEncoding(...)
            // System.NotSupportedException: 'No data is available for encoding 1252. For information on defining a custom encoding, see the documentation for the Encoding.RegisterProvider method.'
            // StackOverflow
            // https://stackoverflow.com/questions/50858209/system-notsupportedexception-no-data-is-available-for-encoding-1252
            // Workaround for Bug in Shapefile
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Error, e.Message, e);
        }
    }

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
    private BinaryReader? _brShapeFile;
    private BinaryReader? _brShapeIndex;
    private readonly DbaseReader? _dbaseFile;
    private FileStream? _fsShapeFile;
    private FileStream? _fsShapeIndex;
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
    /// <param name="readPrjFile">Read the proj File and set the correct CRS</param>
    /// <param name="projectionCrs">Projection Crs</param>
    public ShapeFile(string filename, bool fileBasedIndex = false, bool readPrjFile = false, IProjectionCrs? projectionCrs = null)
    {
        _filename = filename;
        _fileBasedIndex = fileBasedIndex && File.Exists(Path.ChangeExtension(filename, ".shx"));
        _projectionCrs = projectionCrs ?? ProjectionDefaults.Projection as IProjectionCrs;

        //Initialize DBF
        var dbfFile = Path.ChangeExtension(filename, ".dbf");
        if (File.Exists(dbfFile))
            _dbaseFile = new DbaseReader(dbfFile);
        //Parse shape header
        ParseHeader();
        //Read projection file
        if (readPrjFile)
        {
            ParseProjection();
        }
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
                    _filename = value;
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
    /// The DBase default encoding is UTF7"/>.
    /// </remarks>
    public Encoding? Encoding
    {
        get => _dbaseFile?.Encoding;
        set
        {
            if (_dbaseFile != null)
                _dbaseFile.Encoding = value;
        }
    }

    /// <summary>
    /// Filter Delegate Method for limiting the data source
    /// </summary>
    public FilterMethod? FilterDelegate { get; set; }


    private bool _disposed;
    private readonly IProjectionCrs? _projectionCrs;

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
            _brShapeIndex?.Dispose();
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
            if (_isOpen)
            {
                _brShapeIndex?.Dispose();
                _brShapeFile?.Dispose();
                _fsShapeFile?.Dispose();
                _fsShapeIndex?.Dispose();
                _dbaseFile?.Dispose();
                _isOpen = false;
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
    public Collection<Geometry> GetGeometriesInView(MRect bbox)
    {
        lock (_syncRoot)
        {
            Open();

            try
            {
                //Use the spatial index to get a list of features whose BoundingBox intersects bbox
                var objectList = GetObjectIDsInViewPrivate(bbox);
                if (objectList.Count == 0) //no features found. Return an empty set
                    return new Collection<Geometry>();

                var geometries = new Collection<Geometry>();

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
    public Geometry? GetGeometry(uint oid)
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

    private Geometry? GetGeometryPrivate(uint oid)
    {
        if (FilterDelegate != null) //Apply filtering
        {
            using var fdr = GetFeature(oid);
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
            throw new Exception("Invalid shapefile filename: " + filename);

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
            throw new ApplicationException("Invalid Shapefile Index (.shx)");

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
        var projFile = Path.ChangeExtension(Filename, ".prj");
        if (File.Exists(projFile))
            try
            {
                //Read Projection
                var esriString = File.ReadAllText(projFile);
                if (_projectionCrs != null)
                {
                    CRS = _projectionCrs.CrsFromEsri(esriString);
                }

            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, "Coordinate system file '" + projFile +
                                             "' found, but could not be parsed. WKT parser returned:" + ex.Message, ex);
                throw;
            }
    }

    /// <summary>
    /// Reads the record offsets from the .shx index file and returns the information in an array
    /// </summary>
    private int[] ReadIndex()
    {
        if (_brShapeIndex is null)
            return Array.Empty<int>();

        var offsetOfRecord = new int[_featureCount];
        _brShapeIndex.BaseStream.Seek(100, 0); // Skip the header

        for (var x = 0; x < _featureCount; ++x)
        {
            offsetOfRecord[x] = 2 * SwapByteOrder(_brShapeIndex.ReadInt32()); // Read shape data position // ibuffer);
            _brShapeIndex.BaseStream.Seek(_brShapeIndex.BaseStream.Position + 4, 0); // Skip content length
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
        if (_brShapeIndex is null)
            throw new Exception("_brShapeIndex can not be null");
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
            try
            {
                return QuadTree.FromFile(filename + ".sidx");
            }
            catch (QuadTree.ObsoleteFileFormatException ex)
            {
                File.Delete(filename + ".sidx");
                Logger.Log(LogLevel.Warning, ex.Message, ex);
                return CreateSpatialIndexFromFile(filename);
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
            if (!double.IsNaN(box.Left) && !double.IsNaN(box.Right) && !double.IsNaN(box.Bottom) &&
                !double.IsNaN(box.Top))
            {
                var g = new QuadTree.BoxObjects { Box = box, Id = i };
                objList.Add(g);
                i++;
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
        if (_fsShapeFile is null)
            yield break;
        if (_brShapeFile is null)
            yield break;

        var offsetOfRecord = ReadIndex(); //Read the whole .idx file

        if (_shapeType == ShapeType.Point)
            for (var a = 0; a < _featureCount; ++a)
            {
                _fsShapeFile.Seek(offsetOfRecord[a] + 8, 0); // Skip record number and content length
                if ((ShapeType)_brShapeFile.ReadInt32() != ShapeType.Null)
                {
                    var x = _brShapeFile.ReadDouble();
                    var y = _brShapeFile.ReadDouble();
                    yield return new MRect(x, y, x, y);
                }
            }
        else
            for (var a = 0; a < _featureCount; ++a)
            {
                _fsShapeFile.Seek(offsetOfRecord[a] + 8, 0); // Skip record number and content length
                if ((ShapeType)_brShapeFile.ReadInt32() != ShapeType.Null)
                    yield return new MRect(_brShapeFile.ReadDouble(), _brShapeFile.ReadDouble(),
                                                 _brShapeFile.ReadDouble(), _brShapeFile.ReadDouble());
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
        if (_brShapeFile is null) return null;
        _brShapeFile.BaseStream.Seek(GetShapeIndex(oid) + 8, 0); // Skip record number and content length
        var type = (ShapeType)_brShapeFile.ReadInt32(); //Shape type
        if (type == ShapeType.Null)
            return null;
        if (_shapeType == ShapeType.Point || _shapeType == ShapeType.PointM || _shapeType == ShapeType.PointZ)
            return new Point(_brShapeFile.ReadDouble(), _brShapeFile.ReadDouble());
        if (_shapeType == ShapeType.Multipoint || _shapeType == ShapeType.MultiPointM ||
            _shapeType == ShapeType.MultiPointZ)
        {
            _brShapeFile.BaseStream.Seek(32 + _brShapeFile.BaseStream.Position, 0); // Skip min/max box
            var nPoints = _brShapeFile.ReadInt32(); // Get the number of points
            if (nPoints == 0)
                return null;

            var points = new List<Point>();
            for (var i = 0; i < nPoints; i++)
                points.Add(new Point(_brShapeFile.ReadDouble(), _brShapeFile.ReadDouble()));

            return new MultiPoint(points.ToArray());
        }
        if (_shapeType == ShapeType.PolyLine || _shapeType == ShapeType.Polygon ||
            _shapeType == ShapeType.PolyLineM || _shapeType == ShapeType.PolygonM ||
            _shapeType == ShapeType.PolyLineZ || _shapeType == ShapeType.PolygonZ)
        {
            _brShapeFile.BaseStream.Seek(32 + _brShapeFile.BaseStream.Position, 0); // Skip min/max box

            var nParts = _brShapeFile.ReadInt32(); // Get number of parts (segments)
            if (nParts == 0)
                return null;
            var nPoints = _brShapeFile.ReadInt32(); // Get number of points

            var segments = new int[nParts + 1];
            //Read in the segment indexes
            for (var b = 0; b < nParts; b++)
                segments[b] = _brShapeFile.ReadInt32();
            //add end point
            segments[nParts] = nPoints;

            if ((int)_shapeType % 10 == 3)
            {
                var lineStrings = new List<LineString>();
                for (var lineId = 0; lineId < nParts; lineId++)
                {
                    var coordinates = new List<Coordinate>();
                    for (var i = segments[lineId]; i < segments[lineId + 1]; i++)
                        coordinates.Add(new Coordinate(_brShapeFile.ReadDouble(), _brShapeFile.ReadDouble()));
                    lineStrings.Add(new LineString(coordinates.ToArray()));
                }
                if (lineStrings.Count == 1)
                    return lineStrings[0];
                return new MultiLineString(lineStrings.ToArray());
            }
            else
            {
                // First read all the rings
                var rings = new List<LinearRing>();
                for (var ringId = 0; ringId < nParts; ringId++)
                {
                    var ring = new List<Coordinate>();
                    for (var i = segments[ringId]; i < segments[ringId + 1]; i++)
                        ring.Add(new Coordinate(_brShapeFile.ReadDouble(), _brShapeFile.ReadDouble()));
                    rings.Add(new LinearRing(ring.ToArray()));
                }
                var isCounterClockWise = new bool[rings.Count];
                var polygonCount = 0;
                for (var i = 0; i < rings.Count; i++)
                {
                    isCounterClockWise[i] = rings[i].IsCCW;
                    if (!isCounterClockWise[i])
                        polygonCount++;
                }
                if (polygonCount == 1) // We only have one polygon
                {
                    var p = CreatePolygon(rings);
                    return p;
                }
                else
                {
                    var polygons = new List<Polygon>();
                    var linearRings = new List<LinearRing> { rings[0] };

                    for (var i = 1; i < rings.Count; i++)
                        if (!isCounterClockWise[i])
                        {
                            // The !isCCW indicates this is an outerRing (or shell in NTS)
                            // So the previous one is done and is added to the list. A new list of linear rings is created for the next polygon.
                            var p1 = CreatePolygon(linearRings);
                            if (p1 is not null) polygons.Add(p1);
                            linearRings = new List<LinearRing> { rings[i] };
                        }
                        else
                            linearRings.Add(rings[i]);
                    var p = CreatePolygon(linearRings);
                    if (p is not null) polygons.Add(p);

                    return new MultiPolygon(polygons.ToArray());
                }
            }
        }

        throw new ApplicationException($"Shapefile type {_shapeType} not supported");
    }

    private static Polygon? CreatePolygon(List<LinearRing> poly)
    {
        if (poly.Count == 1)
            return new Polygon(poly[0]);
        if (poly.Count > 1)
            return new Polygon(poly[0], poly.Skip(1).ToArray());
        return null;
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
        throw new ApplicationException("An attempt was made to read DBase data from a shapefile without a valid .DBF file");
    }


    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
    public Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
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
                        if (feature.Geometry?.EnvelopeInternal == null) continue;
                        if (!feature.Geometry.EnvelopeInternal.Intersects(fetchInfo.Extent.ToEnvelope())) continue;
                        if (FilterDelegate != null && !FilterDelegate(feature)) continue;
                        features.Add(feature);
                    }
                }
                return Task.FromResult((IEnumerable<IFeature>)features);
            }
            finally
            {
                Close();
            }
        }
    }
}
