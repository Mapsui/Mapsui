// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using SharpMap.Data;
using SharpMap.Geometries;

namespace SharpMap.Utilities.Wfs
{
    /// <summary>
    /// This class is the base class for geometry production.
    /// It provides some parsing routines for XML compliant to GML2/GML3.
    /// </summary>
    internal abstract class GeometryFactory : IDisposable
    {
        #region Fields

        protected const string _GMLNS = "http://www.opengis.net/gml";
        private readonly NumberFormatInfo _FormatInfo = new NumberFormatInfo();
        private readonly HttpClientUtil _HttpClientUtil;
        private readonly List<IPathNode> _PathNodes = new List<IPathNode>();
        protected AlternativePathNodesCollection _CoordinatesNode;
        private string _Cs;
        protected IPathNode _FeatureNode;
        protected XmlReader _FeatureReader;
        protected WfsFeatureTypeInfo _FeatureTypeInfo;
        protected XmlReader _GeomReader;

        protected Collection<Geometry> _Geoms = new Collection<Geometry>();

        protected FeatureDataTable _LabelInfo;
        protected IPathNode _LabelNode;
        protected AlternativePathNodesCollection _ServiceExceptionNode;
        private string _Ts;
        protected XmlReader _XmlReader;

        #endregion

        #region Constructors

        /// <summary>
        /// Protected constructor for the abstract class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelInfo">A FeatureDataTable for labels</param>
        protected GeometryFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo,
                                  FeatureDataTable labelInfo)
        {
            _FeatureTypeInfo = featureTypeInfo;
            _HttpClientUtil = httpClientUtil;
            createReader(httpClientUtil);

            try
            {
                if (labelInfo != null)
                {
                    _LabelInfo = labelInfo;
                    _LabelNode = new PathNode(_FeatureTypeInfo.FeatureTypeNamespace, _LabelInfo.Columns[0].ColumnName,
                                              (NameTable) _XmlReader.NameTable);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while initializing the label path node!");
                throw ex;
            }

            initializePathNodes();
            initializeSeparators();
        }

        /// <summary>
        /// Protected constructor for the abstract class.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        protected GeometryFactory(XmlReader xmlReader, WfsFeatureTypeInfo featureTypeInfo)
        {
            _FeatureTypeInfo = featureTypeInfo;
            _XmlReader = xmlReader;
            initializePathNodes();
            initializeSeparators();
        }

        #endregion

        #region Internal Member

        /// <summary>
        /// Abstract method - overwritten by derived classes for producing instances
        /// derived from <see cref="SharpMap.Geometries.Geometry"/>.
        /// </summary>
        internal abstract Collection<Geometry> createGeometries();

        /// <summary>
        /// This method parses quickly without paying attention to
        /// context validation, polygon boundaries and multi-geometries.
        /// This accelerates the geometry parsing process, 
        /// but in scarce cases can lead to errors. 
        /// </summary>
        /// <param name="geometryType">The geometry type (Point, LineString, Polygon, MultiPoint, MultiCurve, 
        /// MultiLineString (deprecated), MultiSurface, MultiPolygon (deprecated)</param>
        /// <returns>The created geometries</returns>
        internal virtual Collection<Geometry> createQuickGeometries(string geometryType)
        {
            // Ignore multi-geometries
            if (geometryType.Equals("MultiPointPropertyType")) geometryType = "PointPropertyType";
            else if (geometryType.Equals("MultiLineStringPropertyType")) geometryType = "LineStringPropertyType";
            else if (geometryType.Equals("MultiPolygonPropertyType")) geometryType = "PolygonPropertyType";
            else if (geometryType.Equals("MultiCurvePropertyType")) geometryType = "CurvePropertyType";
            else if (geometryType.Equals("MultiSurfacePropertyType")) geometryType = "SurfacePropertyType";

            string serviceException = null;

            while (_XmlReader.Read())
            {
                if (_CoordinatesNode.Matches(_XmlReader))
                {
                    try
                    {
                        switch (geometryType)
                        {
                            case "PointPropertyType":
                                _Geoms.Add(ParseCoordinates(_XmlReader.ReadSubtree())[0]);
                                break;
                            case "LineStringPropertyType":
                            case "CurvePropertyType":
                                _Geoms.Add(new LineString(ParseCoordinates(_XmlReader.ReadSubtree())));
                                break;
                            case "PolygonPropertyType":
                            case "SurfacePropertyType":
                                Polygon polygon = new Polygon();
                                polygon.ExteriorRing = new LinearRing(ParseCoordinates(_XmlReader.ReadSubtree()));
                                _Geoms.Add(polygon);
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("An exception occured while parsing a " + geometryType + " geometry: " +
                                         ex.Message);
                        throw ex;
                    }
                    continue;
                }

                if (_ServiceExceptionNode.Matches(_XmlReader))
                {
                    serviceException = _XmlReader.ReadInnerXml();
                    Trace.TraceError("A service exception occured: " + serviceException);
                    throw new Exception("A service exception occured: " + serviceException);
                }
            }

            return _Geoms;
        }

        #endregion

        #region Protected Member

        /// <summary>
        /// This method parses a coordinates or posList(from 'GetFeature' response). 
        /// </summary>
        /// <param name="reader">An XmlReader instance at the position of the coordinates to read</param>
        /// <returns>A point collection (the collected coordinates)</returns>
        protected Collection<Point> ParseCoordinates(XmlReader reader)
        {
            if (!reader.Read()) return null;

            string name = reader.LocalName;
            string coordinateString = reader.ReadElementString();
            Collection<Point> vertices = new Collection<Point>();
            string[] coordinateValues;
            int i = 0, length = 0;

            if (name.Equals("coordinates"))
                coordinateValues = coordinateString.Split(_Cs[0], _Ts[0]);
            else
                coordinateValues = coordinateString.Split(' ');

            length = coordinateValues.Length;

            while (i < length - 1)
            {
                double c1 = Convert.ToDouble(coordinateValues[i++], _FormatInfo);
                double c2 = Convert.ToDouble(coordinateValues[i++], _FormatInfo);

                if (name.Equals("coordinates"))
                    vertices.Add(new Point(c1, c2));
                else
                    vertices.Add(new Point(c2, c1));
            }

            return vertices;
        }

        /// <summary>
        /// This method retrieves an XmlReader within a specified context.
        /// </summary>
        /// <param name="reader">An XmlReader instance that is the origin of a created sub-reader</param>
        /// <param name="labelValue">A string array for recording a found label value. Pass 'null' to ignore searching for label values</param>
        /// <param name="pathNodes">A list of <see cref="PathNodeDepr"/> instances defining the context of the retrieved reader</param>
        /// <returns>A sub-reader of the XmlReader given as argument</returns>
        protected XmlReader GetSubReaderOf(XmlReader reader, string[] labelValue, params IPathNode[] pathNodes)
        {
            _PathNodes.Clear();
            _PathNodes.AddRange(pathNodes);
            return GetSubReaderOf(reader, labelValue, _PathNodes);
        }

        /// <summary>
        /// This method retrieves an XmlReader within a specified context.
        /// Moreover it collects label values before or after a geometry could be found.
        /// </summary>
        /// <param name="reader">An XmlReader instance that is the origin of a created sub-reader</param>
        /// <param name="labelValue">A string array for recording a found label value. Pass 'null' to ignore searching for label values</param>
        /// <param name="pathNodes">A list of <see cref="PathNodeDepr"/> instances defining the context of the retrieved reader</param>
        /// <returns>A sub-reader of the XmlReader given as argument</returns>
        protected XmlReader GetSubReaderOf(XmlReader reader, string[] labelValue, List<IPathNode> pathNodes)
        {
            string errorMessage = null;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (pathNodes[0].Matches(reader))
                    {
                        pathNodes.RemoveAt(0);

                        if (pathNodes.Count > 0)
                            return GetSubReaderOf(reader.ReadSubtree(), null, pathNodes);

                        return reader.ReadSubtree();
                    }

                    if (labelValue != null)
                        if (_LabelNode != null)
                            if (_LabelNode.Matches(reader))
                                labelValue[0] = reader.ReadElementString();


                    if (_ServiceExceptionNode.Matches(reader))
                    {
                        errorMessage = reader.ReadInnerXml();
                        Trace.TraceError("A service exception occured: " + errorMessage);
                        throw new Exception("A service exception occured: " + errorMessage);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// This method adds a label to the collection.
        /// </summary>
        protected void AddLabel(string labelValue, Geometry geom)
        {
            if (_LabelInfo == null || geom == null || string.IsNullOrEmpty(labelValue)) return;

            try
            {
                FeatureDataRow row = _LabelInfo.NewRow();
                row[0] = labelValue;
                row.Geometry = geom;
                _LabelInfo.AddRow(row);
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while adding a label to the collection!");
                throw ex;
            }
        }

        #endregion

        #region Private Member

        /// <summary>
        /// This method initializes the XmlReader member.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        private void createReader(HttpClientUtil httpClientUtil)
        {
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.IgnoreProcessingInstructions = true;
            xmlReaderSettings.IgnoreWhitespace = true;
            xmlReaderSettings.ProhibitDtd = true;
            _XmlReader = XmlReader.Create(httpClientUtil.GetDataStream(), xmlReaderSettings);
        }

        /// <summary>
        /// This method initializes path nodes needed by the derived classes.
        /// </summary>
        private void initializePathNodes()
        {
            IPathNode coordinatesNode = new PathNode("http://www.opengis.net/gml", "coordinates",
                                                     (NameTable) _XmlReader.NameTable);
            IPathNode posListNode = new PathNode("http://www.opengis.net/gml", "posList",
                                                 (NameTable) _XmlReader.NameTable);
            IPathNode ogcServiceExceptionNode = new PathNode("http://www.opengis.net/ogc", "ServiceException",
                                                             (NameTable) _XmlReader.NameTable);
            IPathNode serviceExceptionNode = new PathNode("", "ServiceException", (NameTable) _XmlReader.NameTable);
                //ServiceExceptions without ogc prefix are returned by deegree. PDD.
            IPathNode exceptionTextNode = new PathNode("http://www.opengis.net/ows", "ExceptionText",
                                                       (NameTable) _XmlReader.NameTable);
            _CoordinatesNode = new AlternativePathNodesCollection(coordinatesNode, posListNode);
            _ServiceExceptionNode = new AlternativePathNodesCollection(ogcServiceExceptionNode, exceptionTextNode,
                                                                       serviceExceptionNode);
            _FeatureNode = new PathNode(_FeatureTypeInfo.FeatureTypeNamespace, _FeatureTypeInfo.Name,
                                        (NameTable) _XmlReader.NameTable);
        }

        /// <summary>
        /// This method initializes separator variables for parsing coordinates.
        /// From GML specification: Coordinates can be included in a single string, but there is no 
        /// facility for validating string content. The value of the 'cs' attribute 
        /// is the separator for coordinate values, and the value of the 'ts' 
        /// attribute gives the tuple separator (a single space by default); the 
        /// default values may be changed to reflect local usage.
        /// </summary>
        private void initializeSeparators()
        {
            string decimalDel = string.IsNullOrEmpty(_FeatureTypeInfo.DecimalDel) ? ":" : _FeatureTypeInfo.DecimalDel;
            _Cs = string.IsNullOrEmpty(_FeatureTypeInfo.Cs) ? "," : _FeatureTypeInfo.Cs;
            _Ts = string.IsNullOrEmpty(_FeatureTypeInfo.Ts) ? " " : _FeatureTypeInfo.Ts;
            _FormatInfo.NumberDecimalSeparator = decimalDel;
        }

        #endregion

        #region IDisposable Member

        /// <summary>
        /// This method closes the XmlReader member and the used <see cref="HttpClientUtil"/> instance.
        /// </summary>
        public void Dispose()
        {
            if (_XmlReader != null)
                _XmlReader.Close();
            if (_HttpClientUtil != null)
                _HttpClientUtil.Close();
        }

        #endregion
    }

    /// <summary>
    /// This class produces instances of type <see cref="SharpMap.Geometries.Point"/>.
    /// The base class is <see cref="GeometryFactory"/>.
    /// </summary>
    internal class PointFactory : GeometryFactory
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PointFactory"/> class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelInfo">A FeatureDataTable for labels</param>
        internal PointFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo,
                              FeatureDataTable labelInfo) : base(httpClientUtil, featureTypeInfo, labelInfo)
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
            _FeatureNode.IsActive = false;
        }

        #endregion

        #region Internal Member

        /// <summary>
        /// This method produces instances of type <see cref="SharpMap.Geometries.Point"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<Geometry> createGeometries()
        {
            IPathNode pointNode = new PathNode(_GMLNS, "Point", (NameTable) _XmlReader.NameTable);
            string[] labelValue = new string[1];
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((_FeatureReader = GetSubReaderOf(_XmlReader, null, _FeatureNode)) != null)
                {
                    while ((_GeomReader = GetSubReaderOf(_FeatureReader, labelValue, pointNode, _CoordinatesNode)) !=
                           null)
                    {
                        _Geoms.Add(ParseCoordinates(_GeomReader)[0]);
                        geomFound = true;
                    }
                    if (geomFound) AddLabel(labelValue[0], _Geoms[_Geoms.Count - 1]);
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a point geometry string: " + ex.Message);
                throw ex;
            }

            return _Geoms;
        }

        #endregion
    }

    /// <summary>
    /// This class produces instances of type <see cref="SharpMap.Geometries.LineString"/>.
    /// The base class is <see cref="GeometryFactory"/>.
    /// </summary>
    internal class LineStringFactory : GeometryFactory
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LineStringFactory"/> class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelInfo">A FeatureDataTable for labels</param>
        internal LineStringFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo,
                                   FeatureDataTable labelInfo) : base(httpClientUtil, featureTypeInfo, labelInfo)
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
            _FeatureNode.IsActive = false;
        }

        #endregion

        #region Internal Member

        /// <summary>
        /// This method produces instances of type <see cref="SharpMap.Geometries.LineString"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<Geometry> createGeometries()
        {
            IPathNode lineStringNode = new PathNode(_GMLNS, "LineString", (NameTable) _XmlReader.NameTable);
            string[] labelValue = new string[1];
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((_FeatureReader = GetSubReaderOf(_XmlReader, null, _FeatureNode)) != null)
                {
                    while (
                        (_GeomReader = GetSubReaderOf(_FeatureReader, labelValue, lineStringNode, _CoordinatesNode)) !=
                        null)
                    {
                        _Geoms.Add(new LineString(ParseCoordinates(_GeomReader)));
                        geomFound = true;
                    }
                    if (geomFound) AddLabel(labelValue[0], _Geoms[_Geoms.Count - 1]);
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a line geometry string: " + ex.Message);
                throw ex;
            }

            return _Geoms;
        }

        #endregion
    }

    /// <summary>
    /// This class produces instances of type <see cref="SharpMap.Geometries.Polygon"/>.
    /// The base class is <see cref="GeometryFactory"/>.
    /// </summary>
    internal class PolygonFactory : GeometryFactory
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PolygonFactory"/> class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelInfo">A FeatureDataTable for labels</param>
        internal PolygonFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo,
                                FeatureDataTable labelInfo) : base(httpClientUtil, featureTypeInfo, labelInfo)
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
            _FeatureNode.IsActive = false;
        }

        #endregion

        #region Internal Member

        /// <summary>
        /// This method produces instances of type <see cref="SharpMap.Geometries.Polygon"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<Geometry> createGeometries()
        {
            Polygon polygon = null;
            XmlReader outerBoundaryReader = null;
            XmlReader innerBoundariesReader = null;

            IPathNode polygonNode = new PathNode(_GMLNS, "Polygon", (NameTable) _XmlReader.NameTable);
            IPathNode outerBoundaryNode = new PathNode(_GMLNS, "outerBoundaryIs", (NameTable) _XmlReader.NameTable);
            IPathNode exteriorNode = new PathNode(_GMLNS, "exterior", (NameTable) _XmlReader.NameTable);
            IPathNode outerBoundaryNodeAlt = new AlternativePathNodesCollection(outerBoundaryNode, exteriorNode);
            IPathNode innerBoundaryNode = new PathNode(_GMLNS, "innerBoundaryIs", (NameTable) _XmlReader.NameTable);
            IPathNode interiorNode = new PathNode(_GMLNS, "interior", (NameTable) _XmlReader.NameTable);
            IPathNode innerBoundaryNodeAlt = new AlternativePathNodesCollection(innerBoundaryNode, interiorNode);
            IPathNode linearRingNode = new PathNode(_GMLNS, "LinearRing", (NameTable) _XmlReader.NameTable);
            string[] labelValue = new string[1];
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((_FeatureReader = GetSubReaderOf(_XmlReader, null, _FeatureNode)) != null)
                {
                    while ((_GeomReader = GetSubReaderOf(_FeatureReader, labelValue, polygonNode)) != null)
                    {
                        polygon = new Polygon();

                        if (
                            (outerBoundaryReader =
                             GetSubReaderOf(_GeomReader, null, outerBoundaryNodeAlt, linearRingNode, _CoordinatesNode)) !=
                            null)
                            polygon.ExteriorRing = new LinearRing(ParseCoordinates(outerBoundaryReader));

                        while (
                            (innerBoundariesReader =
                             GetSubReaderOf(_GeomReader, null, innerBoundaryNodeAlt, linearRingNode, _CoordinatesNode)) !=
                            null)
                            polygon.InteriorRings.Add(new LinearRing(ParseCoordinates(innerBoundariesReader)));

                        _Geoms.Add(polygon);
                        geomFound = true;
                    }
                    if (geomFound) AddLabel(labelValue[0], _Geoms[_Geoms.Count - 1]);
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a polygon geometry: " + ex.Message);
                throw ex;
            }

            return _Geoms;
        }

        #endregion
    }

    /// <summary>
    /// This class produces instances of type <see cref="SharpMap.Geometries.MultiPoint"/>.
    /// The base class is <see cref="GeometryFactory"/>.
    /// </summary>
    internal class MultiPointFactory : GeometryFactory
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPointFactory"/> class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelInfo">A FeatureDataTable for labels</param>
        internal MultiPointFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo,
                                   FeatureDataTable labelInfo) : base(httpClientUtil, featureTypeInfo, labelInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPointFactory"/> class.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        internal MultiPointFactory(XmlReader xmlReader, WfsFeatureTypeInfo featureTypeInfo)
            : base(xmlReader, featureTypeInfo)
        {
        }

        #endregion

        #region Internal Member

        /// <summary>
        /// This method produces instances of type <see cref="SharpMap.Geometries.MultiPoint"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<Geometry> createGeometries()
        {
            MultiPoint multiPoint = null;

            IPathNode multiPointNode = new PathNode(_GMLNS, "MultiPoint", (NameTable) _XmlReader.NameTable);
            IPathNode pointMemberNode = new PathNode(_GMLNS, "pointMember", (NameTable) _XmlReader.NameTable);
            string[] labelValue = new string[1];
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((_FeatureReader = GetSubReaderOf(_XmlReader, null, _FeatureNode)) != null)
                {
                    while (
                        (_GeomReader = GetSubReaderOf(_FeatureReader, labelValue, multiPointNode, pointMemberNode)) !=
                        null)
                    {
                        multiPoint = new MultiPoint();
                        GeometryFactory geomFactory = new PointFactory(_GeomReader, _FeatureTypeInfo);
                        Collection<Geometry> points = geomFactory.createGeometries();

                        foreach (Point point in points)
                            multiPoint.Points.Add(point);

                        _Geoms.Add(multiPoint);
                        geomFound = true;
                    }
                    if (geomFound) AddLabel(labelValue[0], _Geoms[_Geoms.Count - 1]);
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a multi-point geometry: " + ex.Message);
                throw ex;
            }

            return _Geoms;
        }

        #endregion
    }

    /// <summary>
    /// This class produces objects of type <see cref="SharpMap.Geometries.MultiLineString"/>.
    /// The base class is <see cref="GeometryFactory"/>.
    /// </summary>
    internal class MultiLineStringFactory : GeometryFactory
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLineStringFactory"/> class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelInfo">A FeatureDataTable for labels</param>
        internal MultiLineStringFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo,
                                        FeatureDataTable labelInfo) : base(httpClientUtil, featureTypeInfo, labelInfo)
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

        #endregion

        #region Internal Member

        /// <summary>
        /// This method produces instances of type <see cref="SharpMap.Geometries.MultiLineString"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<Geometry> createGeometries()
        {
            MultiLineString multiLineString = null;

            IPathNode multiLineStringNode = new PathNode(_GMLNS, "MultiLineString", (NameTable) _XmlReader.NameTable);
            IPathNode multiCurveNode = new PathNode(_GMLNS, "MultiCurve", (NameTable) _XmlReader.NameTable);
            IPathNode multiLineStringNodeAlt = new AlternativePathNodesCollection(multiLineStringNode, multiCurveNode);
            IPathNode lineStringMemberNode = new PathNode(_GMLNS, "lineStringMember", (NameTable) _XmlReader.NameTable);
            IPathNode curveMemberNode = new PathNode(_GMLNS, "curveMember", (NameTable) _XmlReader.NameTable);
            IPathNode lineStringMemberNodeAlt = new AlternativePathNodesCollection(lineStringMemberNode, curveMemberNode);
            string[] labelValue = new string[1];
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((_FeatureReader = GetSubReaderOf(_XmlReader, null, _FeatureNode)) != null)
                {
                    while (
                        (_GeomReader =
                         GetSubReaderOf(_FeatureReader, labelValue, multiLineStringNodeAlt, lineStringMemberNodeAlt)) !=
                        null)
                    {
                        multiLineString = new MultiLineString();
                        GeometryFactory geomFactory = new LineStringFactory(_GeomReader, _FeatureTypeInfo);
                        Collection<Geometry> lineStrings = geomFactory.createGeometries();

                        foreach (LineString lineString in lineStrings)
                            multiLineString.LineStrings.Add(lineString);

                        _Geoms.Add(multiLineString);
                        geomFound = true;
                    }
                    if (geomFound) AddLabel(labelValue[0], _Geoms[_Geoms.Count - 1]);
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a multi-lineString geometry: " + ex.Message);
                throw ex;
            }

            return _Geoms;
        }

        #endregion
    }

    /// <summary>
    /// This class produces instances of type <see cref="SharpMap.Geometries.MultiPolygon"/>.
    /// The base class is <see cref="GeometryFactory"/>.
    /// </summary>
    internal class MultiPolygonFactory : GeometryFactory
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPolygonFactory"/> class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelInfo">A FeatureDataTable for labels</param>
        internal MultiPolygonFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo,
                                     FeatureDataTable labelInfo) : base(httpClientUtil, featureTypeInfo, labelInfo)
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

        #endregion

        #region Internal Member

        /// <summary>
        /// This method produces instances of type <see cref="SharpMap.Geometries.MultiPolygon"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<Geometry> createGeometries()
        {
            MultiPolygon multiPolygon = null;

            IPathNode multiPolygonNode = new PathNode(_GMLNS, "MultiPolygon", (NameTable) _XmlReader.NameTable);
            IPathNode multiSurfaceNode = new PathNode(_GMLNS, "MultiSurface", (NameTable) _XmlReader.NameTable);
            IPathNode multiPolygonNodeAlt = new AlternativePathNodesCollection(multiPolygonNode, multiSurfaceNode);
            IPathNode polygonMemberNode = new PathNode(_GMLNS, "polygonMember", (NameTable) _XmlReader.NameTable);
            IPathNode surfaceMemberNode = new PathNode(_GMLNS, "surfaceMember", (NameTable) _XmlReader.NameTable);
            IPathNode polygonMemberNodeAlt = new AlternativePathNodesCollection(polygonMemberNode, surfaceMemberNode);
            IPathNode linearRingNode = new PathNode(_GMLNS, "LinearRing", (NameTable) _XmlReader.NameTable);
            string[] labelValue = new string[1];
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((_FeatureReader = GetSubReaderOf(_XmlReader, null, _FeatureNode)) != null)
                {
                    while (
                        (_GeomReader =
                         GetSubReaderOf(_FeatureReader, labelValue, multiPolygonNodeAlt, polygonMemberNodeAlt)) != null)
                    {
                        multiPolygon = new MultiPolygon();
                        GeometryFactory geomFactory = new PolygonFactory(_GeomReader, _FeatureTypeInfo);
                        Collection<Geometry> polygons = geomFactory.createGeometries();

                        foreach (Polygon polygon in polygons)
                            multiPolygon.Polygons.Add(polygon);

                        _Geoms.Add(multiPolygon);
                        geomFound = true;
                    }
                    if (geomFound) AddLabel(labelValue[0], _Geoms[_Geoms.Count - 1]);
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a multi-polygon geometry: " + ex.Message);
                throw ex;
            }

            return _Geoms;
        }

        #endregion
    }

    /// <summary>
    /// This class must detect the geometry type of the queried layer.
    /// Therefore it works a bit slower than the other factories. Specify the geometry type manually,
    /// if it isn't specified in 'DescribeFeatureType'.
    /// </summary>
    internal class UnspecifiedGeometryFactory_WFS1_0_0_GML2 : GeometryFactory
    {
        #region Fields

        private readonly HttpClientUtil _HttpClientUtil;
        private readonly bool _QuickGeometries;
        private bool _MultiGeometries;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UnspecifiedGeometryFactory_WFS1_0_0_GML2"/> class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="multiGeometries">A boolean value specifying whether multi-geometries should be created</param>
        /// <param name="quickGeometries">A boolean value specifying whether the factory should create geometries quickly, but without validation</param>
        /// <param name="labelInfo">A FeatureDataTable for labels</param>
        internal UnspecifiedGeometryFactory_WFS1_0_0_GML2(HttpClientUtil httpClientUtil,
                                                          WfsFeatureTypeInfo featureTypeInfo, bool multiGeometries,
                                                          bool quickGeometries, FeatureDataTable labelInfo)
            : base(httpClientUtil, featureTypeInfo, labelInfo)
        {
            _HttpClientUtil = httpClientUtil;
            _MultiGeometries = multiGeometries;
            _QuickGeometries = quickGeometries;
        }

        #endregion

        #region Internal Member

        /// <summary>
        /// This method detects the geometry type from 'GetFeature' response and uses a geometry factory to create the 
        /// appropriate geometries.
        /// </summary>
        /// <returns></returns>
        internal override Collection<Geometry> createGeometries()
        {
            GeometryFactory geomFactory = null;

            string geometryTypeString = string.Empty;
            string serviceException = null;

            if (_QuickGeometries) _MultiGeometries = false;

            IPathNode pointNode = new PathNode(_GMLNS, "Point", (NameTable) _XmlReader.NameTable);
            IPathNode lineStringNode = new PathNode(_GMLNS, "LineString", (NameTable) _XmlReader.NameTable);
            IPathNode polygonNode = new PathNode(_GMLNS, "Polygon", (NameTable) _XmlReader.NameTable);
            IPathNode multiPointNode = new PathNode(_GMLNS, "MultiPoint", (NameTable) _XmlReader.NameTable);
            IPathNode multiLineStringNode = new PathNode(_GMLNS, "MultiLineString", (NameTable) _XmlReader.NameTable);
            IPathNode multiCurveNode = new PathNode(_GMLNS, "MultiCurve", (NameTable) _XmlReader.NameTable);
            IPathNode multiLineStringNodeAlt = new AlternativePathNodesCollection(multiLineStringNode, multiCurveNode);
            IPathNode multiPolygonNode = new PathNode(_GMLNS, "MultiPolygon", (NameTable) _XmlReader.NameTable);
            IPathNode multiSurfaceNode = new PathNode(_GMLNS, "MultiSurface", (NameTable) _XmlReader.NameTable);
            IPathNode multiPolygonNodeAlt = new AlternativePathNodesCollection(multiPolygonNode, multiSurfaceNode);

            while (_XmlReader.Read())
            {
                if (_XmlReader.NodeType == XmlNodeType.Element)
                {
                    if (_MultiGeometries)
                    {
                        if (multiPointNode.Matches(_XmlReader))
                        {
                            geomFactory = new MultiPointFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                            geometryTypeString = "MultiPointPropertyType";
                            break;
                        }
                        if (multiLineStringNodeAlt.Matches(_XmlReader))
                        {
                            geomFactory = new MultiLineStringFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                            geometryTypeString = "MultiLineStringPropertyType";
                            break;
                        }
                        if (multiPolygonNodeAlt.Matches(_XmlReader))
                        {
                            geomFactory = new MultiPolygonFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                            geometryTypeString = "MultiPolygonPropertyType";
                            break;
                        }
                    }

                    if (pointNode.Matches(_XmlReader))
                    {
                        geomFactory = new PointFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        geometryTypeString = "PointPropertyType";
                        break;
                    }
                    if (lineStringNode.Matches(_XmlReader))
                    {
                        geomFactory = new LineStringFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        geometryTypeString = "LineStringPropertyType";
                        break;
                    }
                    if (polygonNode.Matches(_XmlReader))
                    {
                        geomFactory = new PolygonFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        geometryTypeString = "PolygonPropertyType";
                        break;
                    }
                    if (_ServiceExceptionNode.Matches(_XmlReader))
                    {
                        serviceException = _XmlReader.ReadInnerXml();
                        Trace.TraceError("A service exception occured: " + serviceException);
                        throw new Exception("A service exception occured: " + serviceException);
                    }
                }
            }

            _FeatureTypeInfo.Geometry._GeometryType = geometryTypeString;

            if (geomFactory != null)
                return _QuickGeometries
                           ? geomFactory.createQuickGeometries(geometryTypeString)
                           : geomFactory.createGeometries();
            return _Geoms;
        }

        #endregion
    }
}