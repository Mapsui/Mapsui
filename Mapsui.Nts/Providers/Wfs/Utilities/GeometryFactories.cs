// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// WFS provider by Peter Robineau (www.geoimpact.ch)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using NetTopologySuite.Geometries;

namespace Mapsui.Providers.Wfs.Utilities;

/// <summary>
/// This class is the base class for geometry production.
/// It provides some parsing routines for XML compliant to GML2/GML3.
/// </summary>
internal abstract class GeometryFactory : IDisposable
{

    protected const string Gmlns = "http://www.opengis.net/gml";
    private readonly NumberFormatInfo _formatInfo = new();
    private readonly HttpClientUtil? _httpClientUtil;
    private readonly List<IPathNode> _pathNodes = new();
    protected AlternativePathNodesCollection? CoordinatesNode;
    private string _cs = ",";
    protected IPathNode? FeatureNode;
    protected XmlReader? FeatureReader;
    protected readonly WfsFeatureTypeInfo FeatureTypeInfo;
    protected XmlReader? GeomReader;
    protected Collection<Geometry> Geoms = new();
    protected IPathNode? LabelNode;
    protected AlternativePathNodesCollection? ServiceExceptionNode;
    private string _ts = " ";
    protected XmlReader? XmlReader;
    private bool _initialized;

    /// <summary>
    /// Gets or sets the axis order
    /// </summary>
    internal int[] AxisOrder { get; set; } = { 0, 1 }; // default value

    /// <summary>
    /// Protected constructor for the abstract class.
    /// </summary>
    /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
    /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
    protected GeometryFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo)
    {
        FeatureTypeInfo = featureTypeInfo;
        _httpClientUtil = httpClientUtil;
    }

    /// <summary>Init Async </summary>
    /// <returns></returns>
    public async Task InitAsync()
    {
        if (_initialized)
            return;

        if (_httpClientUtil == null)
            return;

        _initialized = true;
        XmlReader = await CreateReaderAsync(_httpClientUtil);

        try
        {
            if (FeatureTypeInfo?.LabelFields != null)
            {
                var pathNodes = new IPathNode[FeatureTypeInfo.LabelFields.Count];
                for (var i = 0; i < pathNodes.Length; i++)
                {
                    pathNodes[i] = new PathNode(FeatureTypeInfo.FeatureTypeNamespace, FeatureTypeInfo.LabelFields[i], (NameTable)XmlReader.NameTable);
                }
                LabelNode = new AlternativePathNodesCollection(pathNodes);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "An exception occured while initializing the label path node! " + ex.Message, ex);
            throw;
        }

        InitializePathNodes();
        InitializeSeparators();
    }

    /// <summary>
    /// Protected constructor for the abstract class.
    /// </summary>
    /// <param name="xmlReader">An XmlReader instance</param>
    /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
    protected GeometryFactory(XmlReader xmlReader, WfsFeatureTypeInfo featureTypeInfo)
    {
        FeatureTypeInfo = featureTypeInfo;
        XmlReader = xmlReader;
        InitializePathNodes();
        InitializeSeparators();
    }

    /// <summary>
    /// Abstract method - overwritten by derived classes for producing instances
    /// derived from <see cref="Geometry"/>.
    /// </summary>
    internal abstract Task<Collection<Geometry>> CreateGeometriesAsync(List<IFeature> features);

    /// <summary>
    /// This method parses a coordinates or posList(from 'GetFeature' response). 
    /// </summary>
    /// <param name="reader">An XmlReader instance at the position of the coordinates to read</param>
    /// <returns>A point collection (the collected coordinates)</returns>
    protected Collection<Coordinate> ParseCoordinates(XmlReader reader)
    {
        if (!reader.Read()) return new Collection<Coordinate>();

        var name = reader.LocalName;
        var coordinateString = reader.ReadElementString();
        var coordinates = new Collection<Coordinate>();
        string[][] coordinateValues;
        var i = 0;

        if (name.Equals("coordinates"))
        {
            var coords = coordinateString.Split(_ts[0]);
            coordinateValues = coords.Select(s => s.Split(_cs[0])).ToArray();
        }
        else
        {
            // we assume there are only x,y pairs
            var coords = coordinateString.Split(' ');
            var odds = coords.Where((s, idx) => idx % 2 == 0);
            var evens = coords.Where((s, idx) => idx % 2 != 0);
            coordinateValues = odds.Zip(evens, (odd, even) => new[] { odd, even }).ToArray();
        }
        var length = coordinateValues.Length;

        while (i < length)
        {
            var c = new double[2];
            var values = coordinateValues[i++];
            c[AxisOrder[0]] = Convert.ToDouble(values[0], _formatInfo);
            c[AxisOrder[1]] = Convert.ToDouble(values[1], _formatInfo);

            var coordinate = new Coordinate(c[0], c[1]);

            coordinates.Add(coordinate);
        }

        return coordinates;
    }

    /// <summary>
    /// This method retrieves an XmlReader within a specified context.
    /// </summary>
    /// <param name="reader">An XmlReader instance that is the origin of a created sub-reader</param>
    /// <param name="labels">A dictionary for recording label values. Pass 'null' to ignore searching for label values</param>
    /// <param name="pathNodes">A list of <see cref="IPathNode"/> instances defining the context of the retrieved reader</param>
    /// <returns>A sub-reader of the XmlReader given as argument</returns>
    protected XmlReader? GetSubReaderOf(XmlReader reader, Dictionary<string, string>? labels, params IPathNode?[] pathNodes)
    {
        if (pathNodes.All(f => f == null))
            return null;
        _pathNodes.Clear();
        _pathNodes.AddRange(pathNodes.Where(f => f != null)!);
        return GetSubReaderOf(reader, labels, _pathNodes);
    }

    /// <summary>
    /// This method retrieves an XmlReader within a specified context.
    /// Moreover it collects label values before or after a geometry could be found.
    /// </summary>
    /// <param name="reader">An XmlReader instance that is the origin of a created sub-reader</param>
    /// <param name="labels">A dictionary for recording label values. Pass 'null' to ignore searching for label values</param>
    /// <param name="pathNodes">A list of <see cref="IPathNode"/> instances defining the context of the retrieved reader</param>
    /// <returns>A sub-reader of the XmlReader given as argument</returns>
    protected XmlReader? GetSubReaderOf(XmlReader reader, Dictionary<string, string>? labels, List<IPathNode> pathNodes)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (pathNodes[0].Matches(reader))
                {
                    pathNodes.RemoveAt(0);

                    return pathNodes.Count > 0
                        ? GetSubReaderOf(reader.ReadSubtree(), null, pathNodes)
                        : reader.ReadSubtree();
                }

                if (labels != null)
                    if (LabelNode != null)
                        if (LabelNode.Matches(reader))
                        {
                            var labelName = reader.Name;
                            var labelValue = reader.ReadString();

                            // remove the namespace
                            if (labelName.Contains(":"))
                                labelName = labelName.Split(':')[1];

                            labels.Add(labelName, labelValue);
                        }


                if (!(ServiceExceptionNode?.Matches(reader) ?? false)) continue;

                var errorMessage = reader.ReadInnerXml();
                Trace.TraceError("A service exception occured: " + errorMessage);
                throw new Exception("A service exception occured: " + errorMessage);
            }
        }

        return null;
    }

    /// <summary>
    /// This method adds labels to the collection.
    /// </summary>
    protected IFeature AddLabel(Dictionary<string, string> labelValues, Geometry geom)
    {
        var feature = new GeometryFeature(geom);

        foreach (var keyPair in labelValues)
        {
            var labelName = keyPair.Key;
            var labelValue = keyPair.Value;

            feature[labelName] = labelValue;
        }

        labelValues.Clear();

        return feature;
    }



    /// <summary>
    /// This method initializes the XmlReader member.
    /// </summary>
    /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
    private async Task<XmlReader> CreateReaderAsync(HttpClientUtil httpClientUtil)
    {
        var xmlReaderSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true,
            DtdProcessing = DtdProcessing.Prohibit,
            Async = true,
        };
        return XmlReader.Create(
            await httpClientUtil.GetDataStreamAsync() ?? throw new ArgumentException(nameof(httpClientUtil)),
            xmlReaderSettings);
    }

    /// <summary>
    /// This method initializes path nodes needed by the derived classes.
    /// </summary>
    private void InitializePathNodes()
    {
        IPathNode coordinatesNode = new PathNode("http://www.opengis.net/gml", "coordinates",
                                                 (NameTable)XmlReader!.NameTable);
        IPathNode posListNode = new PathNode("http://www.opengis.net/gml", "posList",
                                             (NameTable)XmlReader.NameTable);
        IPathNode ogcServiceExceptionNode = new PathNode("http://www.opengis.net/ogc", "ServiceException",
                                                         (NameTable)XmlReader.NameTable);
        IPathNode serviceExceptionNode = new PathNode("", "ServiceException", (NameTable)XmlReader.NameTable);
        //ServiceExceptions without ogc prefix are returned by deegree. PDD.
        IPathNode exceptionTextNode = new PathNode("http://www.opengis.net/ows", "ExceptionText",
                                                   (NameTable)XmlReader.NameTable);
        CoordinatesNode = new AlternativePathNodesCollection(coordinatesNode, posListNode);
        ServiceExceptionNode = new AlternativePathNodesCollection(ogcServiceExceptionNode, exceptionTextNode,
                                                                   serviceExceptionNode);
        FeatureNode = new PathNode(FeatureTypeInfo.FeatureTypeNamespace, FeatureTypeInfo.Name,
                                    (NameTable)XmlReader.NameTable);
    }

    /// <summary>
    /// This method initializes separator variables for parsing coordinates.
    /// From GML specification: Coordinates can be included in a single string, but there is no 
    /// facility for validating string content. The value of the 'cs' attribute 
    /// is the separator for coordinate values, and the value of the 'ts' 
    /// attribute gives the tuple separator (a single space by default); the 
    /// default values may be changed to reflect local usage.
    /// </summary>
    private void InitializeSeparators()
    {
        var decimalDel = string.IsNullOrEmpty(FeatureTypeInfo.DecimalDel) ? ":" : FeatureTypeInfo.DecimalDel;
        _cs = string.IsNullOrEmpty(FeatureTypeInfo.Cs) ? "," : FeatureTypeInfo.Cs;
        _ts = string.IsNullOrEmpty(FeatureTypeInfo.Ts) ? " " : FeatureTypeInfo.Ts;
        _formatInfo.NumberDecimalSeparator = decimalDel;
    }



    /// <summary>
    /// This method closes the XmlReader member and the used <see cref="HttpClientUtil"/> instance.
    /// </summary>
    public virtual void Dispose()
    {
        XmlReader?.Close();
        _httpClientUtil?.Close();
    }
}

/// <summary>
/// This class produces instances of type <see cref="Point"/>.
/// The base class is <see cref="GeometryFactory"/>.
/// </summary>
internal class PointFactory : GeometryFactory
{

    /// <summary>
    /// Initializes a new instance of the <see cref="PointFactory"/> class.
    /// </summary>
    /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
    /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
    internal PointFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo)
        : base(httpClientUtil, featureTypeInfo)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PointFactory"/> class.
    /// This constructor shall just be called from the MultiPoint factory. The feature node therefore is deactivated.
    /// </summary>
    /// <param name="xmlReader">An XmlReader instance</param>
    /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
    internal PointFactory(XmlReader xmlReader, WfsFeatureTypeInfo featureTypeInfo)
        : base(xmlReader, featureTypeInfo)
    {
        FeatureNode!.IsActive = false;
    }



    /// <summary>
    /// This method produces instances of type <see cref="Point"/>.
    /// </summary>
    /// <returns>The created geometries</returns>
    internal override async Task<Collection<Geometry>> CreateGeometriesAsync(List<IFeature> features)
    {
        await InitAsync();
        IPathNode pointNode = new PathNode(Gmlns, "Point", (NameTable)XmlReader!.NameTable);
        var labelValues = new Dictionary<string, string>();
        var geomFound = false;

        try
        {
            // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
            while ((FeatureReader = GetSubReaderOf(XmlReader, null, FeatureNode)) != null)
            {
                while ((GeomReader = GetSubReaderOf(FeatureReader, labelValues, pointNode, CoordinatesNode)) != null)
                {
                    Geoms.Add(ParseCoordinates(GeomReader)[0].ToPoint());
                    geomFound = true;
                }
                if (geomFound) features.Add(AddLabel(labelValues, Geoms[Geoms.Count - 1]));
                geomFound = false;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "An exception occurred while parsing a point geometry string: " + ex.Message, ex);
            throw;
        }

        return Geoms;
    }

}

/// <summary>
/// This class produces instances of type <see cref="LineString"/>.
/// The base class is <see cref="GeometryFactory"/>.
/// </summary>
internal class LineStringFactory : GeometryFactory
{

    /// <summary>
    /// Initializes a new instance of the <see cref="LineStringFactory"/> class.
    /// </summary>
    /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
    /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
    internal LineStringFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo)
        : base(httpClientUtil, featureTypeInfo)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LineStringFactory"/> class.
    /// This constructor shall just be called from the MultiLineString factory. The feature node therefore is deactivated.
    /// </summary>
    /// <param name="xmlReader">An XmlReader instance</param>
    /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
    internal LineStringFactory(XmlReader xmlReader, WfsFeatureTypeInfo featureTypeInfo)
        : base(xmlReader, featureTypeInfo)
    {
        FeatureNode!.IsActive = false;
    }



    /// <summary>
    /// This method produces instances of type <see cref="LineString"/>.
    /// </summary>
    /// <returns>The created geometries</returns>
    internal override async Task<Collection<Geometry>> CreateGeometriesAsync(List<IFeature> features)
    {
        await InitAsync();
        IPathNode lineStringNode = new PathNode(Gmlns, "LineString", (NameTable)XmlReader!.NameTable);
        var labelValues = new Dictionary<string, string>();
        var geomFound = false;

        try
        {
            // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
            while ((FeatureReader = GetSubReaderOf(XmlReader, null, FeatureNode)) != null)
            {
                while (
                    (GeomReader = GetSubReaderOf(FeatureReader, labelValues, lineStringNode, CoordinatesNode)) !=
                    null)
                {
                    Geoms.Add(new LineString(ParseCoordinates(GeomReader).ToArray()));
                    geomFound = true;
                }
                if (geomFound) features.Add(AddLabel(labelValues, Geoms[Geoms.Count - 1]));
                geomFound = false;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "An exception occured while parsing a line geometry string: " + ex.Message, ex);
            throw;
        }

        return Geoms;
    }

}

/// <summary>
/// This class produces instances of type <see cref="Polygon"/>.
/// The base class is <see cref="GeometryFactory"/>.
/// </summary>
internal class PolygonFactory : GeometryFactory
{

    /// <summary>
    /// Initializes a new instance of the <see cref="PolygonFactory"/> class.
    /// </summary>
    /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
    /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
    internal PolygonFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo)
        : base(httpClientUtil, featureTypeInfo)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolygonFactory"/> class.
    /// This constructor shall just be called from the MultiPolygon factory. The feature node therefore is deactivated.
    /// </summary>
    /// <param name="xmlReader">An XmlReader instance</param>
    /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
    internal PolygonFactory(XmlReader xmlReader, WfsFeatureTypeInfo featureTypeInfo)
        : base(xmlReader, featureTypeInfo)
    {
        FeatureNode!.IsActive = false;
    }



    /// <summary>
    /// This method produces instances of type <see cref="Polygon"/>.
    /// </summary>
    /// <returns>The created geometries</returns>
    internal override async Task<Collection<Geometry>> CreateGeometriesAsync(List<IFeature> features)
    {
        await InitAsync();
        IPathNode polygonNode = new PathNode(Gmlns, "Polygon", (NameTable)XmlReader!.NameTable);
        IPathNode outerBoundaryNode = new PathNode(Gmlns, "outerBoundaryIs", (NameTable)XmlReader.NameTable);
        IPathNode exteriorNode = new PathNode(Gmlns, "exterior", (NameTable)XmlReader.NameTable);
        IPathNode outerBoundaryNodeAlt = new AlternativePathNodesCollection(outerBoundaryNode, exteriorNode);
        IPathNode innerBoundaryNode = new PathNode(Gmlns, "innerBoundaryIs", (NameTable)XmlReader.NameTable);
        IPathNode interiorNode = new PathNode(Gmlns, "interior", (NameTable)XmlReader.NameTable);
        IPathNode innerBoundaryNodeAlt = new AlternativePathNodesCollection(innerBoundaryNode, interiorNode);
        IPathNode linearRingNode = new PathNode(Gmlns, "LinearRing", (NameTable)XmlReader.NameTable);
        var labelValues = new Dictionary<string, string>();
        var geomFound = false;

        try
        {
            // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
            while ((FeatureReader = GetSubReaderOf(XmlReader, null, FeatureNode)) != null)
            {
                while ((GeomReader = GetSubReaderOf(FeatureReader, labelValues, polygonNode)) != null)
                {

                    LinearRing? exteriorRing = null;
                    XmlReader? outerBoundaryReader;
                    if ((outerBoundaryReader = GetSubReaderOf(
                            GeomReader, null, outerBoundaryNodeAlt, linearRingNode, CoordinatesNode)) != null)
                        exteriorRing = new LinearRing(ParseCoordinates(outerBoundaryReader).ToArray());

                    var holes = new List<LinearRing>();
                    XmlReader? innerBoundariesReader;
                    while ((innerBoundariesReader = GetSubReaderOf(
                               GeomReader, null, innerBoundaryNodeAlt, linearRingNode, CoordinatesNode)) != null)
                        holes.Add(
                            new LinearRing(ParseCoordinates(innerBoundariesReader).ToArray()));

                    if (exteriorRing is not null)
                    {
                        if (holes.Any())
                            Geoms.Add(new Polygon(exteriorRing, holes.ToArray()));
                        else
                            Geoms.Add(new Polygon(exteriorRing));
                        geomFound = true;
                    }
                }
                if (geomFound) features.Add(AddLabel(labelValues, Geoms[Geoms.Count - 1]));
                geomFound = false;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "An exception occurred while parsing a polygon geometry: " + ex.Message, ex);
            throw;
        }

        return Geoms;
    }

}

/// <summary>
/// This class produces instances of type <see cref="MultiPoint"/>.
/// The base class is <see cref="GeometryFactory"/>.
/// </summary>
internal class MultiPointFactory : GeometryFactory
{

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiPointFactory"/> class.
    /// </summary>
    /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
    /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the feature type to query</param>
    internal MultiPointFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo)
        : base(httpClientUtil, featureTypeInfo)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiPointFactory"/> class.
    /// </summary>
    /// <param name="xmlReader">An XmlReader instance</param>
    /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the feature type to query</param>
    internal MultiPointFactory(XmlReader xmlReader, WfsFeatureTypeInfo featureTypeInfo)
        : base(xmlReader, featureTypeInfo)
    {
    }



    /// <summary>
    /// This method produces instances of type <see cref="MultiPoint"/>.
    /// </summary>
    /// <returns>The created geometries</returns>
    internal override async Task<Collection<Geometry>> CreateGeometriesAsync(List<IFeature> features)
    {
        await InitAsync();
        IPathNode multiPointNode = new PathNode(Gmlns, "MultiPoint", (NameTable)XmlReader!.NameTable);
        IPathNode pointMemberNode = new PathNode(Gmlns, "pointMember", (NameTable)XmlReader.NameTable);
        var labelValues = new Dictionary<string, string>();
        var geomFound = false;

        try
        {
            // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
            while ((FeatureReader = GetSubReaderOf(XmlReader, null, FeatureNode)) != null)
            {
                while (
                    (GeomReader = GetSubReaderOf(FeatureReader, labelValues, multiPointNode, pointMemberNode)) !=
                    null)
                {
                    using GeometryFactory geomFactory = new PointFactory(GeomReader, FeatureTypeInfo) { AxisOrder = AxisOrder };
                    var points = (await geomFactory.CreateGeometriesAsync(features)).Cast<Point>();
                    Geoms.Add(new MultiPoint(points.ToArray()));
                    geomFound = true;
                }
                if (geomFound) features.Add(AddLabel(labelValues, Geoms[Geoms.Count - 1]));
                geomFound = false;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "An exception occured while parsing a multi-point geometry: " + ex.Message, ex);
            throw;
        }

        return Geoms;
    }

}

/// <summary>
/// This class produces objects of type <see cref="MultiLineString"/>.
/// The base class is <see cref="GeometryFactory"/>.
/// </summary>
internal class MultiLineStringFactory : GeometryFactory
{

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiLineStringFactory"/> class.
    /// </summary>
    /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
    /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
    internal MultiLineStringFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo)
        : base(httpClientUtil, featureTypeInfo)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiLineStringFactory"/> class.
    /// </summary>
    /// <param name="xmlReader">An XmlReader instance</param>
    /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
    internal MultiLineStringFactory(XmlReader xmlReader, WfsFeatureTypeInfo featureTypeInfo)
        : base(xmlReader, featureTypeInfo)
    {
    }



    /// <summary>
    /// This method produces instances of type <see cref="MultiLineString"/>.
    /// </summary>
    /// <returns>The created geometries</returns>
    internal override async Task<Collection<Geometry>> CreateGeometriesAsync(List<IFeature> features)
    {
        await InitAsync();
        IPathNode multiLineStringNode = new PathNode(Gmlns, "MultiLineString", (NameTable)XmlReader!.NameTable);
        IPathNode multiCurveNode = new PathNode(Gmlns, "MultiCurve", (NameTable)XmlReader.NameTable);
        IPathNode multiLineStringNodeAlt = new AlternativePathNodesCollection(multiLineStringNode, multiCurveNode);
        IPathNode lineStringMemberNode = new PathNode(Gmlns, "lineStringMember", (NameTable)XmlReader.NameTable);
        IPathNode curveMemberNode = new PathNode(Gmlns, "curveMember", (NameTable)XmlReader.NameTable);
        IPathNode lineStringMemberNodeAlt = new AlternativePathNodesCollection(lineStringMemberNode, curveMemberNode);
        var labelValues = new Dictionary<string, string>();
        var geomFound = false;

        try
        {
            // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
            while ((FeatureReader = GetSubReaderOf(XmlReader, null, FeatureNode)) != null)
            {
                while (
                    (GeomReader =
                     GetSubReaderOf(FeatureReader, labelValues, multiLineStringNodeAlt, lineStringMemberNodeAlt)) !=
                    null)
                {
                    using GeometryFactory geomFactory = new LineStringFactory(GeomReader, FeatureTypeInfo) { AxisOrder = AxisOrder };

                    var lineStrings = (await geomFactory.CreateGeometriesAsync(features)).Cast<LineString>();
                    Geoms.Add(new MultiLineString(lineStrings.ToArray()));
                    geomFound = true;
                }
                if (geomFound) features.Add(AddLabel(labelValues, Geoms[Geoms.Count - 1]));
                geomFound = false;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "An exception occurred while parsing a multi-lineString geometry: " + ex.Message, ex);
            throw;
        }

        return Geoms;
    }

}

/// <summary>
/// This class produces instances of type <see cref="MultiPolygon"/>.
/// The base class is <see cref="GeometryFactory"/>.
/// </summary>
internal class MultiPolygonFactory : GeometryFactory
{

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiPolygonFactory"/> class.
    /// </summary>
    /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
    /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
    internal MultiPolygonFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo)
        : base(httpClientUtil, featureTypeInfo)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiPolygonFactory"/> class.
    /// </summary>
    /// <param name="xmlReader">An XmlReader instance</param>
    /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
    internal MultiPolygonFactory(XmlReader xmlReader, WfsFeatureTypeInfo featureTypeInfo)
        : base(xmlReader, featureTypeInfo)
    {
    }

    /// <summary>
    /// This method produces instances of type <see cref="MultiPolygon"/>.
    /// </summary>
    /// <returns>The created geometries</returns>
    internal override async Task<Collection<Geometry>> CreateGeometriesAsync(List<IFeature> features)
    {
        await InitAsync();
        IPathNode multiPolygonNode = new PathNode(Gmlns, "MultiPolygon", (NameTable)XmlReader!.NameTable);
        IPathNode multiSurfaceNode = new PathNode(Gmlns, "MultiSurface", (NameTable)XmlReader.NameTable);
        IPathNode multiPolygonNodeAlt = new AlternativePathNodesCollection(multiPolygonNode, multiSurfaceNode);
        IPathNode polygonMemberNode = new PathNode(Gmlns, "polygonMember", (NameTable)XmlReader.NameTable);
        IPathNode surfaceMemberNode = new PathNode(Gmlns, "surfaceMember", (NameTable)XmlReader.NameTable);
        IPathNode polygonMemberNodeAlt = new AlternativePathNodesCollection(polygonMemberNode, surfaceMemberNode);
        IPathNode linearRingNode = new PathNode(Gmlns, "LinearRing", (NameTable)XmlReader.NameTable);
        var labelValues = new Dictionary<string, string>();
        var geomFound = false;

        try
        {
            // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
            while ((FeatureReader = GetSubReaderOf(XmlReader, null, FeatureNode)) != null)
            {
                while (
                    (GeomReader =
                     GetSubReaderOf(FeatureReader, labelValues, multiPolygonNodeAlt, polygonMemberNodeAlt)) != null)
                {
                    using GeometryFactory geomFactory = new PolygonFactory(GeomReader, FeatureTypeInfo) { AxisOrder = AxisOrder };
                    var polygons = (await geomFactory.CreateGeometriesAsync(features)).Cast<Polygon>();
                    Geoms.Add(new MultiPolygon(polygons.ToArray()));
                    geomFound = true;
                }
                if (geomFound) features.Add(AddLabel(labelValues, Geoms[Geoms.Count - 1]));
                geomFound = false;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "An exception occurred while parsing a multi-polygon geometry: " + ex.Message, ex);
            throw;
        }

        return Geoms;
    }

}

/// <summary>
/// This class must detect the geometry type of the queried layer.
/// Therefore it works a bit slower than the other factories. Specify the geometry type manually,
/// if it isn't specified in 'DescribeFeatureType'.
/// </summary>
internal class UnspecifiedGeometryFactoryWfs100Gml2 : GeometryFactory
{

    private readonly HttpClientUtil _httpClientUtil;
    private readonly bool _quickGeometries;
    private bool _multiGeometries;



    /// <summary>
    /// Initializes a new instance of the <see cref="UnspecifiedGeometryFactoryWfs100Gml2"/> class.
    /// </summary>
    /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
    /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
    /// <param name="multiGeometries">A boolean value specifying whether multi-geometries should be created</param>
    /// <param name="quickGeometries">A boolean value specifying whether the factory should create geometries quickly, but without validation</param>
    internal UnspecifiedGeometryFactoryWfs100Gml2(HttpClientUtil httpClientUtil,
                                                      WfsFeatureTypeInfo featureTypeInfo, bool multiGeometries,
                                                      bool quickGeometries)
        : base(httpClientUtil, featureTypeInfo)
    {
        _httpClientUtil = httpClientUtil;
        _multiGeometries = multiGeometries;
        _quickGeometries = quickGeometries;
    }



    /// <summary>
    /// This method detects the geometry type from 'GetFeature' response and uses a geometry factory to create the 
    /// appropriate geometries.
    /// </summary>
    /// <returns></returns>
    internal override async Task<Collection<Geometry>> CreateGeometriesAsync(List<IFeature> features)
    {
        await InitAsync();
        GeometryFactory? geomFactory = null;

        var geometryTypeString = string.Empty;

        if (_quickGeometries) _multiGeometries = false;

        IPathNode pointNode = new PathNode(Gmlns, "Point", (NameTable)XmlReader!.NameTable);
        IPathNode lineStringNode = new PathNode(Gmlns, "LineString", (NameTable)XmlReader.NameTable);
        IPathNode polygonNode = new PathNode(Gmlns, "Polygon", (NameTable)XmlReader.NameTable);
        IPathNode multiPointNode = new PathNode(Gmlns, "MultiPoint", (NameTable)XmlReader.NameTable);
        IPathNode multiLineStringNode = new PathNode(Gmlns, "MultiLineString", (NameTable)XmlReader.NameTable);
        IPathNode multiCurveNode = new PathNode(Gmlns, "MultiCurve", (NameTable)XmlReader.NameTable);
        IPathNode multiLineStringNodeAlt = new AlternativePathNodesCollection(multiLineStringNode, multiCurveNode);
        IPathNode multiPolygonNode = new PathNode(Gmlns, "MultiPolygon", (NameTable)XmlReader.NameTable);
        IPathNode multiSurfaceNode = new PathNode(Gmlns, "MultiSurface", (NameTable)XmlReader.NameTable);
        IPathNode multiPolygonNodeAlt = new AlternativePathNodesCollection(multiPolygonNode, multiSurfaceNode);

        while (await XmlReader.ReadAsync())
        {
            if (XmlReader.NodeType == XmlNodeType.Element)
            {
                if (_multiGeometries)
                {
                    if (multiPointNode.Matches(XmlReader))
                    {
                        geomFactory?.Dispose();
                        geomFactory = new MultiPointFactory(_httpClientUtil, FeatureTypeInfo);
                        geometryTypeString = "MultiPointPropertyType";
                        break;
                    }
                    if (multiLineStringNodeAlt.Matches(XmlReader))
                    {
                        geomFactory?.Dispose();
                        geomFactory = new MultiLineStringFactory(_httpClientUtil, FeatureTypeInfo);
                        geometryTypeString = "MultiLineStringPropertyType";
                        break;
                    }
                    if (multiPolygonNodeAlt.Matches(XmlReader))
                    {
                        geomFactory?.Dispose();
                        geomFactory = new MultiPolygonFactory(_httpClientUtil, FeatureTypeInfo);
                        geometryTypeString = "MultiPolygonPropertyType";
                        break;
                    }
                }

                if (pointNode.Matches(XmlReader))
                {
                    geomFactory?.Dispose();
                    geomFactory = new PointFactory(_httpClientUtil, FeatureTypeInfo);
                    geometryTypeString = "PointPropertyType";
                    break;
                }
                if (lineStringNode.Matches(XmlReader))
                {
                    geomFactory?.Dispose();
                    geomFactory = new LineStringFactory(_httpClientUtil, FeatureTypeInfo);
                    geometryTypeString = "LineStringPropertyType";
                    break;
                }
                if (polygonNode.Matches(XmlReader))
                {
                    geomFactory?.Dispose();
                    geomFactory = new PolygonFactory(_httpClientUtil, FeatureTypeInfo);
                    geometryTypeString = "PolygonPropertyType";
                    break;
                }
                if (ServiceExceptionNode?.Matches(XmlReader) ?? false)
                {
                    var serviceException = await XmlReader.ReadInnerXmlAsync();
                    Trace.TraceError("A service exception occured: " + serviceException);
                    throw new Exception("A service exception occured: " + serviceException);
                }
            }
        }

        FeatureTypeInfo.Geometry.GeometryType = geometryTypeString;

        if (geomFactory == null) return Geoms;

        await geomFactory.InitAsync();
        geomFactory.AxisOrder = AxisOrder;
        await geomFactory.CreateGeometriesAsync(features);
        return Geoms;
    }
}
