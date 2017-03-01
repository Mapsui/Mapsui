// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using Mapsui.Geometries;

namespace Mapsui.Providers.Wfs.Utilities
{
    /// <summary>
    /// This class is the base class for geometry production.
    /// It provides some parsing routines for XML compliant to GML2/GML3.
    /// </summary>
    internal abstract class GeometryFactory : IDisposable
    {
        
        protected const string Gmlns = "http://www.opengis.net/gml";
        private readonly NumberFormatInfo _formatInfo = new NumberFormatInfo();
        private readonly HttpClientUtil _httpClientUtil;
        private readonly List<IPathNode> _pathNodes = new List<IPathNode>();
        protected AlternativePathNodesCollection CoordinatesNode;
        private string _cs;
        protected IPathNode FeatureNode;
        protected XmlReader FeatureReader;
        protected WfsFeatureTypeInfo FeatureTypeInfo;
        protected XmlReader GeomReader;
        protected Collection<Geometry> Geoms = new Collection<Geometry>();
        protected IPathNode LabelNode;
        protected AlternativePathNodesCollection ServiceExceptionNode;
        private string _ts;
        protected XmlReader XmlReader;

        
        
        /// <summary>
        /// Protected constructor for the abstract class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        protected GeometryFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo)
        {
            FeatureTypeInfo = featureTypeInfo;
            _httpClientUtil = httpClientUtil;
            CreateReader(httpClientUtil);

            try
            {
                if (featureTypeInfo.LableField != null)
                {
                    LabelNode = new PathNode(FeatureTypeInfo.FeatureTypeNamespace, featureTypeInfo.LableField, 
                                              (NameTable) XmlReader.NameTable);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while initializing the label path node! " + ex.Message);
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
        /// derived from <see cref="Mapsui.Geometries.Geometry"/>.
        /// </summary>
        internal abstract Collection<Geometry> CreateGeometries(Features features);
        
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
            var vertices = new Collection<Point>();
            int i = 0;

            string[] coordinateValues = name.Equals("coordinates") ? coordinateString.Split(_cs[0], _ts[0]) 
                : coordinateString.Split(' ');

            int length = coordinateValues.Length;

            while (i < length - 1)
            {
                double c1 = Convert.ToDouble(coordinateValues[i++], _formatInfo);
                double c2 = Convert.ToDouble(coordinateValues[i++], _formatInfo);

                vertices.Add(name.Equals("coordinates") ? new Point(c1, c2) : new Point(c2, c1));
            }

            return vertices;
        }

        /// <summary>
        /// This method retrieves an XmlReader within a specified context.
        /// </summary>
        /// <param name="reader">An XmlReader instance that is the origin of a created sub-reader</param>
        /// <param name="labelValue">A string array for recording a found label value. Pass 'null' to ignore searching for label values</param>
        /// <param name="pathNodes">A list of <see cref="IPathNode"/> instances defining the context of the retrieved reader</param>
        /// <returns>A sub-reader of the XmlReader given as argument</returns>
        protected XmlReader GetSubReaderOf(XmlReader reader, string[] labelValue, params IPathNode[] pathNodes)
        {
            _pathNodes.Clear();
            _pathNodes.AddRange(pathNodes);
            return GetSubReaderOf(reader, labelValue, _pathNodes);
        }

        /// <summary>
        /// This method retrieves an XmlReader within a specified context.
        /// Moreover it collects label values before or after a geometry could be found.
        /// </summary>
        /// <param name="reader">An XmlReader instance that is the origin of a created sub-reader</param>
        /// <param name="labelValue">A string array for recording a found label value. Pass 'null' to ignore searching for label values</param>
        /// <param name="pathNodes">A list of <see cref="IPathNode"/> instances defining the context of the retrieved reader</param>
        /// <returns>A sub-reader of the XmlReader given as argument</returns>
        protected XmlReader GetSubReaderOf(XmlReader reader, string[] labelValue, List<IPathNode> pathNodes)
        {
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
                        if (LabelNode != null)
                            if (LabelNode.Matches(reader))
                                labelValue[0] = reader.ReadElementString();


                    if (!ServiceExceptionNode.Matches(reader)) continue;

                    string errorMessage = reader.ReadInnerXml();
                    Trace.TraceError("A service exception occured: " + errorMessage);
                    throw new Exception("A service exception occured: " + errorMessage);
                }
            }

            return null;
        }

        /// <summary>
        /// This method adds a label to the collection.
        /// </summary>
        protected IFeature CreateFeature(Geometry geom, string labelField, string labelValue)
        {
            var feature = new Feature();
            if (labelField != null) feature[labelField] = labelValue;
            feature.Geometry = geom;
            return feature;
        }

        
        
        /// <summary>
        /// This method initializes the XmlReader member.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        private void CreateReader(HttpClientUtil httpClientUtil)
        {
            var xmlReaderSettings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true,
                DtdProcessing = DtdProcessing.Prohibit
            };
            XmlReader = XmlReader.Create(httpClientUtil.GetDataStream(), xmlReaderSettings);
        }

        /// <summary>
        /// This method initializes path nodes needed by the derived classes.
        /// </summary>
        private void InitializePathNodes()
        {
            IPathNode coordinatesNode = new PathNode("http://www.opengis.net/gml", "coordinates",
                                                     (NameTable) XmlReader.NameTable);
            IPathNode posListNode = new PathNode("http://www.opengis.net/gml", "posList",
                                                 (NameTable) XmlReader.NameTable);
            IPathNode ogcServiceExceptionNode = new PathNode("http://www.opengis.net/ogc", "ServiceException",
                                                             (NameTable) XmlReader.NameTable);
            IPathNode serviceExceptionNode = new PathNode("", "ServiceException", (NameTable) XmlReader.NameTable);
                //ServiceExceptions without ogc prefix are returned by deegree. PDD.
            IPathNode exceptionTextNode = new PathNode("http://www.opengis.net/ows", "ExceptionText",
                                                       (NameTable) XmlReader.NameTable);
            CoordinatesNode = new AlternativePathNodesCollection(coordinatesNode, posListNode);
            ServiceExceptionNode = new AlternativePathNodesCollection(ogcServiceExceptionNode, exceptionTextNode,
                                                                       serviceExceptionNode);
            FeatureNode = new PathNode(FeatureTypeInfo.FeatureTypeNamespace, FeatureTypeInfo.Name,
                                        (NameTable) XmlReader.NameTable);
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
            string decimalDel = string.IsNullOrEmpty(FeatureTypeInfo.DecimalDel) ? ":" : FeatureTypeInfo.DecimalDel;
            _cs = string.IsNullOrEmpty(FeatureTypeInfo.Cs) ? "," : FeatureTypeInfo.Cs;
            _ts = string.IsNullOrEmpty(FeatureTypeInfo.Ts) ? " " : FeatureTypeInfo.Ts;
            _formatInfo.NumberDecimalSeparator = decimalDel;
        }

        
        
        /// <summary>
        /// This method closes the XmlReader member and the used <see cref="HttpClientUtil"/> instance.
        /// </summary>
        public void Dispose()
        {
            if (XmlReader != null)
                XmlReader.Close();
            if (_httpClientUtil != null)
                _httpClientUtil.Close();
        }

            }

    /// <summary>
    /// This class produces instances of type <see cref="Mapsui.Geometries.Point"/>.
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
            FeatureNode.IsActive = false;
        }

        
        
        /// <summary>
        /// This method produces instances of type <see cref="Mapsui.Geometries.Point"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<Geometry> CreateGeometries(Features features)
        {
            IPathNode pointNode = new PathNode(Gmlns, "Point", (NameTable) XmlReader.NameTable);
            var labelValue = new string[1];
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((FeatureReader = GetSubReaderOf(XmlReader, null, FeatureNode)) != null)
                {
                    while ((GeomReader = GetSubReaderOf(FeatureReader, labelValue, pointNode, CoordinatesNode)) != null)
                    {
                        Geoms.Add(ParseCoordinates(GeomReader)[0]);
                        geomFound = true;
                    }
                    if (geomFound) features.Add(CreateFeature(Geoms[Geoms.Count - 1], FeatureTypeInfo.LableField, labelValue[0]));
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a point geometry string: " + ex.Message);
                throw;
            }

            return Geoms;
        }

            }

    /// <summary>
    /// This class produces instances of type <see cref="Mapsui.Geometries.LineString"/>.
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
            FeatureNode.IsActive = false;
        }

        
        
        /// <summary>
        /// This method produces instances of type <see cref="Mapsui.Geometries.LineString"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<Geometry> CreateGeometries(Features features)
        {
            IPathNode lineStringNode = new PathNode(Gmlns, "LineString", (NameTable) XmlReader.NameTable);
            var labelValue = new string[1];
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((FeatureReader = GetSubReaderOf(XmlReader, null, FeatureNode)) != null)
                {
                    while (
                        (GeomReader = GetSubReaderOf(FeatureReader, labelValue, lineStringNode, CoordinatesNode)) !=
                        null)
                    {
                        Geoms.Add(new LineString(ParseCoordinates(GeomReader)));
                        geomFound = true;
                    }
                    if (geomFound) features.Add(CreateFeature(Geoms[Geoms.Count - 1], FeatureTypeInfo.LableField, labelValue[0]));
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a line geometry string: " + ex.Message);
                throw;
            }

            return Geoms;
        }

            }

    /// <summary>
    /// This class produces instances of type <see cref="Mapsui.Geometries.Polygon"/>.
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
            FeatureNode.IsActive = false;
        }

        
        
        /// <summary>
        /// This method produces instances of type <see cref="Mapsui.Geometries.Polygon"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<Geometry> CreateGeometries(Features features)
        {
            IPathNode polygonNode = new PathNode(Gmlns, "Polygon", (NameTable) XmlReader.NameTable);
            IPathNode outerBoundaryNode = new PathNode(Gmlns, "outerBoundaryIs", (NameTable) XmlReader.NameTable);
            IPathNode exteriorNode = new PathNode(Gmlns, "exterior", (NameTable) XmlReader.NameTable);
            IPathNode outerBoundaryNodeAlt = new AlternativePathNodesCollection(outerBoundaryNode, exteriorNode);
            IPathNode innerBoundaryNode = new PathNode(Gmlns, "innerBoundaryIs", (NameTable) XmlReader.NameTable);
            IPathNode interiorNode = new PathNode(Gmlns, "interior", (NameTable) XmlReader.NameTable);
            IPathNode innerBoundaryNodeAlt = new AlternativePathNodesCollection(innerBoundaryNode, interiorNode);
            IPathNode linearRingNode = new PathNode(Gmlns, "LinearRing", (NameTable) XmlReader.NameTable);
            var labelValue = new string[1];
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((FeatureReader = GetSubReaderOf(XmlReader, null, FeatureNode)) != null)
                {
                    while ((GeomReader = GetSubReaderOf(FeatureReader, labelValue, polygonNode)) != null)
                    {
                        var polygon = new Polygon();

                        XmlReader outerBoundaryReader;
                        if (
                            (outerBoundaryReader =
                             GetSubReaderOf(GeomReader, null, outerBoundaryNodeAlt, linearRingNode, CoordinatesNode)) !=
                            null)
                            polygon.ExteriorRing = new LinearRing(ParseCoordinates(outerBoundaryReader));

                        XmlReader innerBoundariesReader;
                        while (
                            (innerBoundariesReader =
                             GetSubReaderOf(GeomReader, null, innerBoundaryNodeAlt, linearRingNode, CoordinatesNode)) !=
                            null)
                            polygon.InteriorRings.Add(new LinearRing(ParseCoordinates(innerBoundariesReader)));

                        Geoms.Add(polygon);
                        geomFound = true;
                    }
                    if (geomFound) features.Add(CreateFeature(Geoms[Geoms.Count - 1], FeatureTypeInfo.LableField, labelValue[0]));
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a polygon geometry: " + ex.Message);
                throw;
            }

            return Geoms;
        }

            }

    /// <summary>
    /// This class produces instances of type <see cref="Mapsui.Geometries.MultiPoint"/>.
    /// The base class is <see cref="GeometryFactory"/>.
    /// </summary>
    internal class MultiPointFactory : GeometryFactory
    {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPointFactory"/> class.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        internal MultiPointFactory(HttpClientUtil httpClientUtil, WfsFeatureTypeInfo featureTypeInfo) 
            : base(httpClientUtil, featureTypeInfo)
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

        
        
        /// <summary>
        /// This method produces instances of type <see cref="Mapsui.Geometries.MultiPoint"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<Geometry> CreateGeometries(Features features)
        {
            IPathNode multiPointNode = new PathNode(Gmlns, "MultiPoint", (NameTable) XmlReader.NameTable);
            IPathNode pointMemberNode = new PathNode(Gmlns, "pointMember", (NameTable) XmlReader.NameTable);
            var labelValue = new string[1];
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((FeatureReader = GetSubReaderOf(XmlReader, null, FeatureNode)) != null)
                {
                    while (
                        (GeomReader = GetSubReaderOf(FeatureReader, labelValue, multiPointNode, pointMemberNode)) !=
                        null)
                    {
                        var multiPoint = new MultiPoint();
                        GeometryFactory geomFactory = new PointFactory(GeomReader, FeatureTypeInfo);
                        Collection<Geometry> points = geomFactory.CreateGeometries(features);

                        foreach (var geometry in points)
                        {
                            var point = (Point) geometry;
                            multiPoint.Points.Add(point);
                        }

                        Geoms.Add(multiPoint);
                        geomFound = true;
                    }
                    if (geomFound) features.Add(CreateFeature(Geoms[Geoms.Count - 1], FeatureTypeInfo.LableField, labelValue[0]));
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a multi-point geometry: " + ex.Message);
                throw;
            }

            return Geoms;
        }

            }

    /// <summary>
    /// This class produces objects of type <see cref="Mapsui.Geometries.MultiLineString"/>.
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
        /// This method produces instances of type <see cref="Mapsui.Geometries.MultiLineString"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<Geometry> CreateGeometries(Features features)
        {
            IPathNode multiLineStringNode = new PathNode(Gmlns, "MultiLineString", (NameTable) XmlReader.NameTable);
            IPathNode multiCurveNode = new PathNode(Gmlns, "MultiCurve", (NameTable) XmlReader.NameTable);
            IPathNode multiLineStringNodeAlt = new AlternativePathNodesCollection(multiLineStringNode, multiCurveNode);
            IPathNode lineStringMemberNode = new PathNode(Gmlns, "lineStringMember", (NameTable) XmlReader.NameTable);
            IPathNode curveMemberNode = new PathNode(Gmlns, "curveMember", (NameTable) XmlReader.NameTable);
            IPathNode lineStringMemberNodeAlt = new AlternativePathNodesCollection(lineStringMemberNode, curveMemberNode);
            var labelValue = new string[1];
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((FeatureReader = GetSubReaderOf(XmlReader, null, FeatureNode)) != null)
                {
                    while (
                        (GeomReader =
                         GetSubReaderOf(FeatureReader, labelValue, multiLineStringNodeAlt, lineStringMemberNodeAlt)) !=
                        null)
                    {
                        var multiLineString = new MultiLineString();
                        GeometryFactory geomFactory = new LineStringFactory(GeomReader, FeatureTypeInfo);
                        Collection<Geometry> lineStrings = geomFactory.CreateGeometries(features);

                        foreach (var geometry in lineStrings)
                        {
                            var lineString = (LineString) geometry;
                            multiLineString.LineStrings.Add(lineString);
                        }

                        Geoms.Add(multiLineString);
                        geomFound = true;
                    }
                    if (geomFound) features.Add(CreateFeature(Geoms[Geoms.Count - 1], FeatureTypeInfo.LableField, labelValue[0]));
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a multi-lineString geometry: " + ex.Message);
                throw;
            }

            return Geoms;
        }

            }

    /// <summary>
    /// This class produces instances of type <see cref="Mapsui.Geometries.MultiPolygon"/>.
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
        /// This method produces instances of type <see cref="Mapsui.Geometries.MultiPolygon"/>.
        /// </summary>
        /// <returns>The created geometries</returns>
        internal override Collection<Geometry> CreateGeometries(Features features)
        {
            IPathNode multiPolygonNode = new PathNode(Gmlns, "MultiPolygon", (NameTable) XmlReader.NameTable);
            IPathNode multiSurfaceNode = new PathNode(Gmlns, "MultiSurface", (NameTable) XmlReader.NameTable);
            IPathNode multiPolygonNodeAlt = new AlternativePathNodesCollection(multiPolygonNode, multiSurfaceNode);
            IPathNode polygonMemberNode = new PathNode(Gmlns, "polygonMember", (NameTable) XmlReader.NameTable);
            IPathNode surfaceMemberNode = new PathNode(Gmlns, "surfaceMember", (NameTable) XmlReader.NameTable);
            IPathNode polygonMemberNodeAlt = new AlternativePathNodesCollection(polygonMemberNode, surfaceMemberNode);
            IPathNode linearRingNode = new PathNode(Gmlns, "LinearRing", (NameTable) XmlReader.NameTable);
            var labelValue = new string[1];
            bool geomFound = false;

            try
            {
                // Reading the entire feature's node makes it possible to collect label values that may appear before or after the geometry property
                while ((FeatureReader = GetSubReaderOf(XmlReader, null, FeatureNode)) != null)
                {
                    while (
                        (GeomReader =
                         GetSubReaderOf(FeatureReader, labelValue, multiPolygonNodeAlt, polygonMemberNodeAlt)) != null)
                    {
                        var multiPolygon = new MultiPolygon();
                        GeometryFactory geomFactory = new PolygonFactory(GeomReader, FeatureTypeInfo);
                        Collection<Geometry> polygons = geomFactory.CreateGeometries(features);

                        foreach (var geometry in polygons)
                        {
                            var polygon = (Polygon) geometry;
                            multiPolygon.Polygons.Add(polygon);
                        }

                        Geoms.Add(multiPolygon);
                        geomFound = true;
                    }
                    if (geomFound) features.Add(CreateFeature(Geoms[Geoms.Count - 1], FeatureTypeInfo.LableField, labelValue[0]));
                    geomFound = false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while parsing a multi-polygon geometry: " + ex.Message);
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
        internal override Collection<Geometry> CreateGeometries(Features features)
        {
            GeometryFactory geomFactory = null;

            string geometryTypeString = string.Empty;

            if (_quickGeometries) _multiGeometries = false;

            IPathNode pointNode = new PathNode(Gmlns, "Point", (NameTable) XmlReader.NameTable);
            IPathNode lineStringNode = new PathNode(Gmlns, "LineString", (NameTable) XmlReader.NameTable);
            IPathNode polygonNode = new PathNode(Gmlns, "Polygon", (NameTable) XmlReader.NameTable);
            IPathNode multiPointNode = new PathNode(Gmlns, "MultiPoint", (NameTable) XmlReader.NameTable);
            IPathNode multiLineStringNode = new PathNode(Gmlns, "MultiLineString", (NameTable) XmlReader.NameTable);
            IPathNode multiCurveNode = new PathNode(Gmlns, "MultiCurve", (NameTable) XmlReader.NameTable);
            IPathNode multiLineStringNodeAlt = new AlternativePathNodesCollection(multiLineStringNode, multiCurveNode);
            IPathNode multiPolygonNode = new PathNode(Gmlns, "MultiPolygon", (NameTable) XmlReader.NameTable);
            IPathNode multiSurfaceNode = new PathNode(Gmlns, "MultiSurface", (NameTable) XmlReader.NameTable);
            IPathNode multiPolygonNodeAlt = new AlternativePathNodesCollection(multiPolygonNode, multiSurfaceNode);

            while (XmlReader.Read())
            {
                if (XmlReader.NodeType == XmlNodeType.Element)
                {
                    if (_multiGeometries)
                    {
                        if (multiPointNode.Matches(XmlReader))
                        {
                            geomFactory = new MultiPointFactory(_httpClientUtil, FeatureTypeInfo);
                            geometryTypeString = "MultiPointPropertyType";
                            break;
                        }
                        if (multiLineStringNodeAlt.Matches(XmlReader))
                        {
                            geomFactory = new MultiLineStringFactory(_httpClientUtil, FeatureTypeInfo);
                            geometryTypeString = "MultiLineStringPropertyType";
                            break;
                        }
                        if (multiPolygonNodeAlt.Matches(XmlReader))
                        {
                            geomFactory = new MultiPolygonFactory(_httpClientUtil, FeatureTypeInfo);
                            geometryTypeString = "MultiPolygonPropertyType";
                            break;
                        }
                    }

                    if (pointNode.Matches(XmlReader))
                    {
                        geomFactory = new PointFactory(_httpClientUtil, FeatureTypeInfo);
                        geometryTypeString = "PointPropertyType";
                        break;
                    }
                    if (lineStringNode.Matches(XmlReader))
                    {
                        geomFactory = new LineStringFactory(_httpClientUtil, FeatureTypeInfo);
                        geometryTypeString = "LineStringPropertyType";
                        break;
                    }
                    if (polygonNode.Matches(XmlReader))
                    {
                        geomFactory = new PolygonFactory(_httpClientUtil, FeatureTypeInfo);
                        geometryTypeString = "PolygonPropertyType";
                        break;
                    }
                    if (ServiceExceptionNode.Matches(XmlReader))
                    {
                        string serviceException = XmlReader.ReadInnerXml();
                        Trace.TraceError("A service exception occured: " + serviceException);
                        throw new Exception("A service exception occured: " + serviceException);
                    }
                }
            }

            FeatureTypeInfo.Geometry.GeometryType = geometryTypeString;

            if (geomFactory == null) return Geoms;
            geomFactory.CreateGeometries(features);
            return Geoms;
        }
    }
}